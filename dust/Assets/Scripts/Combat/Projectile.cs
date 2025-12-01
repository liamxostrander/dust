using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float lifetime = 3f;
    public float damage = 20f;
    private LayerMask hitLayers;
    [Header("Hit FX")]
    [SerializeField] private GameObject hitEffectPrefab;

    [Header("Impact Feedback")]
    [SerializeField] private float hitstopDuration = 0.06f;
    [SerializeField] private float screenShakeMagnitude = 0.1f;
    [SerializeField] private float screenShakeDuration = 0.08f;
    [SerializeField] private AudioClip hitSound;

    Rigidbody2D rb;
    private HitStop hitStop;
    private const int LAYER_GROUND = 6;
    private const int LAYER_ENEMY = 8;
    private bool hasHit = false;
    void Start()
    {
        if (hitLayers == 0)
        {
            hitLayers = (1 << LAYER_GROUND) | (1 << LAYER_ENEMY);
        }
        hitStop = FindFirstObjectByType<HitStop>();
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (rb != null)
        {
            Vector2 v = rb.linearVelocity;

            if (v.sqrMagnitude > 0.01f)
            {
                float angle = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if ((hitLayers.value & (1 << other.gameObject.layer)) == 0)
            return;

        if (hasHit) 
            return;
        hasHit = true;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        bool hitEnemy = other.gameObject.layer == 8;
        IsDamageable dmg = other.GetComponent<IsDamageable>();
        if (dmg != null && dmg.IsAlive)
        {
            dmg.TakeDamage(damage);
        }

        if (hitEffectPrefab != null)
        {
            Vector2 hitPoint = other.ClosestPoint(transform.position);
            GameObject fx = Instantiate(hitEffectPrefab, hitPoint, Quaternion.identity);

            ParticleSystem ps = fx.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                Destroy(fx, ps.main.duration + ps.main.startLifetime.constantMax);
            }
            else
            {
                Destroy(fx, 1f);
            }
        }

        if (hitSound != null && PlayerMovementSM.GlobalSFXSource != null)
            PlayerMovementSM.GlobalSFXSource.PlayOneShot(hitSound);

        if (hitEnemy){
            if (hitEnemy && HitStop.Instance != null)
                HitStop.Instance.DoHitstopGlobal(hitstopDuration);

            if (CameraShake.Instance != null)
                CameraShake.Instance.ShakeOnce(screenShakeDuration, screenShakeMagnitude);
        }

        Destroy(gameObject);
    }
}
