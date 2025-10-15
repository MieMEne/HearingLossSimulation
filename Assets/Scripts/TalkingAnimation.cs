using UnityEngine;
using System.Collections;

public class TalkingAnimations : MonoBehaviour
{
    public Animator animator;

    [System.Serializable]
    public struct AnimationStep
    {
        public float time;       // seconds relative to event start
        public bool isTalking;   // whether to switch to talking
    }

    public AnimationStep[] sequence;

    private Coroutine sequenceCoroutine;

    // Call this from EventManager to start the sequence
    public void PlaySequence()
    {
        if (sequenceCoroutine != null)
            StopCoroutine(sequenceCoroutine);

        sequenceCoroutine = StartCoroutine(RunSequence());
    }

    private IEnumerator RunSequence()
    {
        float timer = 0f;
        int currentIndex = 0;

        while (currentIndex < sequence.Length)
        {
            timer += Time.deltaTime;

            if (timer >= sequence[currentIndex].time)
            {
                animator.SetBool("IsTalking", sequence[currentIndex].isTalking);
                currentIndex++;
            }

            yield return null;
        }
    }
}
