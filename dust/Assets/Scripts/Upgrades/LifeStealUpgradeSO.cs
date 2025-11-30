using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Life Steal")]
public class LifeStealUpgradeSO : UpgradeSO
{
    [Min(0f)] public float healAmountPerKill = 5f;

    public override void Apply(ref PlayerUpgrades.AccumulatedMods mods)
    {
        mods.healOnKill += healAmountPerKill;
    }

    public override string GetDisplayText()
    {
        string title = !string.IsNullOrEmpty(displayName) ? displayName : name;
        string desc = "Kills Heal You";
        return $"{title}\n{desc}";
    }
}
