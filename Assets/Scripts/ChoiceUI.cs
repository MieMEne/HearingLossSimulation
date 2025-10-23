// ChoiceUI.cs
using UnityEngine;

public class ChoiceUI : MonoBehaviour
{
    [Header("Optional - Save Key (for PlayerPrefs)")]
    public string saveKey = "ChosenFoodIndex";

    [HideInInspector] public bool buttonPressed = false;
    [HideInInspector] public int chosenIndex = -1;

    [Header("Optional")]
    public bool hideOnChoose = true;       // hide the panel after a click
    public CanvasGroup canvasGroup;        // optional; auto-found if missing

    void Awake()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
    }

    void OnEnable()
    {
        // fresh state whenever the panel is shown
        ResetChoice();

        // ensure it is visible & clickable if a CanvasGroup is used
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }

    /// <summary>Clear selection and ready to wait for a new click.</summary>
    public void ResetChoice()
    {
        buttonPressed = false;
        chosenIndex = -1;
    }

    /// <summary>Call with 0/1/etc from your button or OptionsFood script.</summary>
    public void OnChoiceButtonPressed(int index)
    {
        chosenIndex = index;
        buttonPressed = true;

        if (!string.IsNullOrEmpty(saveKey))
        {
            PlayerPrefs.SetInt(saveKey, index);
            PlayerPrefs.Save();
        }

        if (hideOnChoose)
            gameObject.SetActive(false);

        Debug.Log($"[ChoiceUI] Choice pressed: {index}, saved under '{saveKey}'.");
    }
}
