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

    public enum UIType { None, Timed, Choice }

    [Header("UI")]
    public GameObject uiToShow;
    public UIType uiType = UIType.None;
    public float uiDuration = 3f; // only used if UIType.Timed

    [Header("Special Actions")]
    public WaiterWalking waiterToStart;

    [Header("Timing")]
    public float waitTimeAfter = 1f; // new: delay after this event
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

        // Play all audio sources simultaneously
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

        // Show UI if assigned
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
                }
            }
        }

        // Trigger waiter walking if assigned
        if (e.waiterToStart != null)
        {
            e.waiterToStart.StartWalking();

            // Wait until waiter has finished walking
            yield return new WaitUntil(() => e.waiterToStart.IsWalking() == false);
        }

        // Wait additional time if set
        if (e.waitTimeAfter > 0)
            yield return new WaitForSeconds(e.waitTimeAfter);
    }
}
