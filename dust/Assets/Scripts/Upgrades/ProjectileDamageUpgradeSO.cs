using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Ranged Damage +%")]
public class RangedDamageUpgradeSO : UpgradeSO
{
    [Range(0f, 2f)] public float bonusPercent = 0.20f;

    public override void Apply(ref PlayerUpgrades.AccumulatedMods mods)
    {
        mods.rangedDamageMult *= (1f + bonusPercent);
    }

    public override string GetDisplayText()
    {
        string title = !string.IsNullOrEmpty(displayName) ? displayName : name;
        string desc  = $"+{Mathf.RoundToInt(bonusPercent * 100f)}% Bow Damage";
        return $"{title}\n{desc}";
    }
}
