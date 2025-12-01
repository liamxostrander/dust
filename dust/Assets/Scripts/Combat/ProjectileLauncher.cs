using UnityEngine;

public class ProjectileLauncher : MonoBehaviour
{
    [Header("References")]
    private WeaponStats stats;

    void Awake()
    {
        stats = GetComponent<WeaponStats>();
        if (stats == null)
        {
            Debug.LogError("ProjectileLauncher requires WeaponStats on the same object!");
        }
    }

    // Called from SlashState
    public void LaunchProjectile(Vector2 attackDir, bool isFlipped)
    {
        if (stats.projectilePrefab == null || stats.projectileSpawnPoint == null)
        {
            Debug.LogWarning("ProjectileLauncher missing prefab or spawn point.");
            return;
        }

        // Spawn
        GameObject proj = Instantiate(
            stats.projectilePrefab,
            stats.projectileSpawnPoint.position,
            Quaternion.identity
        );

        // Align rotation
        float angle = Mathf.Atan2(attackDir.y, attackDir.x) * Mathf.Rad2Deg;
        proj.transform.rotation = Quaternion.Euler(0, 0, angle);

        // Velocity
        Rigidbody2D rb = proj.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.linearVelocity = attackDir.normalized * stats.projectileSpeed;

        // Flip X if facing left (optional)
        if (isFlipped)
            proj.transform.rotation = Quaternion.Euler(0, 0, angle + 180f);
    }
}
