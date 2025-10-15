using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class StoryEvent
{
    [Header("General Info")]
    public string eventName;

    [Header("Audio")]
    public List<AudioSource> speakerSources = new List<AudioSource>(); // multiple characters
    public float timeBetweenClips = 0.5f;                              // optional delay after audio

   [Header("UI")]
    public GameObject uiToShow;
    public float uiDuration = 3f; // duration the UI stays active

    [Header("Character Movement")]
    public GameObject actorToMove;
    public Transform targetPosition;
    public float moveDuration = 2f;

    [Header("Special Actions")]
    public WaiterWalking waiterToStart; // special scripts like waiter walking

    [Header("Timing")]
    public float waitTimeBefore = 0f;
    public float waitTimeAfter = 0.5f;
}

public class EventManager : MonoBehaviour
{
    [Header("Story Setup")]
    public StoryEvent[] events; // list of story events
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

        // Optional delay before event
        if (e.waitTimeBefore > 0)
            yield return new WaitForSeconds(e.waitTimeBefore);

        // 🎙️ Play all AudioSources simultaneously
        if (e.speakerSources != null && e.speakerSources.Count > 0)
        {
            foreach (AudioSource source in e.speakerSources)
            {
                if (source != null && source.clip != null)
                    source.Play();
            }

            // Wait until the longest clip finishes
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

            // Wait for specified duration
            yield return new WaitForSeconds(e.uiDuration);

            // Hide UI
            e.uiToShow.SetActive(false);
        }


        // 🚶 Move actor if assigned
        if (e.actorToMove != null && e.targetPosition != null)
        {
            yield return StartCoroutine(MoveActor(e.actorToMove, e.targetPosition.position, e.moveDuration));
        }

        // 🧑‍🍳 Trigger special actions like WaiterWalking
        if (e.waiterToStart != null)
        {
            e.waiterToStart.StartWalking();
        }

        // Optional delay after event
        if (e.waitTimeAfter > 0)
            yield return new WaitForSeconds(e.waitTimeAfter);
    }

    IEnumerator MoveActor(GameObject actor, Vector3 target, float duration)
    {
        Vector3 start = actor.transform.position;
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / duration;
            actor.transform.position = Vector3.Lerp(start, target, t);
            yield return null;
        }
    }
}
