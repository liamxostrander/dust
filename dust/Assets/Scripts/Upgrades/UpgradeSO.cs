using UnityEngine;

public abstract class UpgradeSO : ScriptableObject
{
    [TextArea] public string description;
    [Header("Audio (optional)")]
    public AudioClip pickupSfx;
    public abstract void Apply(ref PlayerUpgrades.AccumulatedMods mods);
    public abstract string GetDisplayText();
}
