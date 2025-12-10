using UnityEngine;

public class FireballProjectile : MonoBehaviour
{
    [Header("Movement")]
    public float horizontalSpeed = 7f;
    public float bounceForce = 7f;
    public float maxLifetime = 3f;

    [Header("Damage")]
    public float damage = 20f;
    public float knockback = 4f;

    [Header("Effects")]
    public GameObject hitEffect;

    private Rigidbody2D rb;
    private Vector2 direction;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Launch(Vector2 initialDir)
    {
        direction = initialDir.normalized;

        // Initial hop, like Mario fireball
        rb.linearVelocity = new Vector2(direction.x * horizontalSpeed,
                                        bounceForce * 0.6f);

        Destroy(gameObject, maxLifetime);
    }

    void FixedUpdate()
    {
        // Maintain constant horizontal speed
        rb.linearVelocity = new Vector2(direction.x * horizontalSpeed,
                                        rb.linearVelocity.y);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // ---------- 1) ENEMY HIT CHECK ----------
        IsDamageable dmg = collision.collider.GetComponent<IsDamageable>();
        if (dmg != null)
        {
            // Apply damage + knockback
            dmg.TakeDamage(damage, direction * knockback);

            // Hit VFX
            if (hitEffect != null)
                Instantiate(hitEffect, transform.position, Quaternion.identity);

            // Destroy fireball on enemy hit
            Destroy(gameObject);
            return; // Don't do bounce logic
        }

        // ---------- 2) BOUNCE LOGIC (GROUND / WALLS) ----------
        foreach (var contact in collision.contacts)
        {
            Vector2 normal = contact.normal;

            // Ground bounce (normal pointing up)
            if (normal.y > 0.5f)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, bounceForce);
            }
            // Wall bounce (normal mostly horizontal)
            else if (Mathf.Abs(normal.x) > 0.5f)
            {
                // Flip horizontal direction
                direction.x *= -1;
                rb.linearVelocity = new Vector2(direction.x * horizontalSpeed,
                                                rb.linearVelocity.y);
            }
        }
    }
}
