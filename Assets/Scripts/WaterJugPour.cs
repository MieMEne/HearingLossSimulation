using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class WaterJugPour : MonoBehaviour
{
    private Animator animator;
    private XRBaseInteractable interactable;
    private bool isAnimating;

    void Awake()
    {
        animator = GetComponent<Animator>();
        interactable = GetComponent<XRBaseInteractable>();
    }

    void OnEnable()
    {
        interactable.selectEntered.AddListener(OnSelectEntered);
    }

    void OnDisable()
    {
        interactable.selectEntered.RemoveListener(OnSelectEntered);
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (isAnimating) return;  // Prevent re-triggering while animation is playing
        isAnimating = true;

        animator.ResetTrigger("LiftAndPour");
        animator.SetTrigger("LiftAndPour");
    }

    // Optional: add this if you want to allow replay after the animation ends
    public void OnAnimationFinished()
    {
        isAnimating = false;
    }
}
