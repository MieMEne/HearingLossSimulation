using UnityEngine;
using System.Collections;

[System.Serializable]
public class StoryEvent
{
    [Header("General Info")]
    public string eventName;

    [Header("Audio")]
    public AudioClip[] voiceClips;        // multiple audio clips
    public float timeBetweenClips = 0.5f; // time interval between clips

    [Header("UI")]
    public GameObject uiToShow;

    [Header("Character Movement")]
    public GameObject actorToMove;
    public Transform targetPosition;
    public float moveDuration = 2f;

    [Header("Timing")]
    public float waitTimeBefore = 0f;
    public float waitTimeAfter = 0.5f;
}

public class EventManager : MonoBehaviour
{
    [Header("Story Setup")]
    public StoryEvent[] events;        // list of story events
    public AudioSource audioSource;    // plays dialogue and sound
    private int currentIndex = 0;      // keeps track of progress

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

        // 🎙️ Play multiple audio clips
        if (e.voiceClips != null && e.voiceClips.Length > 0 && audioSource != null)
        {
            foreach (AudioClip clip in e.voiceClips)
            {
                if (clip != null)
                {
                    audioSource.clip = clip;
                    audioSource.Play();
                    yield return new WaitWhile(() => audioSource.isPlaying);

                    // Wait interval between clips
                    if (e.timeBetweenClips > 0)
                        yield return new WaitForSeconds(e.timeBetweenClips);
                }
            }
        }

        // 💬 Show UI
        if (e.uiToShow != null)
        {
            e.uiToShow.SetActive(true);
            // Wait until UI hides itself (after player answers)
            yield return new WaitUntil(() => e.uiToShow.activeSelf == false);
        }

        // 🚶 Move actor
        if (e.actorToMove != null && e.targetPosition != null)
        {
            yield return StartCoroutine(MoveActor(e.actorToMove, e.targetPosition.position, e.moveDuration));
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
