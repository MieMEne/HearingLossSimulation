using UnityEngine;
using UnityEngine.SceneManagement;

public class PanelSequenceController : MonoBehaviour
{
    [System.Serializable]
    public class PanelData
    {
        public GameObject panel;
        public AudioClip narration;
    }

    public PanelData[] panels;
    public AudioSource narrationSource;
    public string sceneToLoadAtEnd;

    private int currentIndex = 0;

    void Start()
    {
        foreach (var p in panels)
            p.panel.SetActive(false);

        ShowPanel(0);
    }

    public void NextPanel()
    {
        if (narrationSource.isPlaying)
            return;

        currentIndex++;

        if (currentIndex >= panels.Length)
        {
            SceneManager.LoadScene(sceneToLoadAtEnd);
            return;
        }

        ShowPanel(currentIndex);
    }

    private void ShowPanel(int index)
    {
        foreach (var p in panels)
            p.panel.SetActive(false);

        panels[index].panel.SetActive(true);

        if (panels[index].narration != null)
        {
            narrationSource.clip = panels[index].narration;
            narrationSource.Play();
        }
    }
}
