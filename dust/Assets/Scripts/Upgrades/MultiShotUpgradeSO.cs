using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Multi Shot")]
public class MultiShotUpgradeSO : UpgradeSO
{
    [Header("Multi Shot Settings")]
    [Tooltip("Extra projectiles in addition to the main one. 2 => 3 arrows total.")]
    public int extraProjectiles = 2;

    [Tooltip("Angle between neighboring arrows in degrees.")]
    public float spreadPerProjectileDeg = 7.5f;

    public override void Apply(ref PlayerUpgrades.AccumulatedMods mods)
    {
        mods.extraProjectiles += extraProjectiles;
        mods.projectileSpreadAngleDeg = Mathf.Max(
            mods.projectileSpreadAngleDeg,
            spreadPerProjectileDeg
        );
    }

    public override string GetDisplayText()
    {
        string title = !string.IsNullOrEmpty(displayName) ? displayName : name;
        string desc  = "Fire 3 arrows in a spread";
        return $"{title}\n{desc}";
    }
}
