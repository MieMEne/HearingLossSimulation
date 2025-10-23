using UnityEngine;
using UnityEngine.UI;

public class OptionsFood : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public Button option1Button;           // Burger
    public Button option2Button;           // Soup

    public GameObject option1ResultObject; // optional preview for burger
    public GameObject option2ResultObject; // optional preview for soup

    [Header("Required: ChoiceUI on this same panel")]
    public ChoiceUI choiceUI; // Drag the panel here or leave null to auto-find

    void Awake()
    {
        if (choiceUI == null)
            choiceUI = GetComponent<ChoiceUI>();
        if (choiceUI == null)
            Debug.LogError("[OptionsFood] Missing ChoiceUI component on the same GameObject.");
    }

    void Start()
    {
        if (option1ResultObject) option1ResultObject.SetActive(false);
        if (option2ResultObject) option2ResultObject.SetActive(false);

        option1Button.onClick.AddListener(() => Pick(0)); // 0 = burger
        option2Button.onClick.AddListener(() => Pick(1)); // 1 = soup
    }

    void Pick(int index)
    {
        if (option1ResultObject) option1ResultObject.SetActive(index == 0);
        if (option2ResultObject) option2ResultObject.SetActive(index == 1);

        if (choiceUI != null)
            choiceUI.OnChoiceButtonPressed(index); // <- tells EventManager & saves
        else
            gameObject.SetActive(false); // fallback: at least close the panel

        Debug.Log($"[OptionsFood] Picked index {index}");
    }
}
