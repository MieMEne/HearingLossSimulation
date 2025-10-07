using UnityEngine;

public class AudioController: MonoBehaviour
{
    public AudioSource cafeSoundSource;

    void Start()
    {
        if (cafeSoundSource != null)
        {
            cafeSoundSource.Play();
            Debug.Log("Playing cafe sound...");
            Debug.Log("Playing");
        }
        else
        {
            Debug.Log("No AudioSource assigned!");
        }
    }
}