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
