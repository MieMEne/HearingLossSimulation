using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.SceneManagement;

[System.Serializable]
public class StoryEvent
{
    [Header("Event Info")]
    public string eventName;

    [Header("Audio (In-Scene Speakers)")]
    public List<AudioSource> speakerSources = new List<AudioSource>();
    public float timeBetweenClips = 0.5f;

    [Header("Optional Narrator Line (Plays with Timed UI)")]
    public AudioClip narratorClip;
    public float narratorVolume = 1f;

    [Header("Animations")]
    public List<TalkingAnimations> talkingAnimations = new List<TalkingAnimations>();
    public List<TalkingAnimations2> talkingAnimations2 = new List<TalkingAnimations2>();

    [Header("UI")]
    public GameObject uiToShow;
    public enum UIType { None, Timed, Choice }
    public UIType uiType = UIType.None;
    public float uiDuration = 3f;

    [Header("Choice Branching (for Choice UI only)")]
    public int nextEventIfChoice0 = -1;
    public int nextEventIfChoice1 = -1;

    [Header("Grabbable (scene object OR prefab)")]
    public GameObject grabbableObject;
    public GameObject grabbablePrefab;
    public Transform spawnPoint;
    public bool parentToSpawnPoint = false;
    public float eventDuration = 0f;
    public bool keepInteractableAfterEvent = false;
    public bool showGrabbableAtEventStart = false;

    [Header("Special Actions")]
    public WaiterWalking waiterToStart;
    [Tooltip("If true, the waiter walks backward along the targets (return). If false, walks forward (leaving).")]
    public bool reverseWaiterMovement = false;

    [Header("Timing")]
    public float waitTimeAfter = 1f;
}

public class EventManager : MonoBehaviour
{
    [Header("Story Events")]
    public StoryEvent[] events;
    private int currentIndex = 0;

    [Header("After Story Finishes")]
    public bool loadNextScene = false;
    public string nextSceneName = "";

    private readonly Dictionary<GameObject, List<XRBaseInteractable>> _interactablesByObject =
        new Dictionary<GameObject, List<XRBaseInteractable>>();
    private readonly Dictionary<XRBaseInteractable, InteractionLayerMask> _originalLayers =
        new Dictionary<XRBaseInteractable, InteractionLayerMask>();
    private readonly HashSet<GameObject> _spawnedAtRuntime = new HashSet<GameObject>();

    // === One-shot story + load guards (NEW) ===
    private bool _storyStarted = false;     // NEW
    private bool _storyFinished = false;    // NEW
    private bool _isLoadingNextScene = false; // NEW

    void Start()
    {
        // Guard against double-start (NEW)
        if (_storyStarted) return; // NEW
        _storyStarted = true;      // NEW

        foreach (var e in events)
        {
            if (e.grabbableObject != null)
                CacheAndHardLock(e.grabbableObject, alsoHide: false);
        }

        StartCoroutine(PlayStory());
    }

    IEnumerator PlayStory()
    {
        while (currentIndex < events.Length)
        {
            yield return StartCoroutine(RunEvent(events[currentIndex]));
            currentIndex++;
        }

        Debug.Log("🎉 Story finished!");
        _storyFinished = true; // NEW

        // Reliable async load with guard (NEW)
        if (loadNextScene && !string.IsNullOrEmpty(nextSceneName) && !_isLoadingNextScene)
        {
            _isLoadingNextScene = true;
            StartCoroutine(LoadNextSceneReliable(nextSceneName)); // NEW
        }
        else
        {
            if (loadNextScene && string.IsNullOrEmpty(nextSceneName))
                Debug.LogWarning("[EventManager] loadNextScene is true but nextSceneName is empty."); // NEW
        }
    }

