using UnityEngine;

public class GlobalAudio : MonoBehaviour
{
    public static AudioSource SFX;

    [SerializeField] private AudioSource sfxSource;

    private void Awake()
    {
        SFX = sfxSource;
    }
}