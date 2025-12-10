using System;
using UnityEngine;

public class EnemyAttackDamage : MonoBehaviour
{
    [Header("References")]
    private EnemyStateMachine enemy;
    private Collider2D hitboxCollider;

    // Optional attacker reference (e.g., set to the caster or the spell root)
    public Transform attacker;

    [Header("Damage")]
    // Used if EnemyStateMachine is missing
    public float overrideDamage = 10f;

    [Header("Knockback Settings")]
    public float knockbackForce = 5f;
    public float knockupForce = 2f;

    [Header("Knockback Options")]
    public bool horizontalOnly = false;
    public float verticalOverride = -1f;

    void Awake()
    {
        enemy = GetComponentInParent<EnemyStateMachine>();
        hitboxCollider = GetComponent<Collider2D>();

        if (hitboxCollider)
            hitboxCollider.enabled = false;
    }

    public void EnableHitbox()
    {
        if (hitboxCollider)
            hitboxCollider.enabled = true;
    }

    public void DisableHitbox()
    {
        if (hitboxCollider)
            hitboxCollider.enabled = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log(other + " " + other.tag);

        if (!other.CompareTag("Player")) return;

        IsDamageable dmg = other.GetComponentInParent<IsDamageable>();
        if (dmg == null) return;

        // Damage: use enemy.attackDamage if available, otherwise overrideDamage
        float damage = (enemy != null) ? enemy.attackDamage : overrideDamage;

        // Knockback direction: from attacker (preferred) or this hitbox, toward the player
        Vector3 sourcePos = (attacker != null) ? attacker.position : transform.position;
        Vector2 dir = (other.transform.position - sourcePos).normalized;

        float kbX = dir.x * knockbackForce;
        float kbY;

        if (horizontalOnly)
        {
            kbY = Mathf.Max(0f, knockupForce);
        }
        else if (verticalOverride >= 0f)
        {
            kbY = verticalOverride;
        }
        else
        {
            kbY = Mathf.Max(0f, knockupForce);
        }

        Vector2 knockback = new Vector2(kbX, kbY);

        dmg.TakeDamage(damage, knockback);
    }
}
