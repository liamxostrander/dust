using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerUpgrades : MonoBehaviour
{
    [System.Serializable]
    public struct AccumulatedMods
    {
        public float speedMult;
        public float jumpMult;
    }

    public PlayerMovementSM movement;
    public List<UpgradeSO> acquired = new();

    public AccumulatedMods CurrentMods { get; private set; }

    public System.Action<string> OnUpgradeApplied;

    [Header("Audio")]
    public AudioSource sfxSource;
    public AudioClip defaultPickupSfx;
    [Range(0f, 1f)] public float pickupVolume = 1f;

    void Awake()
    {
        if (!movement) movement = GetComponent<PlayerMovementSM>();
        Recalculate();
    }

    public void AddUpgrade(UpgradeSO u)
    {
        if (u == null) return;
        acquired.Add(u);
        Recalculate();
        OnUpgradeApplied?.Invoke(u.GetDisplayText());
        AudioClip clip = u.pickupSfx ? u.pickupSfx : defaultPickupSfx;
        if (clip)
        {
            if (sfxSource)
            {
                sfxSource.PlayOneShot(clip, pickupVolume);
            }
            else
            {
                AudioSource.PlayClipAtPoint(clip, transform.position, pickupVolume);
            }
        }
    }

    public void RemoveUpgrade(UpgradeSO u)
    {
        if (u == null) return;
        if (acquired.Remove(u))
        {
            Recalculate();
        }
    }

    public void Recalculate()
    {
        var m = new AccumulatedMods { speedMult = 1f, jumpMult = 1f };

        foreach (var u in acquired)
            if (u) u.Apply(ref m);

        if (movement)
        {
            movement.speedMultiplier = m.speedMult;
            movement.jumpMultiplier  = m.jumpMult;
        }
        CurrentMods = m;
    }
}
