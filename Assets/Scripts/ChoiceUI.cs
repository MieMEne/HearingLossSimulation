using UnityEngine;

public class ChoiceUI : MonoBehaviour
{
    [HideInInspector] public bool buttonPressed = false;

    // Call this from your buttons' OnClick()
    public void OnChoiceButtonPressed()
    {
        buttonPressed = true;
    }

    // Reset the flag when the panel is shown
    public void ResetChoice()
    {
        buttonPressed = false;
    }
}
