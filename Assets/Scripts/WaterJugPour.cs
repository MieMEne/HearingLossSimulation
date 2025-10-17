using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class WaterJugPour : MonoBehaviour
{
    private Animator animator;
    private XRBaseInteractable interactable;
    private bool isPouring;

    void Awake()
    {
        animator = GetComponent<Animator>();
        interactable = GetComponent<XRBaseInteractable>(); // XR Simple Interactable works (it derives from XRBaseInteractable)
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
        if (isPouring) return;
        isPouring = true;
        animator.SetTrigger("Pour");
    }

    // Call this from an Animation Event at the end of PourWater
    public void OnPourFinished()
    {
        isPouring = false;
    }
}
