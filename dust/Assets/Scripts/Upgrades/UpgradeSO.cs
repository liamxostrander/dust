using UnityEngine;

public abstract class UpgradeSO : ScriptableObject
{
    [TextArea] public string description;
    public abstract void Apply(ref PlayerUpgrades.AccumulatedMods mods);
    public abstract string GetDisplayText();
}
