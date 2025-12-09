using UnityEngine;

[DisallowMultipleComponent]
public class PlayerPassiveRegen : MonoBehaviour
{
    [Header("Regen Settings")]
    [Tooltip("Base regen in HP per second with no upgrades.")]
    [SerializeField] private float baseRegenPerSecond = 0.5f;

    [Tooltip("Seconds after taking damage before regen starts.")]
    [SerializeField] private float delayAfterDamage = 3f;

    private IsDamageable damageable;
    private PlayerUpgrades playerUpgrades;
    private float timeSinceLastDamage = 0f;

    private void Awake()
    {
        damageable = GetComponent<IsDamageable>();
        playerUpgrades = GetComponent<PlayerUpgrades>();

        if (damageable == null)
        {
            Debug.LogWarning("PlayerPassiveRegen: No IsDamageable found on player.");
        }
    }

    private void OnEnable()
    {
        if (damageable != null)
        {
            damageable.OnDamagedWithKnockback.AddListener(OnDamaged);
        }
    }

    private void OnDisable()
    {
        if (damageable != null)
        {
            damageable.OnDamagedWithKnockback.RemoveListener(OnDamaged);
        }
    }

    private void Update()
    {
        if (damageable == null || !damageable.IsAlive)
            return;

        timeSinceLastDamage += Time.deltaTime;

        if (timeSinceLastDamage < delayAfterDamage)
            return;

        float regenRate = baseRegenPerSecond;

        if (playerUpgrades != null)
        {
            regenRate += playerUpgrades.CurrentMods.passiveRegenPerSecond;
        }

        if (regenRate <= 0f)
            return;

        float amount = regenRate * Time.deltaTime;
        damageable.Heal(amount);
    }

    private void OnDamaged(float dmg, Vector2 knockback)
    {
        timeSinceLastDamage = 0f;
    }
}
