using UnityEngine;

public class CafeSoundTest : MonoBehaviour
{
    public AudioSource cafeSoundSource;

    void Start()
    {
        if (cafeSoundSource != null)
        {
            cafeSoundSource.Play();
            Debug.Log("Playing cafe sound...");
        }
        else
        {
            Debug.LogWarning("No AudioSource assigned!");
        }
    }
}