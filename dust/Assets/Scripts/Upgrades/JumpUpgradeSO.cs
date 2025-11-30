using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Jump +%")]
public class JumpUpgradeSO : UpgradeSO
{
    [Range(0f, 2f)] public float bonusPercent = 0.10f;

    public override void Apply(ref PlayerUpgrades.AccumulatedMods mods)
    {
        mods.jumpMult *= (1f + bonusPercent);
    }

    public override string GetDisplayText()
    {
        string title = !string.IsNullOrEmpty(displayName) ? displayName : name;
        string desc = $"+{Mathf.RoundToInt(bonusPercent * 100f)}% Jump Height";
        return $"{title}\n{desc}";
    }
}
