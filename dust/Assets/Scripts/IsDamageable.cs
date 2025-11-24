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
    
    [Header("Coin Drop")]
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private int minCoins = 1;
    [SerializeField] private int maxCoins = 5;
    [SerializeField] private float coinDropForce = 3f;
    [SerializeField] private float coinDropRadius = 0.5f;
    
    [Header("Events")]
    public UnityEvent<float, Vector2> OnDamagedWithKnockback; 
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
        DropCoins();
        OnDeath?.Invoke();
    }
    
    private void DropCoins()
    {
        if (coinPrefab == null)
            return;
        
        int coinCount = Random.Range(minCoins, maxCoins + 1);
        
        for (int i = 0; i < coinCount; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * coinDropRadius;
            Vector3 spawnPosition = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0);
            
            GameObject coin = Instantiate(coinPrefab, spawnPosition, Quaternion.identity);
            
            Rigidbody2D rb = coin.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 randomDirection = Random.insideUnitCircle.normalized;
                float upwardBias = Random.Range(0.5f, 1.5f); 
                Vector2 throwDirection = new Vector2(randomDirection.x, Mathf.Abs(randomDirection.y) + upwardBias).normalized;
                
                rb.AddForce(throwDirection * coinDropForce, ForceMode2D.Impulse);
            }
        }
    }
    
    public void ResetHealth()
    {
        currentHealth = maxHealth;
        invulnerabilityTimer = 0f;
        OnHealthChanged?.Invoke(currentHealth);
    }
}
