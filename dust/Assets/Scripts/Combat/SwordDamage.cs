using UnityEngine;

public class SwordDamage : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private float damageAmount = 25f;
    [SerializeField] private LayerMask enemyLayer;
    
    [Header("Knockback Settings")]
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private Vector2 knockbackDirection = new Vector2(1f, 0.3f);
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if it's an enemy
        if (((1 << other.gameObject.layer) & enemyLayer) == 0)
            return;
        
        // Deal damage with knockback
        IsDamageable damageable = other.GetComponent<IsDamageable>();
        if (damageable != null && damageable.IsAlive)
        {
            // Calculate knockback
            Transform playerTransform = transform.root;
            float facingDirection = Mathf.Sign(playerTransform.localScale.x);
            
            Vector2 knockback = new Vector2(
                knockbackDirection.x * facingDirection * knockbackForce,
                knockbackDirection.y * knockbackForce
            );
            
            Debug.Log($"Applying knockback: {knockback}, facing: {facingDirection}, force: {knockbackForce}");
            
            damageable.TakeDamage(damageAmount, knockback);
        }
    }
}
