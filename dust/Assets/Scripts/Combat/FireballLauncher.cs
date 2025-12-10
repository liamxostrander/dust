using UnityEngine;

public class FireballLauncher : MonoBehaviour
{
    private WeaponStats stats;
    private float nextFireTime = 0f;

    public void TryLaunchFireball(Vector2 direction)
    {
        if (stats == null)
            stats = GetComponent<WeaponStats>();

        if (stats.fireballPrefab == null)
            return;

        // Timestamp cooldown instead of Update()
        if (Time.time < nextFireTime)
        {
            Debug.Log("Cooldown, remaining: " + (nextFireTime - Time.time));
            return;
        }

        Vector2 spawnPos = (Vector2)transform.position + direction.normalized * 1.2f;

        GameObject fireball = Instantiate(stats.fireballPrefab, spawnPos, Quaternion.identity);

        FireballProjectile fp = fireball.GetComponent<FireballProjectile>();
        if (fp != null)
            fp.Launch(direction);

        // Set next allowed fire time
        nextFireTime = Time.time + stats.fireballCooldown;
    }
}
