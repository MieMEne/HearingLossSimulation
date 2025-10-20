using UnityEngine;

public class CloseCanvasButton : MonoBehaviour
{
    public GameObject canvasToDisable;

    public void DisableCanvas()
    {
        Debug.Log("Button pressed! Disabling canvas...");

        if (canvasToDisable != null)
        {
            canvasToDisable.SetActive(false);
            Debug.Log("Canvas disabled successfully.");
        }
        else
        {
            Debug.LogWarning("No canvas assigned to disable!");
        }
    }
}
