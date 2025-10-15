using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class VRFadeIn : MonoBehaviour
{
    public RawImage fadeImage;      // Assign your black image here
    public float fadeDuration = 1f;

    void Start()
    {
        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        float t = 0f;
        Color c = fadeImage.color;
        c.a = 1f; // start fully black
        fadeImage.color = c;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float eased = Mathf.SmoothStep(1f, 0f, t / fadeDuration); // easing: slow at start, faster at end
            c.a = eased;
            fadeImage.color = c;
            yield return null;
        }

        c.a = 0f; 
        fadeImage.color = c; // ensure fully transparent at the end
    }
}
