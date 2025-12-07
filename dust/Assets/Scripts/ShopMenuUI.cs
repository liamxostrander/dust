using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class ShopMenuUI : MonoBehaviour
{
    public static ShopMenuUI Instance { get; private set; }

    [Header("Root")]
    public GameObject root;

    [Header("Item Slots (3 tall squares)")]
    public Image[] itemSlots;
    public Color normalColor = Color.white;
    public Color selectedColor = Color.yellow;

    [Header("Item Icons (centered inside frames)")]
    public Image[] itemIconImages;

    [Header("Item Labels")]
    public TMP_Text[] itemNameTexts;
    public TMP_Text[] itemPriceTexts;

    [Header("Currency Display")]
    public TMP_Text currencyText;
    public string currencyLabel = "Coins";
    public int debugCurrencyAmount = 123;

    [Header("Behavior")]
    public bool pauseGameOnOpen = false;

    [Header("Currency Source")]
    public PlayerCurrency playerCurrency;

    [Header("Audio")]
    public AudioClip moveCursorSfx;
    public AudioClip purchaseSfx;
    public AudioClip cannotAffordSfx;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    bool _isOpen = false;
    int _selectedIndex = 1;
    float _previousTimeScale = 1f;

    private Shopkeeper.ShopItem[] _items;

    private List<Shopkeeper.ShopItem> _itemQueue = new List<Shopkeeper.ShopItem>();
    private bool _queueInitialized = false;

    public bool IsOpen => _isOpen;

    bool HasItemAt(int index)
    {
        return _items != null &&
            index >= 0 &&
            index < _items.Length &&
            _items[index] != null;
    }

    int GetFirstNonEmptyIndex()
    {
        if (_items == null) return -1;
        for (int i = 0; i < _items.Length; i++)
        {
            if (_items[i] != null)
                return i;
        }
        return -1;
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (root == null)
            root = gameObject;

        SetOpen(false);
    }

    void Update()
    {
        if (!_isOpen) return;

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            MoveSelection(-1);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            MoveSelection(+1);
        }
        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            PurchaseSelected();
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
        }
    }

    void MoveSelection(int dir)
    {
        if (_items == null || _items.Length == 0) return;

        if (!HasItemAt(_selectedIndex))
        {
            _selectedIndex = GetFirstNonEmptyIndex();
            if (_selectedIndex == -1) return; // no items at all
        }

        int previous = _selectedIndex;
        int newIndex = _selectedIndex;

        for (int attempts = 0; attempts < _items.Length; attempts++)
        {
            newIndex += dir;

            if (newIndex < 0 || newIndex >= _items.Length)
                break;

            if (HasItemAt(newIndex))
            {
                _selectedIndex = newIndex;
                break;
            }
        }

        if (previous != _selectedIndex)
        {
            UpdateHighlight();
            PlaySfx(moveCursorSfx);
        }
    }

    void UpdateHighlight()
    {
        if (itemSlots == null) return;

        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (itemSlots[i] == null) continue;
            itemSlots[i].color = (i == _selectedIndex) ? selectedColor : normalColor;
        }
    }

    void PurchaseSelected()
    {
        if (_items == null || _items.Length == 0)
        {
            Debug.Log("[Shop] No items configured.");
            return;
        }

        if (_selectedIndex < 0 || _selectedIndex >= _items.Length)
        {
            Debug.Log("[Shop] Selected index out of range.");
            return;
        }

        var item = _items[_selectedIndex];
        if (item == null)
        {
            Debug.Log("[Shop] Empty slot selected.");
            PlaySfx(cannotAffordSfx);
            return;
        }

        int price = Mathf.Max(0, item.price);

        int available = (playerCurrency != null)
            ? playerCurrency.Coins
            : debugCurrencyAmount;

        if (available < price)
        {
            Debug.Log($"[Shop] Cannot afford '{item.itemName}' (cost {price}, you have {available}).");
            PlaySfx(cannotAffordSfx);
            return;
        }

        if (playerCurrency != null)
        {
            bool spent = playerCurrency.TrySpendCoins(price);
            if (!spent)
            {
                Debug.LogWarning("[Shop] TrySpendCoins failed.");
                PlaySfx(cannotAffordSfx);
                return;
            }
        }
        else
        {
            debugCurrencyAmount = Mathf.Max(0, debugCurrencyAmount - price);
            if (currencyText != null)
                currencyText.text = $"{currencyLabel}: {debugCurrencyAmount}";
        }

        Debug.Log($"[Shop] Bought '{item.itemName}' for {price} coins.");
        PlaySfx(purchaseSfx);

        var purchased = item;

        var controller = FindFirstObjectByType<PlayerWeaponController>();
        if (controller != null && purchased.weaponPrefab != null)
        {
            controller.AssignPurchasedWeapon(purchased);
        }

        if (_itemQueue != null && purchased != null)
        {
            _itemQueue.Remove(purchased);
        }

        int slotCount = itemSlots != null ? itemSlots.Length : 0;
        if (_items == null || _items.Length != slotCount)
        {
            _items = new Shopkeeper.ShopItem[slotCount];
        }

        for (int i = 0; i < slotCount; i++)
        {
            if (_itemQueue != null && i < _itemQueue.Count)
            {
                _items[i] = _itemQueue[i];
            }
            else
            {
                _items[i] = null;
            }
        }

        RefreshItemLabels();
    }

    public void Open(int currentCurrency = -1, Shopkeeper.ShopItem[] items = null)
    {
        if (_isOpen) return;

        if (!_queueInitialized)
        {
            _itemQueue.Clear();
            if (items != null)
            {
                foreach (var it in items)
                {
                    if (it != null)
                        _itemQueue.Add(it);
                }
            }
            _queueInitialized = true;
        }

        int slotCount = itemSlots != null ? itemSlots.Length : 0;

        if (_items == null || _items.Length != slotCount)
        {
            _items = new Shopkeeper.ShopItem[slotCount];
        }

        for (int i = 0; i < slotCount; i++)
        {
            if (_itemQueue != null && i < _itemQueue.Count)
                _items[i] = _itemQueue[i];
            else
                _items[i] = null;
        }

        _selectedIndex = GetFirstNonEmptyIndex();

        SetOpen(true);
        RefreshItemLabels();
        UpdateHighlight();

        if (currencyText != null)
        {
            int amount = currentCurrency >= 0
                ? currentCurrency
                : (playerCurrency != null ? playerCurrency.Coins : debugCurrencyAmount);

            currencyText.text = $"{currencyLabel}: {amount}";
        }
    }

    public void Close()
    {
        if (!_isOpen) return;
        SetOpen(false);
    }

    void SetOpen(bool open)
    {
        _isOpen = open;

        if (root != null)
            root.SetActive(open);

        if (pauseGameOnOpen)
        {
            if (open)
            {
                _previousTimeScale = Time.timeScale;
                Time.timeScale = 0f;
            }
            else
            {
                Time.timeScale = _previousTimeScale;
            }
        }
    }

    void RefreshItemLabels()
    {
        if (itemSlots == null) return;

        for (int i = 0; i < itemSlots.Length; i++)
        {
            bool hasItem = HasItemAt(i);

            // Frame / slot root
            Image frame = itemSlots[i];
            if (frame != null)
            {
                // Hide the whole slot GameObject if there is no item
                frame.gameObject.SetActive(hasItem);
            }

            // Icon
            Image iconImage = (itemIconImages != null && i < itemIconImages.Length)
                ? itemIconImages[i]
                : null;

            // Name / price text
            TMP_Text nameText = (itemNameTexts != null && i < itemNameTexts.Length)
                ? itemNameTexts[i]
                : null;

            TMP_Text priceText = (itemPriceTexts != null && i < itemPriceTexts.Length)
                ? itemPriceTexts[i]
                : null;

            if (hasItem)
            {
                var item = _items[i];

                if (nameText != null)
                    nameText.text = item.itemName;

                if (priceText != null)
                    priceText.text = "¢" + item.price; // or "$" if you prefer

                if (iconImage != null)
                {
                    iconImage.sprite = item.icon;
                    iconImage.enabled = (item.icon != null);
                }

                // Make sure children are active if slot is visible
                if (nameText != null)  nameText.gameObject.SetActive(true);
                if (priceText != null) priceText.gameObject.SetActive(true);
                if (iconImage != null) iconImage.gameObject.SetActive(true);
            }
            else
            {
                // No item => clear texts, hide icon, hide texts
                if (nameText != null)
                {
                    nameText.text = "";
                    nameText.gameObject.SetActive(false);
                }

                if (priceText != null)
                {
                    priceText.text = "";
                    priceText.gameObject.SetActive(false);
                }

                if (iconImage != null)
                {
                    iconImage.sprite = null;
                    iconImage.enabled = false;
                    iconImage.gameObject.SetActive(false);
                }
            }
        }

        // After refreshing, make sure selection points at a visible item
        if (_items != null && _items.Length > 0)
        {
            if (!HasItemAt(_selectedIndex))
            {
                _selectedIndex = GetFirstNonEmptyIndex();
            }
        }
        else
        {
            _selectedIndex = -1;
        }

        UpdateHighlight();
    }

    void OnEnable()
    {
        if (playerCurrency == null)
            playerCurrency = FindFirstObjectByType<PlayerCurrency>();

        if (playerCurrency != null)
            playerCurrency.OnCoinsChanged += HandleCoinsChanged;
    }

    void OnDisable()
    {
        if (playerCurrency != null)
            playerCurrency.OnCoinsChanged -= HandleCoinsChanged;
    }

    void PlaySfx(AudioClip clip)
    {
        if (clip == null) return;
        if (GlobalAudio.SFX != null)
        {
            GlobalAudio.SFX.PlayOneShot(clip, sfxVolume);
        }
    }

    private void HandleCoinsChanged(int amount)
    {
        if (_isOpen && currencyText != null)
        {
            currencyText.text = $"{currencyLabel}: {amount}";
        }
    }

    IEnumerator FadeOutItemSlot(int index)
    {
        if (itemSlots == null || index < 0 || index >= itemSlots.Length)
            yield break;

        float duration = 0.3f;
        float t = 0f;

        Image frame = itemSlots[index];
        Image icon = (itemIconImages != null && index < itemIconImages.Length)
            ? itemIconImages[index]
            : null;

        TMP_Text nameText = (itemNameTexts != null && index < itemNameTexts.Length)
            ? itemNameTexts[index]
            : null;

        TMP_Text priceText = (itemPriceTexts != null && index < itemPriceTexts.Length)
            ? itemPriceTexts[index]
            : null;

        Color frameStart  = frame     ? frame.color     : Color.white;
        Color iconStart   = icon      ? icon.color      : Color.white;
        Color nameStart   = nameText  ? nameText.color  : Color.white;
        Color priceStart  = priceText ? priceText.color : Color.white;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Lerp(1f, 0f, t / duration);

            if (frame)
            {
                var c = frameStart;
                c.a = a;
                frame.color = c;
            }

            if (icon)
            {
                var c = iconStart;
                c.a = a;
                icon.color = c;
            }

            if (nameText)
            {
                var c = nameStart;
                c.a = a;
                nameText.color = c;
            }

            if (priceText)
            {
                var c = priceStart;
                c.a = a;
                priceText.color = c;
            }

            yield return null;
        }
        if (icon)      icon.enabled = false;
        if (frame)     frame.enabled = false;
        if (nameText)  nameText.text = "";
        if (priceText) priceText.text = "";
    }
}
