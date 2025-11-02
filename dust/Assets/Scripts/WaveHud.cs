using UnityEngine;
using TMPro;

public class WaveHUD : MonoBehaviour
{
    [Header("Text Refs")]
    public TMP_Text waveText;
    public TMP_Text enemiesText;
    public TMP_Text timerText;

    public void SetWaveText(int current, int total)
    {
        if (waveText != null)
            waveText.text = $"Wave: {current}/{total}";
    }

    public void SetEnemies(int count)
    {
        if (enemiesText != null)
            enemiesText.text = $"Enemies: {count}";
    }

    public void SetTimer(string display)
    {
        if (timerText != null)
            timerText.text = $"Timer: {display}";
    }
}
