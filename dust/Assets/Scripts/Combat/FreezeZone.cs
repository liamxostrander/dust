using UnityEngine;
using System.Collections;

public class FreezeZone : MonoBehaviour
{
    private WeaponStats stats;

    public void Initialize(WeaponStats weaponStats)
    {
        stats = weaponStats;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (stats == null)
        {
            Debug.LogError("FreezeZone: stats not assigned!");
            return;
        }

        EnemyStateMachine enemy = other.GetComponent<EnemyStateMachine>();
        if (enemy != null)
        {
            // Apply slow effect
            enemy.ApplyIceSlow(stats.iceSlowAmount, stats.iceSlowDuration);

            // Apply damage (through the enemy's IsDamageable component)
            IsDamageable dmg = enemy.GetComponent<IsDamageable>();
            if (dmg != null)
            {
                dmg.TakeDamage(stats.iceDamage, Vector2.zero); // no knockback
            }
        }
    }

    public void DestroyAfterAnimation(float duration)
    {
        StartCoroutine(DestroySoon(duration));
    }

    IEnumerator DestroySoon(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}
