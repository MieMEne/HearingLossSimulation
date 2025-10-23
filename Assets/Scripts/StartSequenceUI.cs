using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartSequenceUI : MonoBehaviour
{
    [Header("Screens (RawImage/Image GameObjects)")]
    public GameObject screen1;          // FIRST sprite (Start)
    public Button screen1Button;        // Button on screen1

    public GameObject screen2;          // SECOND sprite (Start Simulation)
    public Button screen2Button;        // Button on screen2

    public GameObject screen3;          // THIRD sprite/UI (OK / confirmation)
    public Button screen3OkButton;      // OK button on screen3

    [Header("Next Scene")]
    public string nextSceneName = "NormalHearingScene";

    void Awake()
    {
        if (screen1Button) screen1Button.onClick.AddListener(OnStartPressed);
        if (screen2Button) screen2Button.onClick.AddListener(OnStartSimulationPressed);
        if (screen3OkButton) screen3OkButton.onClick.AddListener(OnOkPressed);
    }

    void Start()
    {
        ShowOnly(screen1); // start at first screen
    }

    void ShowOnly(GameObject active)
    {
        if (screen1) screen1.SetActive(active == screen1);
        if (screen2) screen2.SetActive(active == screen2);
        if (screen3) screen3.SetActive(active == screen3);
    }

    // ---- Button callbacks ----
    public void OnStartPressed() { ShowOnly(screen2); }
    public void OnStartSimulationPressed() { ShowOnly(screen3); }
    public void OnOkPressed() { SceneManager.LoadScene(nextSceneName); }
}
