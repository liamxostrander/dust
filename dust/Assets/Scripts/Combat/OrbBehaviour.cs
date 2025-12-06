using UnityEngine;
using System.Collections;
using System.Linq;

public class OrbBehaviour : MonoBehaviour
{
    private WeaponStats stats;
    private GameObject player;

    private Rigidbody2D rb;

    private enum OrbState { Hover, Seek, Explode }
    private OrbState state = OrbState.Hover;

    private float hoverYOffset;
    private float detectRadius;
    private float speed;

    public void Initialize(WeaponStats s, GameObject p)
    {
        stats = s;
        player = p;

        hoverYOffset = s.orbHoverHeight;
        detectRadius = s.orbDetectRadius;
        speed = s.orbSpeed;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        switch (state)
        {
            case OrbState.Hover:
                Hover();
                TryFindTarget();
                break;

            case OrbState.Seek:
                SeekTarget();
                break;
        }
    }

    // -------------------------
    //  HOVER ABOVE PLAYER
    // -------------------------
    void Hover()
    {
        Vector2 targetPos = (Vector2)player.transform.position + Vector2.up * hoverYOffset;
        transform.position = Vector2.Lerp(transform.position, targetPos, 6f * Time.deltaTime);
    }

    // -------------------------
    //  SEARCH FOR ENEMIES
    // -------------------------
    Collider2D target;

    void TryFindTarget()
    {
        int enemyLayer = 8; // Based on your layer screenshot

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectRadius, 1 << enemyLayer);
        if (hits.Length > 0)
        {
            target = hits[0];
            state = OrbState.Seek;
        }
    }

    // -------------------------
    //  SEEK TARGET
    // -------------------------
    void SeekTarget()
    {
        if (target == null)
        {
            state = OrbState.Hover;
            return;
        }

        Vector2 dir = (target.transform.position - transform.position).normalized;
        rb.linearVelocity = dir * speed;

        float dist = Vector2.Distance(transform.position, target.transform.position);
        if (dist <= 0.5f)
        {
            StartCoroutine(Explode());
        }
    }

    // -------------------------
    //  EXPLOSION
    // -------------------------
    IEnumerator Explode()
    {
    state = OrbState.Explode;
    rb.linearVelocity = Vector2.zero;

    // ---------------------------------------------------
    //  VISUAL EXPLOSION FX
    // ---------------------------------------------------
    if (stats.orbExplosionFX != null)
    {
        GameObject fx = Instantiate(stats.orbExplosionFX, transform.position, Quaternion.identity);

        // Optional: auto-destroy after particle duration
        ParticleSystem ps = fx.GetComponent<ParticleSystem>();
        if (ps != null)
            Destroy(fx, ps.main.duration + ps.main.startLifetime.constantMax);
        else
            Destroy(fx, 1f);
    }

    // ---------------------------------------------------
    //  EXPLOSION SOUND
    // ---------------------------------------------------
    if (stats.orbExplosionSFX != null)
    {
        AudioSource s = PlayerMovementSM.GlobalSFXSource;
        s.pitch = 1f;
        s.spatialBlend = 0f; // 2D sound
        s.clip = stats.orbExplosionSFX;
        s.Play();
    }

    // ---------------------------------------------------
    //  APPLY DAMAGE
    // ---------------------------------------------------
    int enemyLayer = 8;

    Collider2D[] hits = Physics2D.OverlapCircleAll(
        transform.position,
        stats.orbExplosionRadius,
        1 << enemyLayer
    );

    foreach (var h in hits)
    {
        IsDamageable dmg = h.GetComponent<IsDamageable>();
        if (dmg != null)
            dmg.TakeDamage(stats.orbExplosionDamage);
    }

    // ---------------------------------------------------
    //  HITSTOP + SCREEN SHAKE
    // ---------------------------------------------------
    if (hits.Length > 0)
    {
        HitStop.Instance?.DoHitstopGlobal(0.08f);
        CameraShake.Instance?.ShakeOnce(0.12f, 0.18f);
    }

    yield return null;

    Destroy(gameObject); // Remove orb
}


    // -------------------------
    //  Gizmos for debug
    // -------------------------
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectRadius);

        if (stats != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, stats.orbExplosionRadius);
        }
    }
}
