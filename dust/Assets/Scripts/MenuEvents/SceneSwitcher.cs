using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SceneSwitcher : MonoBehaviour
{
    [Header("Transition Effect Settings")]
    public Image transitionEffectImage;
    public float fadeDuration;

    [Header("Scene Names")]
    public string gameSceneName;

    [Header("MusicSettings")]
    public AudioSource musicSource;

    void Start()
    {
        musicSource.volume = 1f;
    }

    private IEnumerator FadeOutAndLoad(string sceneName)
    {
        transitionEffectImage.gameObject.SetActive(true);
        float t = 0f;
        Color c = transitionEffectImage.color;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            c.a = Mathf.Lerp(0f, 1f, t / fadeDuration);
            transitionEffectImage.color = c;
            yield return null;
        }
        c.a = 1f;
        transitionEffectImage.color = c;

        SceneManager.LoadScene(sceneName);
    }
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
    public void LoadGameScene()
    {
        StartCoroutine(FadeOutAndLoad(gameSceneName));
    }

    public void LoadGameSceneWithTransition()
    {
        Invoke("LoadGameScene", 0f);
    }
}
