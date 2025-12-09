using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DeathScreenFade : MonoBehaviour
{
    public static DeathScreenFade Instance;

    [Header("UI Elements")]
    public CanvasGroup canvasGroup;      
    public TextMeshProUGUI deathText;    
    public float fadeDuration = 1.5f;    

    void Start()
    {
        Instance = this;
        canvasGroup.alpha = 0f;
        deathText.alpha = 0f;
    }

    /// <summary>
    /// Called when the death animation finishes.
    /// </summary>
    public void PlayDeathScreen()
    {
        gameObject.SetActive(true);
        StartCoroutine(FadeRoutine());
    }

    private IEnumerator FadeRoutine()
    {
        // Fade screen to black
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }

        // Show "DEATH"
        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            deathText.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }

        // Game is now fully paused
        Time.timeScale = 0f;
    }
}
