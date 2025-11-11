using UnityEngine;
using TMPro;

public class WaveHUD : MonoBehaviour
{
    [Header("Text Refs")]
    public TMP_Text waveText;
    public TMP_Text enemiesText;

    [Header("Timer (preferred: split label/value)")]
    [Tooltip("Use these to color only the value (eg. 0:03) for when waves tick down to 3 seconds or less")]
    public TMP_Text timerLabelText;
    public TMP_Text timerValueText;

    [Header("Fallback (single line)")]
    [Tooltip("If you prefer one line, leave the two above null and use this instead.")]
    public TMP_Text timerText;

    [Header("Timer Colors")]
    public Color normalValueColor = Color.white;
    public Color urgentValueColor = Color.red;

    private bool urgentOn = false;

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
        if (timerLabelText && timerValueText)
        {
            timerLabelText.text = "Timer:";
            timerValueText.text = display;
            timerValueText.color = urgentOn ? urgentValueColor : normalValueColor;
        }
        else if (timerText)
        {
            string colorHex = ColorUtility.ToHtmlStringRGBA(urgentOn ? urgentValueColor : normalValueColor);
            timerText.text = $"Timer: <color=#{colorHex}>{display}</color>";
        }
    }

    public void SetTimerUrgent(bool urgent)
    {
        urgentOn = urgent;
        if (timerLabelText && timerValueText)
        {
            timerValueText.color = urgentOn ? urgentValueColor : normalValueColor;
        }
    }
}