    IEnumerator RunEvent(StoryEvent e)
    {
        Debug.Log("▶ Running event: " + e.eventName);
        yield return null;

        // Trigger animations
        if (e.talkingAnimations != null)
            foreach (var anim in e.talkingAnimations) anim?.PlaySequence();

        if (e.talkingAnimations2 != null)
            foreach (var anim in e.talkingAnimations2) anim?.PlaySequence();

        // Handle grabbables early if needed
        GameObject earlyGo = null;
        bool grabbableAlreadyActivated = false;
        if (e.showGrabbableAtEventStart && (e.grabbableObject != null || e.grabbablePrefab != null))
        {
            earlyGo = PrepareAndEnableGrabbable(e);
            grabbableAlreadyActivated = (earlyGo != null);
        }

        // Handle speaker audio sources (blocking as before)
        if (e.speakerSources != null && e.speakerSources.Count > 0)
        {
            foreach (var source in e.speakerSources)
                if (source != null && source.clip != null)
                    source.Play();

            float maxLength = 0f;
            foreach (var source in e.speakerSources)
                if (source != null && source.clip != null)
                    maxLength = Mathf.Max(maxLength, source.clip.length);

            yield return new WaitForSeconds(maxLength + e.timeBetweenClips);
        }

        // Handle UI + narrator
        if (e.uiToShow != null)
        {
            e.uiToShow.SetActive(true);

            if (e.uiType == StoryEvent.UIType.Timed)
            {
                // Start narrator if present
                float narratorLength = 0f;
                if (e.narratorClip != null)
                {
                    AudioSource narratorSource = new GameObject("TempNarratorSource").AddComponent<AudioSource>();
                    narratorSource.clip = e.narratorClip;
                    narratorSource.volume = e.narratorVolume;
                    narratorSource.Play();
                    narratorLength = e.narratorClip.length;
                    Destroy(narratorSource.gameObject, narratorLength + 1f);
                }

                float waitTime = Mathf.Max(e.uiDuration, narratorLength);
                yield return new WaitForSeconds(waitTime);

                e.uiToShow.SetActive(false);
            }
            else if (e.uiType == StoryEvent.UIType.Choice)
            {
                ChoiceUI choice = e.uiToShow.GetComponent<ChoiceUI>();
                if (choice != null)
                {
                    choice.ResetChoice();
                    yield return new WaitUntil(() => choice.buttonPressed);
                    e.uiToShow.SetActive(false);

                    if (choice.chosenIndex == 0 && e.nextEventIfChoice0 >= 0)
                    {
                        currentIndex = e.nextEventIfChoice0 - 1;
                        yield break;
                    }
                    else if (choice.chosenIndex == 1 && e.nextEventIfChoice1 >= 0)
                    {
                        currentIndex = e.nextEventIfChoice1 - 1;
                        yield break;
                    }
                }
            }
        }
        else
        {
            // No UI, but narrator clip exists → play narrator alone
            if (e.narratorClip != null)
            {
                AudioSource narratorSource = new GameObject("TempNarratorSource").AddComponent<AudioSource>();
                narratorSource.clip = e.narratorClip;
                narratorSource.volume = e.narratorVolume;
                narratorSource.Play();
                float narratorLength = e.narratorClip.length;
                Destroy(narratorSource.gameObject, narratorLength + 1f);

                yield return new WaitForSeconds(narratorLength);
            }
        }

        // Handle grabbable objects
        if (e.grabbableObject != null || e.grabbablePrefab != null)
        {
            GameObject go = grabbableAlreadyActivated ? (earlyGo ?? e.grabbableObject) : PrepareAndEnableGrabbable(e);

            if (go != null)
            {
                GrabbableEventObject grabObj = go.GetComponent<GrabbableEventObject>();
                grabObj?.ResetGrabState();

                if (grabObj != null)
                {
                    Debug.Log($"Waiting for {go.name} to be grabbed...");
                    yield return new WaitUntil(() => grabObj.HasBeenGrabbed);
                    Debug.Log($"{go.name} has been grabbed!");

                    if (!e.keepInteractableAfterEvent)
                        EnableAndRestore(go, enable: false);
                }
                else
                {
                    EnableAndRestore(go, enable: true);
                }
            }
        }

        // Handle waiter walking
        if (e.waiterToStart != null)
        {
            e.waiterToStart.StartWalking(e.reverseWaiterMovement);
            yield return new WaitUntil(() => !e.waiterToStart.IsWalking());
        }

        // Wait after event if specified
        if (e.waitTimeAfter > 0)
            yield return new WaitForSeconds(e.waitTimeAfter);

        // Disable next event grabbable if needed
        if (events != null && currentIndex < events.Length)
        {
            var cur = events[currentIndex];
            if (cur.grabbableObject != null && !cur.keepInteractableAfterEvent)
                EnableAndRestore(cur.grabbableObject, enable: false);
        }
    }

