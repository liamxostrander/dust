using UnityEngine;
using TMPro;

public class WaveHUD : MonoBehaviour
{
    [Header("Text Refs")]
    public TMP_Text waveText;
    public TMP_Text enemiesText;

    [Header("Timer (preferred: split label/value)")]
    public TMP_Text timerLabelText;
    public TMP_Text timerValueText;

    [Header("Fallback (single line)")]
    public TMP_Text timerText;

    [Header("Intermission")]
    [Tooltip("Optional label shown only during intermission, place this wherever you want in the UI.")]
    public TMP_Text intermissionText;

    [Header("Timer Colors")]
    public Color normalValueColor = Color.white;
    public Color urgentValueColor = Color.red;

    private bool urgentOn = false;

    void Awake()
    {
        if (intermissionText != null)
            intermissionText.gameObject.SetActive(false);
    }

    public void SetWaveText(int current, int total)
    {
        if (waveText == null) return;
        waveText.text = $"{current}/{total}";
    }

    public void SetEnemies(int count)
    {
        if (enemiesText != null)
            enemiesText.text = $"{count}";
    }

    public void SetTimer(string display)
    {
        if (timerLabelText && timerValueText)
        {
            timerLabelText.text = "";
            timerValueText.text = display;
            timerValueText.color = urgentOn ? urgentValueColor : normalValueColor;
        }
        else if (timerText)
        {
            string colorHex = ColorUtility.ToHtmlStringRGBA(urgentOn ? urgentValueColor : normalValueColor);
            timerText.text = $"<color=#{colorHex}>{display}</color>";
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

    public void ShowIntermission(string label = "Intermission")
    {
        if (intermissionText == null) return;
        intermissionText.text = label;
        intermissionText.gameObject.SetActive(true);
    }

    public void HideIntermission()
    {
        if (intermissionText == null) return;
        intermissionText.gameObject.SetActive(false);
    }
}
