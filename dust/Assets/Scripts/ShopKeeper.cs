using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Shopkeeper : MonoBehaviour
{
    public enum WeaponCategory { Melee, Ranged, Magic }

    [System.Serializable]
    public class ShopItem
    {
        public string itemName;
        public int price;
        public Sprite icon;
        public GameObject weaponPrefab;
        public WeaponCategory category;
    }

    [Header("Interaction")]
    public string playerTag = "Player";
    public KeyCode interactKey = KeyCode.E;
    public GameObject interactPrompt;

    [Header("Currency (placeholder)")]
    public int debugCurrencyAmount = 123;

    [Header("Inventory")]
    [Tooltip("Items appear in order; only the first few are shown at once.")]
    public ShopItem[] items;

    private PlayerCurrency playerCurrency;

    bool _playerInRange = false;

    void OnValidate()
    {
        var col = GetComponent<Collider2D>();
        if (col) col.isTrigger = true;
    }

    void Awake()
    {
        var col = GetComponent<Collider2D>();
        if (col) col.isTrigger = true;

        if (interactPrompt != null)
            interactPrompt.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsPlayer(other)) return;

        _playerInRange = true;
        if (interactPrompt != null)
            interactPrompt.SetActive(true);

        playerCurrency = other.GetComponentInParent<PlayerCurrency>();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!IsPlayer(other)) return;

        _playerInRange = false;
        if (interactPrompt != null)
            interactPrompt.SetActive(false);

        playerCurrency = null;
    }

    bool IsPlayer(Collider2D other)
    {
        if (other.CompareTag(playerTag)) return true;
        if (other.transform.parent != null && other.transform.parent.CompareTag(playerTag)) return true;
        return false;
    }

    void Update()
    {
        if (!_playerInRange) return;
        if (ShopMenuUI.Instance == null) return;

        if (Input.GetKeyDown(interactKey) && !ShopMenuUI.Instance.IsOpen)
        {
            int amount = (playerCurrency != null) ? playerCurrency.Coins : debugCurrencyAmount;
            ShopMenuUI.Instance.Open(amount, items);
        }
    }
}
