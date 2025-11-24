using UnityEngine;

/// Enemy bullet projectile that travels in a direction and damages the player on contact.
/// Automatically destroyed after a set lifetime or when hitting the player/terrain.
public class EnemyBullet : MonoBehaviour
{
    [Header("Bullet Properties")]
    public float speed = 10f;
    public int damage = 1;
    public float lifetime = 5f;
    
    [Header("Visual")]
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    
    [Header("Effects")]
    public GameObject hitEffectPrefab; // Optional particle effect on impact
    public AudioClip hitSound;
    
    private Vector2 direction;
    private Rigidbody2D rb;
    private bool hasHit = false;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (animator == null) animator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    void Start()
    {
        Destroy(gameObject, lifetime);
    }
    
    void Update()
    {
        if (direction != Vector2.zero && spriteRenderer != null)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
    
    void FixedUpdate()
    {
        if (!hasHit && rb != null)
        {
            rb.linearVelocity = direction * speed;
        }
    }
    
    public void Initialize(Vector2 shootDirection, int bulletDamage)
    {
        direction = shootDirection.normalized;
        damage = bulletDamage;
        
        if (rb != null)
        {
            rb.linearVelocity = direction * speed;
        }
    }
    
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHit) return;
        
        if (collision.CompareTag("Player"))
        {
            IsDamageable damageable = collision.GetComponent<IsDamageable>();
            if (damageable != null)
            {
                Vector2 knockback = direction * 3f;
                damageable.TakeDamage(damage, knockback);
                Debug.Log($"Enemy bullet hit player for {damage} damage!");
            }
            
            OnHit();
        }
    }
    
    private void OnHit()
    {
        if (hasHit) return;
        hasHit = true;
        
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
        
        // Spawn hit effect
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        }
        
        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, transform.position);
        }
        
        Destroy(gameObject);
    }
}
