using UnityEngine;

public class SwordDamage : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private float damageAmount = 25f;
    [SerializeField] private LayerMask enemyLayer;
    
    [Header("Knockback Settings")]
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private float knockbackUpwardForce = 0.3f; // Upward component
    [SerializeField] private float playerRecoilForce = 2000f;   // Recoil applied to player
    [SerializeField] private float screenShakeMagnitude = 0.15f;
    [SerializeField] private float screenShakeDuration = 0.1f;
    private bool appliedRecoil;

    [Header("Hit SFX")]
    public AudioClip hitSound; 
    private HitStop hitStop;
    private Rigidbody2D playerRb;
    private WeaponStats stats;

    // NEW
    private PlayerUpgrades playerUpgrades;

    [Header("Hit FX")]
    [SerializeField] private GameObject hitEffectPrefab;

    void Start() {
        hitStop = FindFirstObjectByType<HitStop>();
        stats   = FindFirstObjectByType<WeaponStats>();
        playerRb = transform.root.GetComponent<Rigidbody2D>();
        playerUpgrades = transform.root.GetComponent<PlayerUpgrades>();
        if (playerUpgrades == null)
            playerUpgrades = FindFirstObjectByType<PlayerUpgrades>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & enemyLayer) == 0)
            return;
        
        IsDamageable damageable = other.GetComponent<IsDamageable>();
        if (damageable != null && damageable.IsAlive)
        {
            Vector2 hitPoint = other.ClosestPoint(transform.position);
            GameObject effect = Instantiate(hitEffectPrefab, hitPoint, Quaternion.identity);
            ParticleSystem ps = effect.GetComponent<ParticleSystem>();

            if (ps != null)
            {
                Destroy(effect, ps.main.duration + ps.main.startLifetime.constantMax);
            }
            else
            {
                Destroy(effect, 1f);
            }

            HitStop.Instance.DoHitstopGlobal(stats.hitstopDuration);
            if (CameraShake.Instance != null)
                CameraShake.Instance.ShakeOnce(screenShakeDuration, screenShakeMagnitude);

            Transform playerTransform = transform.root;
            Vector2 directionToEnemy = (other.transform.position - playerTransform.position).normalized;
            
            Vector2 knockback = new Vector2(
                directionToEnemy.x * knockbackForce,
                knockbackUpwardForce * knockbackForce
            );

            float finalDamage = damageAmount;
            if (playerUpgrades != null)
                finalDamage *= playerUpgrades.CurrentMods.meleeDamageMult;

            damageable.TakeDamage(finalDamage, knockback);
            
            if (!appliedRecoil)
            {
                Vector2 recoil = -directionToEnemy * playerRecoilForce;
                playerRb.linearVelocity = new Vector2(0f, playerRb.linearVelocity.y);
                playerRb.AddForce(recoil, ForceMode2D.Impulse);
                playerRb.GetComponent<PlayerMovementSM>()?.DisableMovement(0.1f);
                
                appliedRecoil = true;
            }

            PlayerMovementSM.GlobalSFXSource.PlayOneShot(hitSound);
        }
    }

    public void ResetRecoil()
    {
        appliedRecoil = false;
    }
}
