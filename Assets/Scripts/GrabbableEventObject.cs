using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class GrabbableEventObject : MonoBehaviour
{
    public bool HasBeenGrabbed { get; private set; } = false;

    private XRGrabInteractable grabInteractable;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
    }

    void OnEnable()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrab);
        }
    }

    void OnDisable()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrab);
        }
    }

    public void ResetGrabState()
    {
        HasBeenGrabbed = false;
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        Debug.Log($"{name} was grabbed!");
        HasBeenGrabbed = true;
    }
}
