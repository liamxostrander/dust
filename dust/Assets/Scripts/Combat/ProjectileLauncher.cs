using UnityEngine;

public class ProjectileLauncher : MonoBehaviour
{
    [Header("References")]
    private WeaponStats stats;

    private PlayerUpgrades playerUpgrades;

    void Awake()
    {
        stats = GetComponent<WeaponStats>();
        playerUpgrades = FindFirstObjectByType<PlayerUpgrades>();
        if (stats == null)
        {
            Debug.LogError("ProjectileLauncher requires WeaponStats on the same object!");
        }
    }

    public void LaunchProjectile(Vector2 attackDir, bool isFlipped)
    {
        if (stats.projectilePrefab == null || stats.projectileSpawnPoint == null)
        {
            Debug.LogWarning("ProjectileLauncher missing prefab or spawn point.");
            return;
        }

        if (attackDir.sqrMagnitude < 0.001f)
            attackDir = Vector2.right;

        int extraProjectiles = 0;
        float spreadAngleDeg = 0f;
        float speedMult = 1f;

        if (playerUpgrades != null)
        {
            var mods = playerUpgrades.CurrentMods;
            extraProjectiles      = Mathf.Max(0, mods.extraProjectiles);
            spreadAngleDeg        = mods.projectileSpreadAngleDeg;
            speedMult             = mods.projectileSpeedMult;
        }

        int projectileCount = 1 + extraProjectiles;

        float baseAngle = Mathf.Atan2(attackDir.y, attackDir.x) * Mathf.Rad2Deg;

        float startOffset = -(spreadAngleDeg * (projectileCount - 1) * 0.5f);

        for (int i = 0; i < projectileCount; i++)
        {
            float finalAngle = baseAngle + startOffset + spreadAngleDeg * i;

            Vector2 dir = new Vector2(
                Mathf.Cos(finalAngle * Mathf.Deg2Rad),
                Mathf.Sin(finalAngle * Mathf.Deg2Rad)
            ).normalized;

            GameObject proj = Instantiate(
                stats.projectilePrefab,
                stats.projectileSpawnPoint.position,
                Quaternion.Euler(0f, 0f, finalAngle)
            );

            Rigidbody2D rb = proj.GetComponent<Rigidbody2D>();
            float speed = stats.projectileSpeed * speedMult;

            if (rb != null)
                rb.linearVelocity = dir * speed;
        }
    }
}
