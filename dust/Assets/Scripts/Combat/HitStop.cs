using UnityEngine;
using System.Collections;


public class HitStop : MonoBehaviour
{
    private bool isFrozen = false;

    public IEnumerator DoHitStop(float duration)
    {
        if (isFrozen) yield break; // prevent stacking
        isFrozen = true;

        float originalTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = originalTimeScale;
        isFrozen = false;
    }
}

