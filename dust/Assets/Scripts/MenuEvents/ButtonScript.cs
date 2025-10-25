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
    void Start()
    {
        sceneSwitcher = FindFirstObjectByType<SceneSwitcher>();
    }

    public void OnButtonClick()
    {
        GameObject clickedButton = EventSystem.current.currentSelectedGameObject;
        if (clickedButton != null)
        {
            if (clickedButton.CompareTag("QuitButton"))
            {
                Application.Quit();
                UnityEditor.EditorApplication.isPlaying = false;
            }
            else if (clickedButton.CompareTag("StartButton"))
            {
                musicSource.volume = 0.1f;
                audioSource.PlayOneShot(onClickClip);
                sceneSwitcher.LoadGameSceneWithTransition();
            }
        }
    }


}
