using UnityEngine;
using System.Collections;

public class TalkingAnimations2 : MonoBehaviour
{
    public Animator animator;

    // BODY animations = Layer 0
    public enum BodyAnimationType
    {
        Idle,
        Idle2,
        StandingIdle,
        Talk1,
        Talk2,
        Talk3,
        Talk4,
        Talk5,
        Cheer,
        LookAtLiam,
        LookAtEmma,
        Laugh,
    }

    // FACE animations = Layer 1
    public enum FaceAnimationType
    {
        None,
        Liam_Normal1,
        Liam_Normal2,
        Liam_Normal3,
        Liam_Normal4,
        LiamMild1,
        LiamMild2,
        LiamMild3,
        LiamMild4,
        LiamMod1,
        LiamMod2,
        LiamMod3,
        LiamMod4,
        LiamMod5,
        LiamMod6,
        LiamSever1,
        LiamSever2,
        LiamSevere3,
        LiamSevere4,
        EmmaNormal1,
        EmmaNormal2,
        EmmaNormal3,
        EmmaNormal4,
        EmmaNormal5,
        EmmaNormal6,
        EmmaMild1,
        EmmaMild2,
        EmmaMild3,
        EmmaMild4,
        EmmaMild5,
        EmmaMild6,
        EmmaMod1,
        EmmaMod2,
        EmmaMod3,
        EmmaMod4,
        EmmaMod5_001,
        EmmaMod6,
        EmmaMod7,
        EmmaMod8,
        EmmaSever1,
        EmmaSevere2,
        EmmaSevere3,
        EmmaSevere4,
        EmmaSevere5,
        SofiaNormal1,
        SofiaNormal2,
        SofiaNormal3,
        SofiaNormal4,
        SofiaNormal5,
        SofiaNormal6,
        SofiaMild1,
        SofiaMild2,
        SofiaMild3,
        SofiaMild4,
        SofiaMod1,
        SofiaMod2,
        SofiaMod3,
        SofiaMod4,
        SofiaMod5,
        SofiaSevere1,
        SofiaSevere2,
        SofiaSevere3,
        SofiaSevere4,
        SofiaSevere5,
        Mild1,
        Mild2,
        normal,
        // long clips, no loops needed
    }

    [System.Serializable]
    public struct AnimationStep
    {
        public float time;
        public BodyAnimationType bodyAnim;
        public FaceAnimationType faceAnim;   // Only set when you want a NEW face clip
    }

    public AnimationStep[] sequence;

    private Coroutine sequenceCoroutine;
    private FaceAnimationType currentFaceAnim = FaceAnimationType.None;

    public void PlaySequence()
    {
        if (sequenceCoroutine != null)
            StopCoroutine(sequenceCoroutine);

        sequenceCoroutine = StartCoroutine(RunSequence());
    }

    private IEnumerator RunSequence()
    {
        float timer = 0f;
        int index = 0;

        while (index < sequence.Length)
        {
            timer += Time.deltaTime;

            if (timer >= sequence[index].time)
            {
                // Always play body animation
                PlayBodyAnimation(sequence[index].bodyAnim);

                // Play face animation ONLY if it is a new one
                if (sequence[index].faceAnim != FaceAnimationType.None &&
                    sequence[index].faceAnim != currentFaceAnim)
                {
                    currentFaceAnim = sequence[index].faceAnim;
                    PlayFaceAnimation(currentFaceAnim);
                }

                index++;
            }

            yield return null;
        }
    }

    private void PlayBodyAnimation(BodyAnimationType type)
    {
        animator.CrossFade(type.ToString(), 0.1f, 0); // Layer 0
    }

    private void PlayFaceAnimation(FaceAnimationType type)
    {
        animator.CrossFade(type.ToString(), 0.1f, 1); // Layer 1
    }
}