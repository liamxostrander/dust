using UnityEngine;
using TMPro;
using System.Collections;

public class UpgradeMessage : MonoBehaviour
{
    public TMP_Text label;
    [Range(0.25f, 5f)] public float showTime = 2f;

    Coroutine _r;

    public void Show(string text)
    {
        if (!label) return;
        if (_r != null) StopCoroutine(_r);
        _r = StartCoroutine(ShowCo(text));
    }

    IEnumerator ShowCo(string text)
    {
        label.gameObject.SetActive(true);
        label.text = text;
        yield return new WaitForSecondsRealtime(showTime);
        label.gameObject.SetActive(false);
        _r = null;
    }
}
