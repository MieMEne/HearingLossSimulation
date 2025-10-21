using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.SceneManagement; // ✅ Added for scene shifting

[System.Serializable]
public class StoryEvent
{
    [Header("Event Info")]
    public string eventName;

    [Header("Audio")]
    public List<AudioSource> speakerSources = new List<AudioSource>();
    public float timeBetweenClips = 0.5f;

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

    // ✅ New scene transition options
    [Header("After Story Finishes")]
    public bool loadNextScene = false;
    public string nextSceneName = ""; // Must match a scene in Build Settings

    private readonly Dictionary<GameObject, List<XRBaseInteractable>> _interactablesByObject = new();
    private readonly Dictionary<XRBaseInteractable, InteractionLayerMask> _originalLayers = new();
    private readonly HashSet<GameObject> _spawnedAtRuntime = new();

    void Start()
    {
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

        // ✅ If enabled, load next scene
        if (loadNextScene && !string.IsNullOrEmpty(nextSceneName))
        {
            Debug.Log($"📦 Loading scene: {nextSceneName}");
            SceneManager.LoadScene(nextSceneName);
        }
    }

    IEnumerator RunEvent(StoryEvent e)
    {
        Debug.Log("▶ Running event: " + e.eventName);
        yield return null;

        if (e.talkingAnimations != null && e.talkingAnimations.Count > 0)
        {
            foreach (var anim in e.talkingAnimations)
                anim?.PlaySequence();
        }

        if (e.talkingAnimations2 != null && e.talkingAnimations2.Count > 0)
        {
            foreach (var anim in e.talkingAnimations2)
                anim?.PlaySequence();
        }

        GameObject earlyGo = null;
        bool grabbableAlreadyActivated = false;
        if (e.showGrabbableAtEventStart && (e.grabbableObject != null || e.grabbablePrefab != null))
        {
            earlyGo = PrepareAndEnableGrabbable(e);
            grabbableAlreadyActivated = (earlyGo != null);
        }

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

        if (e.uiToShow != null)
        {
            e.uiToShow.SetActive(true);

            if (e.uiType == StoryEvent.UIType.Timed)
            {
                yield return new WaitForSeconds(e.uiDuration);
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

        if (e.waiterToStart != null)
        {
            e.waiterToStart.StartWalking(e.reverseWaiterMovement);
            yield return new WaitUntil(() => !e.waiterToStart.IsWalking());
        }

        if (e.waitTimeAfter > 0)
            yield return new WaitForSeconds(e.waitTimeAfter);

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

        if (!_interactablesByObject.ContainsKey(go))
        {
            var list = new List<XRBaseInteractable>();
            go.GetComponentsInChildren(true, list);
            _interactablesByObject[go] = list;

            foreach (var xri in list)
            {
                if (xri == null) continue;
                if (!_originalLayers.ContainsKey(xri))
                    _originalLayers[xri] = xri.interactionLayers;
            }
        }

        EnableAndRestore(go, enable: false);

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
}
