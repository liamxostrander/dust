using UnityEngine;
using UnityEngine.Events;

public class IsDamageable : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    
    [Header("Damage Settings")]
    [SerializeField] private bool isInvulnerable = false;
    [SerializeField] private float invulnerabilityDuration = 0.5f;
    private float invulnerabilityTimer = 0f;
    
    [Header("Events")]
    public UnityEvent<float, Vector2> OnDamagedWithKnockback; // Combined event
    public UnityEvent<float> OnHealed;
    public UnityEvent OnDeath;
    public UnityEvent<float> OnHealthChanged;
    
    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public bool IsAlive => currentHealth > 0;
    public float HealthPercentage => currentHealth / maxHealth;
    
    void Start()
    {
        currentHealth = maxHealth;
    }
    
    void Update()
    {
        if (invulnerabilityTimer > 0)
        {
            invulnerabilityTimer -= Time.deltaTime;
        }
    }
    
    public void TakeDamage(float damage, Vector2 knockback = default)
    {
        if (!IsAlive || isInvulnerable || invulnerabilityTimer > 0)
            return;
        
        currentHealth = Mathf.Max(0, currentHealth - damage);
        
        // Invoke combined event with both damage and knockback
        OnDamagedWithKnockback?.Invoke(damage, knockback);
        OnHealthChanged?.Invoke(currentHealth);
        
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Trigger brief invulnerability
            invulnerabilityTimer = invulnerabilityDuration;
        }
    }
    
    public void Heal(float amount)
    {
        if (!IsAlive)
            return;
        
        float healedAmount = Mathf.Min(amount, maxHealth - currentHealth);
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealed?.Invoke(healedAmount);
        OnHealthChanged?.Invoke(currentHealth);
    }
    
    public void SetInvulnerable(bool invulnerable)
    {
        isInvulnerable = invulnerable;
    }
    
    private void Die()
    {
        OnDeath?.Invoke();
    }
    
    public void ResetHealth()
    {
        currentHealth = maxHealth;
        invulnerabilityTimer = 0f;
        OnHealthChanged?.Invoke(currentHealth);
    }
}
