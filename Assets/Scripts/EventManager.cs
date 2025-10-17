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

    [Header("Grabbable Object (optional)")]
    [Tooltip("An object that becomes interactable only during this event")]
    public GameObject grabbableObject;
    [Tooltip("Time allowed for interaction before moving to next event (seconds). 0 = wait until grabbed")]
    public float eventDuration = 0f;

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

    void Start()
    {
        // Lock all grabbable objects at start
        foreach (var e in events)
        {
            if (e.grabbableObject != null)
            {
                XRGrabInteractable grabInteractable = e.grabbableObject.GetComponent<XRGrabInteractable>();
                if (grabInteractable != null) grabInteractable.enabled = false;
            }
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
            {
                if (anim != null)
                    anim.PlaySequence();
            }
        }

        // ---- Audio ----
        if (e.speakerSources != null && e.speakerSources.Count > 0)
        {
            foreach (AudioSource source in e.speakerSources)
            {
                if (source != null && source.clip != null)
                    source.Play();
            }

            float maxLength = 0f;
            foreach (AudioSource source in e.speakerSources)
            {
                if (source != null && source.clip != null)
                    maxLength = Mathf.Max(maxLength, source.clip.length);
            }

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
        if (e.grabbableObject != null)
        {
            e.grabbableObject.SetActive(true);

            XRGrabInteractable grabInteractable = e.grabbableObject.GetComponent<XRGrabInteractable>();
            GrabbableEventObject grabObj = e.grabbableObject.GetComponent<GrabbableEventObject>();

            if (grabObj != null) grabObj.ResetGrabState();

            // Enable interaction only while event is active
            if (grabInteractable != null) grabInteractable.enabled = true;

            if (e.eventDuration > 0f)
            {
                float timer = 0f;
                while ((grabObj == null || !grabObj.HasBeenGrabbed) && timer < e.eventDuration)
                {
                    timer += Time.deltaTime;
                    yield return null;
                }

                // Lock the object after duration
                if (grabInteractable != null) grabInteractable.enabled = false;

                if (grabObj != null && grabObj.HasBeenGrabbed)
                    Debug.Log($"{e.grabbableObject.name} was grabbed during the allowed time.");
                else
                    Debug.Log($"{e.grabbableObject.name} interaction time expired.");
            }
            else if (grabObj != null)
            {
                yield return new WaitUntil(() => grabObj.HasBeenGrabbed);
                Debug.Log($"{e.grabbableObject.name} was grabbed!");
            }

            // The object stays visible → DO NOT deactivate
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
    }
}
