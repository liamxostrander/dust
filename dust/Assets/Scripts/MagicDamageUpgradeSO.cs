using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Magic Damage +%", fileName = "MagicDamageUpgrade")]
public class MagicDamageUpgradeSO : UpgradeSO
{
    [Range(0f, 2f)] public float bonusPercent = 0.20f;

    public override void Apply(ref PlayerUpgrades.AccumulatedMods mods)
    {
        mods.magicDamageMult *= (1f + bonusPercent);
    }

    public override string GetDisplayText()
    {
        string title = !string.IsNullOrEmpty(displayName) ? displayName : name;
        string desc  = $"+{Mathf.RoundToInt(bonusPercent * 100f)}% Magic Damage";
        return $"{title}\n{desc}";
    }
}
