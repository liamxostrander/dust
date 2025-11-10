using UnityEngine;

public class SwordDamage : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private float damageAmount = 25f;
    [SerializeField] private LayerMask enemyLayer;
    
    [Header("Knockback Settings")]
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private float knockbackUpwardForce = 0.3f; // Upward component
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & enemyLayer) == 0)
            return;
        
        // Deal damage with knockback
        IsDamageable damageable = other.GetComponent<IsDamageable>();
        if (damageable != null && damageable.IsAlive)
        {
            // Calculate direction from player to enemy
            Transform playerTransform = transform.root;
            Vector2 directionToEnemy = (other.transform.position - playerTransform.position).normalized;
            
            // Apply knockback in that direction with upward force
            Vector2 knockback = new Vector2(
                directionToEnemy.x * knockbackForce,
                knockbackUpwardForce * knockbackForce
            );
            
            Debug.Log($"Applying knockback: {knockback}, direction to enemy: {directionToEnemy}");
            
            damageable.TakeDamage(damageAmount, knockback);
        }
    }
}
