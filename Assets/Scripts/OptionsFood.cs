using UnityEngine;
using UnityEngine.UI;

public class OptionsFood : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public Button option1Button;
    public Button option2Button;

    public GameObject option1ResultObject;
    public GameObject option2ResultObject;

    void Start()
    {
        // Hide both result objects at start
        option1ResultObject.SetActive(false);
        option2ResultObject.SetActive(false);

        // Add listeners
        option1Button.onClick.AddListener(OnOption1Pressed);
        option2Button.onClick.AddListener(OnOption2Pressed);
    }

    void OnOption1Pressed()
    {
        Debug.Log("Option 1 pressed!");
        option1ResultObject.SetActive(true);
        option2ResultObject.SetActive(false);

        // Hide only THIS question
        gameObject.SetActive(false);
    }

    void OnOption2Pressed()
    {
        Debug.Log("Option 2 pressed!");
        option1ResultObject.SetActive(false);
        option2ResultObject.SetActive(true);

        // Hide only THIS question
        gameObject.SetActive(false);
    }
}
