using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("Collection Settings")]
    [SerializeField] private int coinValue = 1;
    [SerializeField] private string playerTag = "Player";

    [Header("Audio")]
    [SerializeField] private AudioClip collectSound;

    [Header("Magnet Settings")]
    [Tooltip("Base radius even with no upgrades. Set to 0 if you ONLY want upgrade radius.")]
    [SerializeField] private float baseMagnetRadius = 0f;

    [Tooltip("How fast coins move toward the player when inside magnet radius.")]
    [SerializeField] private float magnetMoveSpeed = 10f;

    [Header("Lifetime")]
    [Tooltip("Total lifetime in seconds before the coin despawns.")]
    [SerializeField] private float lifetimeSeconds = 7f;

    [Tooltip("When to start blinking (seconds from spawn). Must be < lifetimeSeconds.")]
    [SerializeField] private float blinkStartSeconds = 5.5f;

    [Tooltip("Blink interval in seconds at the start of blinking.")]
    [SerializeField] private float initialBlinkInterval = 0.2f;

    [Tooltip("Blink interval in seconds near the end (speeds up as it approaches despawn).")]
    [SerializeField] private float finalBlinkInterval = 0.08f;

    private bool isCollected = false;

    private Transform playerTransform;
    private PlayerUpgrades playerUpgrades;

    // Cache sprite renderers for blinking
    private SpriteRenderer[] spriteRenderers;

    private void Awake()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            playerUpgrades = playerObj.GetComponent<PlayerUpgrades>()
                            ?? playerObj.GetComponentInChildren<PlayerUpgrades>()
                            ?? playerObj.GetComponentInParent<PlayerUpgrades>();
        }
        else
        {
            Debug.LogWarning($"Coin: Couldn't find player with tag '{playerTag}'. Magnet behavior disabled.");
        }

        // Cache all SpriteRenderers on this coin (including children)
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
    }

    private void OnEnable()
    {
        // Start lifetime routine
        if (lifetimeSeconds > 0f)
            StartCoroutine(LifetimeRoutine());
    }

    private void Update()
    {
        if (isCollected || playerTransform == null || playerUpgrades == null)
            return;

        // Total magnet radius = base + upgrades (CoinMagnetSO modifies this)
        float magnetRadius = baseMagnetRadius + playerUpgrades.CoinMagnetRadius;
        if (magnetRadius <= 0f)
            return;

        Vector2 toPlayer = playerTransform.position - transform.position;
        float sqrDist = toPlayer.sqrMagnitude;
        float sqrRadius = magnetRadius * magnetRadius;

        if (sqrDist <= sqrRadius)
        {
            Vector2 dir = toPlayer.normalized;
            transform.position += (Vector3)(dir * magnetMoveSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected)
            return;

        bool isPlayer = other.CompareTag(playerTag);
        if (!isPlayer && other.transform.parent != null)
        {
            isPlayer = other.transform.parent.CompareTag(playerTag);
        }

        if (isPlayer)
        {
            PlayerCurrency currency =
                other.GetComponentInParent<PlayerCurrency>() ??
                other.GetComponent<PlayerCurrency>();

            CollectCoin(currency);
        }
    }

    private void CollectCoin(PlayerCurrency currency)
    {
        isCollected = true;

        // Ensure visible when collected
        SetRenderersEnabled(true);

        if (collectSound != null && GlobalAudio.SFX != null)
        {
            GlobalAudio.SFX.PlayOneShot(collectSound);
        }

        if (currency != null)
        {
            currency.AddCoins(coinValue);
        }
        else
        {
            Debug.LogWarning("Coin collected, but no PlayerCurrency found on player!");
        }

        Destroy(gameObject);
    }

    public int GetCoinValue()
    {
        return coinValue;
    }

    // --- Lifetime & Blinking ---
    private System.Collections.IEnumerator LifetimeRoutine()
    {
        // Clamp settings
        blinkStartSeconds = Mathf.Clamp(blinkStartSeconds, 0f, Mathf.Max(0f, lifetimeSeconds - 0.01f));
        initialBlinkInterval = Mathf.Max(0.01f, initialBlinkInterval);
        finalBlinkInterval = Mathf.Max(0.01f, finalBlinkInterval);

        float t = 0f;

        // Wait until blink start
        while (!isCollected && t < blinkStartSeconds)
        {
            t += Time.deltaTime;
            yield return null;
        }

        // Blinking until lifetime ends
        float elapsedBlink = 0f;
        bool visible = true;
        SetRenderersEnabled(true);

        while (!isCollected && t < lifetimeSeconds)
        {
            // Lerp blink interval from initial to final as we approach despawn
            float remaining = Mathf.Max(0f, lifetimeSeconds - t);
            float totalBlinkDuration = Mathf.Max(0.01f, lifetimeSeconds - blinkStartSeconds);
            float lerp = totalBlinkDuration > 0f ? 1f - (remaining / totalBlinkDuration) : 1f;
            float interval = Mathf.Lerp(initialBlinkInterval, finalBlinkInterval, lerp);

            // Toggle visibility
            visible = !visible;
            SetRenderersEnabled(visible);

            // Wait interval
            float waited = 0f;
            while (!isCollected && waited < interval)
            {
                float dt = Time.deltaTime;
                waited += dt;
                t += dt;
                elapsedBlink += dt;
                yield return null;
            }
        }

        if (!isCollected)
        {
            // Ensure hidden right before despawn for effect
            SetRenderersEnabled(false);
            Destroy(gameObject);
        }
    }

    private void SetRenderersEnabled(bool enabled)
    {
        if (spriteRenderers == null || spriteRenderers.Length == 0) return;
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null) spriteRenderers[i].enabled = enabled;
        }
    }
}
