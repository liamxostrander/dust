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

    private bool isCollected = false;

    private Transform playerTransform;
    private PlayerUpgrades playerUpgrades;

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
}
