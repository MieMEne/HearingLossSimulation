using UnityEngine;
using System.Collections;

public class TalkingAnimations2 : MonoBehaviour
{
    public Animator animator;

    [System.Serializable]
    public struct AnimationStep
    {
        public float time;
        public bool isTalking;
    }

    public AnimationStep[] sequence;

    private Coroutine sequenceCoroutine;

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
