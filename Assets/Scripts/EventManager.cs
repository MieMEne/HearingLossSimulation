using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class StoryEvent
{
    [Header("Event Info")]
    public string eventName;

    [Header("Audio")]
    public List<AudioSource> speakerSources = new List<AudioSource>(); // multiple characters
    public float timeBetweenClips = 0.5f;

    [Header("Animations")]
    public List<TalkingAnimations> talkingAnimations = new List<TalkingAnimations>(); // multiple animators

    [Header("UI")]
    public GameObject uiToShow;
    public enum UIType { None, Timed, Choice }
    public UIType uiType = UIType.None;
    public float uiDuration = 3f; // used only if Timed

    [Header("Choice Branching (for Choice UI only)")]
    [Tooltip("Index of next event if first choice is pressed (-1 = continue normally)")]
    public int nextEventIfChoice0 = -1;
    [Tooltip("Index of next event if second choice is pressed (-1 = continue normally)")]
    public int nextEventIfChoice1 = -1;

    [Header("Special Actions")]
    public WaiterWalking waiterToStart;

    [Header("Timing")]
    public float waitTimeAfter = 1f; // delay after this event
}

public class EventManager : MonoBehaviour
{
    [Header("Story Events")]
    public StoryEvent[] events;
    private int currentIndex = 0;

    void Start()
    {
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

        // ensure animators are ready
        yield return null;

        // ---- Talking animations ----
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

            // Wait for longest clip
            float maxLength = 0f;
            foreach (AudioSource source in e.speakerSources)
            {
                if (source != null && source.clip != null)
                    maxLength = Mathf.Max(maxLength, source.clip.length);
            }

            yield return new WaitForSeconds(maxLength + e.timeBetweenClips);
        }

        // ---- UI ----
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
                        Debug.Log($"Player chose option 0 → jumping to event index {e.nextEventIfChoice0}");
                        currentIndex = e.nextEventIfChoice0 - 1; // -1 because PlayStory adds +1
                    }
                    else if (choice.chosenIndex == 1 && e.nextEventIfChoice1 >= 0)
                    {
                        Debug.Log($"Player chose option 1 → jumping to event index {e.nextEventIfChoice1}");
                        currentIndex = e.nextEventIfChoice1 - 1;
                    }
                    else
                    {
                        Debug.Log("No special branch; continuing sequentially");
                    }
                }
            }
        }

        // ---- Waiter Action ----
        if (e.waiterToStart != null)
        {
            e.waiterToStart.StartWalking();
            yield return new WaitUntil(() => e.waiterToStart.IsWalking() == false);
        }

        // ---- Wait after ----
        if (e.waitTimeAfter > 0)
            yield return new WaitForSeconds(e.waitTimeAfter);
    }
}