    private GameObject PrepareAndEnableGrabbable(StoryEvent e)
    {
        GameObject go = e.grabbableObject;

        if (go == null && e.grabbablePrefab != null)
        {
            Vector3 pos = transform.position;
            Quaternion rot = transform.rotation;

            if (e.spawnPoint != null)
            {
                pos = e.spawnPoint.position;
                rot = e.spawnPoint.rotation;
            }

            go = Instantiate(e.grabbablePrefab, pos, rot);
            if (e.parentToSpawnPoint && e.spawnPoint != null)
                go.transform.SetParent(e.spawnPoint, worldPositionStays: true);

            _spawnedAtRuntime.Add(go);
            e.grabbableObject = go;

            CacheAndHardLock(go, alsoHide: false);
        }

        if (go != null)
        {
            if (!go.activeSelf) go.SetActive(true);
            EnableAndRestore(go, enable: true);
        }

        return go;
    }

    private void CacheAndHardLock(GameObject go, bool alsoHide)
    {
        if (go == null) return;

        // Cache all XRBaseInteractables in the object and children
        if (!_interactablesByObject.ContainsKey(go))
        {
            var list = new List<XRBaseInteractable>();
            go.GetComponentsInChildren(true, list);
            _interactablesByObject[go] = list;

            foreach (var xri in list)
            {
                if (xri == null) continue;
                // Save original interaction layers
                if (!_originalLayers.ContainsKey(xri))
                    _originalLayers[xri] = xri.interactionLayers;
            }
        }

        // Disable interactables
        EnableAndRestore(go, enable: false);

        // Optionally hide the object itself
        if (alsoHide) go.SetActive(false);
    }

    private void EnableAndRestore(GameObject go, bool enable)
    {
        if (go == null) return;

        if (!_interactablesByObject.TryGetValue(go, out var list) || list == null) return;

        foreach (var xri in list)
        {
            if (xri == null) continue;

            if (enable)
            {
                // Restore original interaction layers
                if (_originalLayers.TryGetValue(xri, out var mask))
                    xri.interactionLayers = mask;
                xri.enabled = true;
            }
            else
            {
                xri.enabled = false;
                xri.interactionLayers = 0;
            }
        }
    }

    // ===== Reliable async scene loading (NEW) =====
    private IEnumerator LoadNextSceneReliable(string sceneName)
    {
        // Verify the scene exists in Build Settings
        bool found = false;
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            var path = SceneUtility.GetScenePathByBuildIndex(i);
            var name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (name == sceneName) { found = true; break; }
        }
        if (!found)
        {
            Debug.LogError($"[EventManager] Next scene '{sceneName}' is NOT in Build Settings (File → Build Settings → Scenes In Build).");
            yield break;
        }

        Debug.Log($"[EventManager] Loading next scene: {sceneName} …");
        var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        if (op == null)
        {
            Debug.LogError($"[EventManager] LoadSceneAsync returned null for '{sceneName}'.");
            yield break;
        }

        op.allowSceneActivation = true;
        while (!op.isDone)
            yield return null;

        Debug.Log($"[EventManager] Scene '{sceneName}' loaded.");
    }

#if UNITY_EDITOR
    // Editor-only validation to catch typos/missing scenes (NEW)
    private void OnValidate()
    {
        if (loadNextScene && !string.IsNullOrEmpty(nextSceneName))
        {
            bool found = false;
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                var path = SceneUtility.GetScenePathByBuildIndex(i);
                var name = System.IO.Path.GetFileNameWithoutExtension(path);
                if (name == nextSceneName) { found = true; break; }
            }
            if (!found)
            {
                Debug.LogError($"[EventManager] Next scene '{nextSceneName}' is not in Build Settings!");
            }
        }
    }
#endif
}
