using UnityEngine;
using System.Collections;

public class HitStop : MonoBehaviour
{
    public static HitStop Instance;

    private bool isFrozen = false;

    void Awake()
    {
        Instance = this;
    }

    public void DoHitstopGlobal(float duration)
    {
        StartCoroutine(DoHitStopRoutine(duration));
    }

    private IEnumerator DoHitStopRoutine(float duration)
    {
        if (isFrozen) yield break;
        isFrozen = true;

        float original = Time.timeScale;
        Time.timeScale = 0f;

        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = original;
        isFrozen = false;
    }
}
