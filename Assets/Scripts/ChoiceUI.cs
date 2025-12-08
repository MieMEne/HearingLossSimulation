// ChoiceUI.cs (robust version)
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ChoiceUI : MonoBehaviour
{
    [Header("Behavior")]
    [Tooltip("Hide this panel after a choice is made.")]
    public bool hideOnChoose = true;

    [Tooltip("If assigned, its alpha/interactable/blocksRaycasts will be managed on show/hide.")]
    public CanvasGroup canvasGroup;

    [Header("Saving (OFF by default)")]
    [Tooltip("Enable only for the food choice in Mild. Leave OFF for all other questions.")]
    public bool enableSaving = false;

    [Tooltip("Only used if 'Enable Saving' is true. Must match what your spawner reads (e.g., 'ChosenFoodIndex').")]
    public string saveKey = "ChosenFoodIndex";

    [Header("Auto Binding")]
    [Tooltip("Automatically find child Buttons and bind them to indices 0..N-1 in hierarchy order.")]
    public bool autoBindChildButtons = true;

    [Tooltip("Optional explicit list to control index order. If empty, hierarchy order is used.")]
    public List<Button> explicitButtons = new List<Button>();

    [HideInInspector] public bool buttonPressed = false;
    [HideInInspector] public int chosenIndex = -1;

    // Keep references to unbind on disable
    private readonly List<Button> _boundButtons = new List<Button>();

    void Awake()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
    }

    void OnEnable()
    {
        // Fresh state whenever shown
        ResetChoice();

        // Make visible/clickable
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        // Auto-bind (or explicit bind)
        BindButtons();
    }

    void OnDisable()
    {
        UnbindButtons();
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

        if (enableSaving && !string.IsNullOrEmpty(saveKey))
        {
            PlayerPrefs.SetInt(saveKey, index);
            PlayerPrefs.Save();
            Debug.Log($"[ChoiceUI] Choice pressed: {index}, saved under '{saveKey}'.");
        }
        else
        {
            Debug.Log($"[ChoiceUI] Choice pressed: {index} (saving disabled).");
        }

        if (hideOnChoose)
            HidePanel();
    }

    private void HidePanel()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        gameObject.SetActive(false);
    }

    private void BindButtons()
    {
        UnbindButtons();

        if (!autoBindChildButtons && (explicitButtons == null || explicitButtons.Count == 0))
        {
            // Nothing to bind; rely on manual OnClick wiring from inspector
            Debug.Log($"[ChoiceUI] AutoBind OFF and no explicit buttons; waiting for manual OnClick calls.");
            return;
        }

        if (explicitButtons != null && explicitButtons.Count > 0)
        {
            for (int i = 0; i < explicitButtons.Count; i++)
                BindOne(explicitButtons[i], i);
            Debug.Log($"[ChoiceUI] Bound {explicitButtons.Count} explicit buttons.");
            return;
        }

        // Auto-bind by hierarchy order
        var buttons = GetComponentsInChildren<Button>(includeInactive: true);
        for (int i = 0; i < buttons.Length; i++)
            BindOne(buttons[i], i);

        Debug.Log($"[ChoiceUI] Auto-bound {buttons.Length} buttons by hierarchy order.");
    }

    private void BindOne(Button btn, int index)
    {
        if (btn == null) return;
        _boundButtons.Add(btn);
        btn.onClick.AddListener(() => OnChoiceButtonPressed(index));
    }

    private void UnbindButtons()
    {
        foreach (var b in _boundButtons)
            if (b != null) b.onClick.RemoveAllListeners();
        _boundButtons.Clear();
    }
}

// Reference
// https://www.youtube.com/watch?v=_nRzoTzeyxU
// This script was created with inspiration from Copilot and chatgpt