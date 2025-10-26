using UnityEngine;

public class PauseMenuManager : MonoBehaviour
{
    [Header("Pause Menu UI")]
    public GameObject pauseMenuUI;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioSource musicSource;
    public AudioClip onClickClip;
    public float pausedVolume = 0.2f;
    public float volumeFadeSpeed = 2f;
    private float originalVolume;
    private static bool gameIsPaused = false;

    [HideInInspector]
    public float originalTimeScale;
    void Start()
    {
        originalTimeScale = Time.timeScale;
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }
        if (musicSource != null)
        {
            originalVolume = musicSource.volume;
        }
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (gameIsPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }
    public void ResumeGame()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = originalTimeScale;
        gameIsPaused = false;
    }
    void PauseGame()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        gameIsPaused = true;
    }
}
