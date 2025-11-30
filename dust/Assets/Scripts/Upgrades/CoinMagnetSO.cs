using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Coin Magnet", fileName = "CoinMagnet")]
public class CoinMagnetSO : UpgradeSO
{
    [Header("Coin Magnet")]
    public float extraMagnetRadius = 3f;

    public override void Apply(ref PlayerUpgrades.AccumulatedMods mods)
    {
        mods.coinMagnetRadius += extraMagnetRadius;
    }
}
