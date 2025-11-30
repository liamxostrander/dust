using UnityEngine;

public abstract class UpgradeSO : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Short display name, e.g. 'Rocket Boots'")]
    public string displayName;

    [TextArea]
    [Tooltip("Description shown under the name, e.g. '+30% Jump Height'")]
    public string description;

    [Header("Visuals")]
    [Tooltip("Sprite used for the floating pickup above the chest.")]
    public Sprite worldSprite;

    [Header("Audio (optional)")]
    public AudioClip pickupSfx;

    public abstract void Apply(ref PlayerUpgrades.AccumulatedMods mods);

    public virtual string GetDisplayText()
    {
        string title = !string.IsNullOrEmpty(displayName) ? displayName : name;

        if (!string.IsNullOrEmpty(description))
            return $"{title}\n{description}";

        return title;
    }
}
