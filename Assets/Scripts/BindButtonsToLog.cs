using UnityEngine;
using UnityEngine.UI;

public class BindButtonsToLog : MonoBehaviour
{
    void Awake()
    {
        // Loop through all Button components found in children (including inactive ones)
        foreach (var button in GetComponentsInChildren<Button>(true))
        {
            string n = button.gameObject.name;
            // Add a listener to log when the button is clicked
            button.onClick.AddListener(() =>
            {
                Debug.Log($"[UI] Clicked {n} at {Time.time:F2}s");
            });
        }
    }
}
