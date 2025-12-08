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

    [Header("Adjust volume (0–1)")]
    [Range(0f, 1f)]
    public float voiceVolume = 1f;

    [Header("Scene to load after the last clip")]
    public string startSceneName = "StartScene";

    [Tooltip("Small delay after each clip before switching to the next UI (seconds).")]
    public float extraDelayAfterClip = 0.25f;

    private int currentIndex = -1;

    void Start()
    {
        // Ensure we have an AudioSource
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        HideAllUIPanels();
        NextStep();
    }

    private void HideAllUIPanels()
    {
        if (steps == null) return;

        foreach (var s in steps)
        {
            if (s != null && s.uiPanel != null)
                s.uiPanel.SetActive(false);
        }
    }

    private void NextStep()
    {
        currentIndex++;

        if (steps == null || currentIndex >= steps.Length)
        {
            if (!string.IsNullOrEmpty(startSceneName))
                SceneManager.LoadScene(startSceneName);

            return;
        }

        HideAllUIPanels();
        Step step = steps[currentIndex];

        if (step != null && step.uiPanel != null)
            step.uiPanel.SetActive(true);

        // Play audio with adjustable volume
        if (step != null && step.voiceClip != null && audioSource != null)
        {
            audioSource.Stop();
            audioSource.clip = step.voiceClip;

            audioSource.volume = voiceVolume;   // ALWAYS applied before playing
            audioSource.Play();

            float delay = step.voiceClip.length + extraDelayAfterClip;
            Invoke(nameof(NextStep), delay);
        }
        else
        {
            Invoke(nameof(NextStep), 0.5f);
        }
    }
}

// This script was created with inspiration from Copolit and Chatgpt