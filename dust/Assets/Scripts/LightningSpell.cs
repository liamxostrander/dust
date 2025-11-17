using UnityEngine;

/// Lightning spell that strikes down from above.
/// Spawned above the player and travels downward to deal damage.
public class LightningSpell : MonoBehaviour
{
    [Header("Spell Properties")]
    public int damage = 2;
    public Vector3 targetPosition;
    
    [Header("Animation")]
    public Animator animator;
    public AnimationClip strikeAnimation;
    public float animationDuration = 1f;
    [Range(0f, 1f)]
    public float damageTiming = 0.5f; // When in animation to deal damage (0-1)
    
    [Header("Audio")]
    public AudioClip strikeSound;
    
    private bool hasDealtDamage = false;
    private float spawnTime;
    
    void Start()
    {
        spawnTime = Time.time;
        
        // Position at target and play animation
        transform.position = targetPosition;
        
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        if (animator != null && strikeAnimation != null)
        {
            animator.Play(strikeAnimation.name);
            animationDuration = strikeAnimation.length;
        }
        
        // Play sound at start
        if (strikeSound != null)
        {
            AudioSource.PlayClipAtPoint(strikeSound, transform.position);
        }
        
        // Destroy after animation completes
        Destroy(gameObject, animationDuration);
    }
    
    void Update()
    {
        float elapsed = Time.time - spawnTime;
        
        // Deal damage at the specified timing in the animation
        if (!hasDealtDamage && elapsed >= animationDuration * damageTiming)
        {
            DealDamage();
            hasDealtDamage = true;
        }
    }
    
    private void DealDamage()
    {
        // Check for player in trigger area
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1f);
        
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                IsDamageable damageable = hit.GetComponent<IsDamageable>();
                if (damageable != null)
                {
                    // Calculate knockback direction (away from spell, mostly horizontal)
                    Vector2 knockbackDirection = new Vector2(
                        Mathf.Sign(hit.transform.position.x - transform.position.x),
                        0.5f
                    ).normalized;
                    
                    damageable.TakeDamage(damage, knockbackDirection * 5f);
                    Debug.Log($"Lightning spell hit player for {damage} damage!");
                }
                break;
            }
        }
    }
}
