using UnityEngine;
using UnityEngine.InputSystem;

public class animationController : MonoBehaviour
{
    public InputActionProperty grabAction;
    public InputActionProperty triggerAction;

    public Animator myAnimator;
 
    void Update()
    {
        float grabValue = grabAction.action.ReadValue<float>();
        myAnimator.SetFloat("Grab", grabValue);
        float triggerValue = triggerAction.action.ReadValue<float>();
        myAnimator.SetFloat("Trigger", triggerValue);

    }
}

//Reference
// https://www.youtube.com/watch?v=3dYSHu95P_0&t=4s