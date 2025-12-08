using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private float riseSpeed = 1.0f;
    [SerializeField] private float lifetime = 0.8f;
    [SerializeField] private Vector2 randomOffset = new Vector2(0.2f, 0.2f);
    [SerializeField] private AnimationCurve alphaCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    private float timer;

    public void Setup(float amount, Color color)
    {
        if (text == null) text = GetComponentInChildren<TMP_Text>();
        if (text == null) return;

        text.text = Mathf.RoundToInt(amount).ToString();
        text.color = color;
        transform.position += (Vector3)new Vector2(
            Random.Range(-randomOffset.x, randomOffset.x),
            Random.Range(0f, randomOffset.y)
        );
    }

    void Update()
    {
        timer += Time.deltaTime;
        transform.position += Vector3.up * riseSpeed * Time.deltaTime;

        if (text != null)
        {
            Color c = text.color;
            c.a = alphaCurve.Evaluate(timer / lifetime);
            text.color = c;
        }

        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}