using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Speed +%")]
public class SpeedUpgradeSO : UpgradeSO
{
    [Range(0f, 2f)] public float bonusPercent = 0.10f;

    public override void Apply(ref PlayerUpgrades.AccumulatedMods mods)
    {
        mods.speedMult *= (1f + bonusPercent);
    }

    public override string GetDisplayText()
    {
        string title = !string.IsNullOrEmpty(displayName) ? displayName : name;
        string desc = $"+{Mathf.RoundToInt(bonusPercent * 100f)}% Move Speed";
        return $"{title}\n{desc}";
    }
}
