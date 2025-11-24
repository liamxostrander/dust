using System;
using UnityEngine;

public class EnemyAttackDamage : MonoBehaviour
{
    [Header("References")]
    private EnemyStateMachine enemy;
    private Collider2D hitboxCollider;

    [Header("Knockback Settings")]
    public float knockbackForce = 5f;
    public float knockupForce = 2f;

    void Awake()
    {
        enemy = GetComponentInParent<EnemyStateMachine>();
        hitboxCollider = GetComponent<Collider2D>();

        if (hitboxCollider)
            hitboxCollider.enabled = false;
    }


    void OnTriggerEnter2D(Collider2D other)
    {
        if (enemy == null) return;
        
        if (!other.CompareTag("Player")) return;
        
        IsDamageable dmg = other.GetComponentInParent<IsDamageable>();
        if (dmg == null) return;
        
        float damage = enemy.attackDamage;
        Vector2 dir = (other.transform.position - enemy.transform.position).normalized;

        Vector2 knockback = new Vector2(
            dir.x * knockbackForce,
            knockupForce
        );

        dmg.TakeDamage(damage, knockback);

        hitboxCollider.enabled = false;
    }
}
