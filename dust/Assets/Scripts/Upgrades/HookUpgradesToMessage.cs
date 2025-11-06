using UnityEngine;

public class HookUpgradesToMessage : MonoBehaviour
{
    public PlayerUpgrades playerUpgrades;
    public UpgradeMessage message;

    void Awake()
    {
        if (!playerUpgrades) playerUpgrades = FindFirstObjectByType<PlayerUpgrades>();
        if (!message) message = FindFirstObjectByType<UpgradeMessage>();
        if (playerUpgrades && message)
            playerUpgrades.OnUpgradeApplied += message.Show;
    }
}
