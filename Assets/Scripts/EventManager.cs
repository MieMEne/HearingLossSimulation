using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

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

    [Header("UI")]
    public GameObject uiToShow;
    public enum UIType { None, Timed, Choice }
    public UIType uiType = UIType.None;
    public float uiDuration = 3f;

    [Header("Choice Branching (for Choice UI only)")]
    public int nextEventIfChoice0 = -1;
    public int nextEventIfChoice1 = -1;

    [Header("Grabbable (scene object OR prefab)")]
    [Tooltip("OPTION 1: Drag a SCENE object here if it already exists in the scene.")]
    public GameObject grabbableObject;            // existing scene instance (optional)

    [Tooltip("OPTION 2: Drag a PREFAB here to have it spawned at runtime.")]
    public GameObject grabbablePrefab;            // prefab reference (optional)

    [Tooltip("Where to spawn the prefab (position/rotation will be copied). Leave empty to use EventManager transform.")]
    public Transform spawnPoint;

    [Tooltip("Make spawned object a child of the Spawn Point (keeps local offset 0/0/0).")]
    public bool parentToSpawnPoint = false;

    [Tooltip("Time allowed for interaction before moving to next event (seconds). 0 = wait until grabbed")]
    public float eventDuration = 0f;

    [Tooltip("If true, the grabbable remains interactable after the event ends")]
    public bool keepInteractableAfterEvent = false;

    [Tooltip("If true, spawn/enable the grabbable at the START of the event (so it's visible during the UI).")]
    public bool showGrabbableAtEventStart = false; // NEW

    [Header("Special Actions")]
    public WaiterWalking waiterToStart;

    [Header("Timing")]
    public float waitTimeAfter = 1f;
}

public class EventManager : MonoBehaviour
{
    [Header("Story Events")]
    public StoryEvent[] events;
    private int currentIndex = 0;

    // Caches for interactables on each object (works for Grab/Simple/Socket/etc., incl. children)
    private readonly Dictionary<GameObject, List<XRBaseInteractable>> _interactablesByObject = new();
    private readonly Dictionary<XRBaseInteractable, InteractionLayerMask> _originalLayers = new();

    // Track runtime-spawned instances so we can cache/lock them too
    private readonly HashSet<GameObject> _spawnedAtRuntime = new();

    void Start()
    {
        // Build caches and hard-lock any SCENE INSTANCES set in the events
        foreach (var e in events)
        {
            if (e.grabbableObject == null) continue;
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
    }

    IEnumerator RunEvent(StoryEvent e)
    {
        Debug.Log("▶ Running event: " + e.eventName);

        yield return null;

        // ---- Talking Animations ----
        if (e.talkingAnimations != null && e.talkingAnimations.Count > 0)
        {
            foreach (var anim in e.talkingAnimations)
                if (anim != null) anim.PlaySequence();
        }

        // ======= NEW: (optional) show grabbable at START so it's visible during UI =======
        GameObject earlyGo = null;
        bool grabbableAlreadyActivated = false;
        if (e.showGrabbableAtEventStart && (e.grabbableObject != null || e.grabbablePrefab != null))
        {
            earlyGo = PrepareAndEnableGrabbable(e);     // spawn or fetch, set active, enable interactables
            grabbableAlreadyActivated = (earlyGo != null);
        }
        // ================================================================================

        // ---- Audio ----
        if (e.speakerSources != null && e.speakerSources.Count > 0)
        {
            foreach (AudioSource source in e.speakerSources)
                if (source != null && source.clip != null) source.Play();

            float maxLength = 0f;
            foreach (AudioSource source in e.speakerSources)
                if (source != null && source.clip != null) maxLength = Mathf.Max(maxLength, source.clip.length);

            yield return new WaitForSeconds(maxLength + e.timeBetweenClips);
        }

        // ---- UI Interaction ----
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

                    // ---- Branching Logic ----
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

        // ---- Grabbable Object Interaction ----
        // If we already activated it early, skip the activation part and only handle waiting/relocking.
        if (e.grabbableObject != null || e.grabbablePrefab != null)
        {
            GameObject go = null;

            if (grabbableAlreadyActivated)
            {
                go = earlyGo != null ? earlyGo : e.grabbableObject;
            }
            else
            {
                // original behavior: activate AFTER the UI
                go = PrepareAndEnableGrabbable(e);
            }

            if (go != null)
            {
                XRGrabInteractable grabInteractable = go.GetComponent<XRGrabInteractable>();
                GrabbableEventObject grabObj = go.GetComponent<GrabbableEventObject>();
                if (grabObj != null) grabObj.ResetGrabState();

                if (e.eventDuration > 0f)
                {
                    float timer = 0f;
                    while ((grabObj == null || !grabObj.HasBeenGrabbed) && timer < e.eventDuration)
                    {
                        timer += Time.deltaTime;
                        yield return null;
                    }

                    if (!e.keepInteractableAfterEvent)
                        EnableAndRestore(go, enable: false); // relock

                    if (grabObj != null && grabObj.HasBeenGrabbed)
                        Debug.Log($"{go.name} was grabbed during the allowed time.");
                    else
                        Debug.Log($"{go.name} interaction time expired.");
                }
                else if (grabObj != null)
                {
                    // Wait until grabbed
                    yield return new WaitUntil(() => grabObj.HasBeenGrabbed);
                    Debug.Log($"{go.name} was grabbed!");

                    if (!e.keepInteractableAfterEvent)
                        EnableAndRestore(go, enable: false); // relock after successful grab
                }
                else
                {
                    // No GrabbableEventObject: stays enabled during this event;
                    // final safety below applies if keepInteractableAfterEvent == false
                }
            }
        }

        // ---- Waiter Walking ----
        if (e.waiterToStart != null)
        {
            e.waiterToStart.StartWalking();
            yield return new WaitUntil(() => !e.waiterToStart.IsWalking());
        }

        // ---- Wait time after ----
        if (e.waitTimeAfter > 0)
            yield return new WaitForSeconds(e.waitTimeAfter);

        // Final safety — enforce post-event interactable state
        if (events != null && currentIndex < events.Length)
        {
            var cur = events[currentIndex];
            if (cur.grabbableObject != null && !cur.keepInteractableAfterEvent)
                EnableAndRestore(cur.grabbableObject, enable: false);
        }
    }

    // ====================== helpers ======================

    // Prepare a grabbable for an event: spawn prefab if needed, set active, and enable interactables.
    private GameObject PrepareAndEnableGrabbable(StoryEvent e)
    {
        GameObject go = e.grabbableObject;

        // If no scene object, but a prefab is provided, spawn it
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
            e.grabbableObject = go; // store for later events

            CacheAndHardLock(go, alsoHide: false);
        }

        if (go != null)
        {
            if (!go.activeSelf) go.SetActive(true);
            EnableAndRestore(go, enable: true); // enable + restore layers
        }

        return go;
    }

    // Build cache for all XRBaseInteractables under 'go' and hard lock them (disable + set layers None).
    private void CacheAndHardLock(GameObject go, bool alsoHide)
    {
        if (go == null) return;

        if (!_interactablesByObject.ContainsKey(go))
        {
            var list = new List<XRBaseInteractable>();
            go.GetComponentsInChildren(true, list); // include inactive
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

    // Enable/disable all XRBaseInteractables under 'go' and restore/clear their interaction layers.
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
                xri.interactionLayers = 0; // None
            }
        }
    }
}
