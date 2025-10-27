using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class ButtonScript : MonoBehaviour
{

    [Header("Button Audio Settings")]
    public AudioSource audioSource;
    public AudioSource musicSource;
    public AudioClip onClickClip;
    private SceneSwitcher sceneSwitcher;
    private PauseMenuManager pauseMenuManager;
    void Start()
    {
        sceneSwitcher = FindFirstObjectByType<SceneSwitcher>();
        pauseMenuManager = FindFirstObjectByType<PauseMenuManager>();
    }

    public void OnButtonClick()
    {
        GameObject clickedButton = EventSystem.current.currentSelectedGameObject;
        if (clickedButton != null)
        {
            audioSource.PlayOneShot(onClickClip);
            if (clickedButton.CompareTag("QuitButton"))
            {
                Application.Quit();
                UnityEditor.EditorApplication.isPlaying = false;
            }
            else if (clickedButton.CompareTag("StartButton"))
            {
                musicSource.volume = 0.1f;
                sceneSwitcher.LoadGameSceneWithTransition();
            }
            else if (clickedButton.CompareTag("ResumeButton"))
            {
                pauseMenuManager.ResumeGame();
            }
            else if (clickedButton.CompareTag("MainMenuButton"))
            {
                Time.timeScale = pauseMenuManager.originalTimeScale;
                // if (pauseMenuManager.pauseMenuUI.activeInHierarchy)
                // {
                //     pauseMenuManager.pauseMenuUI.SetActive(false);
                // }
                sceneSwitcher.LoadScene("MainMenu");
            }
        }
    }


}
