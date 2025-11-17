using UnityEngine;
using UnityEngine.SceneManagement;

public class EndSceneUISequence : MonoBehaviour
{
    [System.Serializable]
    public class Step
    {
        [Header("UI shown during this voice line")]
        public GameObject uiPanel;

        [Header("Voice line for this step")]
        public AudioClip voiceClip;
    }

    [Header("Steps in order (UI + Audio)")]
    public Step[] steps;

    [Header("AudioSource to play the voice clips")]
    public AudioSource audioSource;

    [Header("Scene to load after the last clip")]
    public string startSceneName = "StartScene";

    [Tooltip("Small delay after each clip before switching to the next UI (seconds).")]
    public float extraDelayAfterClip = 0.25f;

    private int currentIndex = -1;

    void Start()
    {
        // Make sure we have an AudioSource
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        HideAllUIPanels();
        NextStep();   // start with the first UI + audio
    }

    private void HideAllUIPanels()
    {
        if (steps == null) return;

        foreach (var s in steps)
        {
            if (s != null && s.uiPanel != null)
            {
                s.uiPanel.SetActive(false);
            }
        }
    }

    private void NextStep()
    {
        currentIndex++;

        // If we've passed the last step, go back to the start scene
        if (steps == null || currentIndex >= steps.Length)
        {
            if (!string.IsNullOrEmpty(startSceneName))
            {
                Debug.Log("[EndSceneUISequence] All steps finished, loading scene: " + startSceneName);
                SceneManager.LoadScene(startSceneName);
            }
            else
            {
                Debug.LogWarning("[EndSceneUISequence] startSceneName is empty, staying in current scene.");
            }
            return;
        }

        // Show only the current panel
        HideAllUIPanels();
        Step step = steps[currentIndex];

        if (step != null && step.uiPanel != null)
        {
            step.uiPanel.SetActive(true);
        }

        // Play the voice clip for this step
        if (step != null && step.voiceClip != null && audioSource != null)
        {
            audioSource.Stop();
            audioSource.clip = step.voiceClip;
            audioSource.Play();

            float delay = step.voiceClip.length + extraDelayAfterClip;
            Invoke(nameof(NextStep), delay);
        }
        else
        {
            // If there's no clip, just move on quickly
            Invoke(nameof(NextStep), 0.5f);
        }
    }
}
