using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Melee Damage +%", fileName = "MeleeDamageUpgrade")]
public class MeleeDamageUpgradeSO : UpgradeSO
{
    [Range(0f, 2f)] public float bonusPercent = 0.20f;

    public override void Apply(ref PlayerUpgrades.AccumulatedMods mods)
    {
        mods.meleeDamageMult *= (1f + bonusPercent);
    }

    public override string GetDisplayText()
    {
        string title = !string.IsNullOrEmpty(displayName) ? displayName : name;
        string desc  = $"+{Mathf.RoundToInt(bonusPercent * 100f)}% Melee Damage";
        return $"{title}\n{desc}";
    }
}
