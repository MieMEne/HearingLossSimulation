using UnityEngine;
using System.Collections;

public class TalkingAnimations3 : MonoBehaviour
{
    public Animator animator;

    // ✅ Enum for all animations you want available in dropdown
    public enum AnimationType
    {
        Idle,
        Idle2,
        Talk1,
        Talk2,
        Talk3,
        Talk4,
        Talk5,
        Cheer,
        LookAtCharacter,
        Laugh,
        // ✅ Add more as needed
    }

    [System.Serializable]
    public struct AnimationStep
    {
        public float time;               // When to play this animation
        public AnimationType animation;  // Dropdown appears here!
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
                PlayAnimation(sequence[currentIndex].animation);
                currentIndex++;
            }

            yield return null;
        }
    }

    private void PlayAnimation(AnimationType type)
    {
        // ✅ CrossFade forces the animator to go to the named state smoothly
        animator.CrossFade(type.ToString(), 0.1f);
    }
}
