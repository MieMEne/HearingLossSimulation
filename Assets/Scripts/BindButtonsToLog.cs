using UnityEngine;
using UnityEngine.UI;

public class BindButtonsToLog : MonoBehaviour
{
    void Awake()
    {
        foreach (var button in GetComponentsInChildren<Button>(true))
        {
            string n = button.gameObject.name;
            button.onClick.AddListener(() =>
            {
                Debug.Log($"[UI] Clicked {n} at {Time.time:F2}s");
            });
        }
    }
}
