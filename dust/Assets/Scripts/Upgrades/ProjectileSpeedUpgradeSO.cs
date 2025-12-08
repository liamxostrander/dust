using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Projectile Speed +%")]
public class ProjectileSpeedUpgradeSO : UpgradeSO
{
    [Range(0f, 2f)] public float bonusPercent = 0.25f;

    public override void Apply(ref PlayerUpgrades.AccumulatedMods mods)
    {
        mods.projectileSpeedMult *= (1f + bonusPercent);
    }

    public override string GetDisplayText()
    {
        string title = !string.IsNullOrEmpty(displayName) ? displayName : name;
        string desc  = $"+{Mathf.RoundToInt(bonusPercent * 100f)}% Arrow Speed";
        return $"{title}\n{desc}";
    }
}
