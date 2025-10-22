using UnityEngine;

public class ChoiceUI : MonoBehaviour
{
    [HideInInspector] public bool buttonPressed = false;
    [HideInInspector] public int chosenIndex = -1; // 0 = first choice, 1 = second, etc.

    [Header("Optional - Save Key (for PlayerPrefs)")]
    public string saveKey = "ChosenFoodIndex";

    // Called from each button's OnClick(), with a choice index
    public void OnChoiceButtonPressed(int index)
    {
        chosenIndex = index;
        buttonPressed = true;

        //  Save the player's choice for later scenes
        PlayerPrefs.SetInt(saveKey, index);
        PlayerPrefs.Save();

        //  Instantly hide the UI panel so the EventManager can continue smoothly
        gameObject.SetActive(false);

        Debug.Log($"[ChoiceUI] Saved {saveKey} = {index}");
    }

    // Reset the flags when the panel is shown
    public void ResetChoice()
    {
        buttonPressed = false;
        chosenIndex = -1;
    }
}
