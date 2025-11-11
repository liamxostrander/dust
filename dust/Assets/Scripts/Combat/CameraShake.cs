using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    [SerializeField] private float defaultDuration = 0.1f;
    [SerializeField] private float defaultMagnitude = 0.1f;
    private Vector3 originalPos;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public IEnumerator Shake(float duration, float magnitude)
    {
        originalPos = transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = originalPos + new Vector3(x, y, 0f);
            elapsed += Time.unscaledDeltaTime; // unaffected by hit stop
            yield return null;
        }

        transform.localPosition = originalPos;
    }

    public void ShakeOnce(float duration = -1f, float magnitude = -1f)
    {
        StartCoroutine(Shake(
            duration > 0 ? duration : defaultDuration,
            magnitude > 0 ? magnitude : defaultMagnitude
        ));
    }
}
