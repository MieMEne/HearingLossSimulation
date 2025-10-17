using UnityEngine;

public class ChoiceUI : MonoBehaviour
{
    [HideInInspector] public bool buttonPressed = false;
    [HideInInspector] public int chosenIndex = -1; // 0 = first choice, 1 = second, etc.

    // Called from each button's OnClick(), with a choice index
    public void OnChoiceButtonPressed(int index)
    {
        chosenIndex = index;
        buttonPressed = true;
    }

    // Reset the flags when the panel is shown
    public void ResetChoice()
    {
        buttonPressed = false;
        chosenIndex = -1;
    }
}
