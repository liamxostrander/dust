using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSettingsUI : MonoBehaviour
{
    [Header("Mixer")]
    public AudioMixer audioMixer;

    [Header("UI Sliders")]
    public Slider musicSlider;
    public Slider sfxSlider;
    public Slider environmentSlider;

    private void Start()
    {
        if (PlayerPrefs.HasKey("MusicVolume"))
            audioMixer.SetFloat("MusicVolume", PlayerPrefs.GetFloat("MusicVolume"));

        if (PlayerPrefs.HasKey("SFXVolume"))
            audioMixer.SetFloat("SFXVolume", PlayerPrefs.GetFloat("SFXVolume"));
        if (PlayerPrefs.HasKey("EnvironmentVolume"))
            audioMixer.SetFloat("EnvironmentVolume", PlayerPrefs.GetFloat("EnvironmentVolume"));

        float musicVol, sfxVol, environmentVol;
        audioMixer.GetFloat("MusicVolume", out musicVol);
        audioMixer.GetFloat("SFXVolume", out sfxVol);
        audioMixer.GetFloat("EnvironmentVolume", out environmentVol);

        musicSlider.value = musicVol;
        sfxSlider.value = sfxVol;
        environmentSlider.value = environmentVol;

        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        environmentSlider.onValueChanged.AddListener(SetEnvironmentVolume);
    }

    public void SetMusicVolume(float value)
    {
        audioMixer.SetFloat("MusicVolume", value);
        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    public void SetSFXVolume(float value)
    {
        audioMixer.SetFloat("SFXVolume", value);
        PlayerPrefs.SetFloat("SFXVolume", value);
    }

    public void SetEnvironmentVolume(float value)
    {
        audioMixer.SetFloat("EnvironmentVolume", value);
        PlayerPrefs.SetFloat("EnvironmentVolume", value);
    }
}
