using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

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
    private bool[] _soldOut;

    public bool IsOpen => _isOpen;

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
        if (itemSlots == null || itemSlots.Length == 0) return;

        int previous = _selectedIndex;
        _selectedIndex = Mathf.Clamp(_selectedIndex + dir, 0, itemSlots.Length - 1);

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

        if (_soldOut != null && _selectedIndex < _soldOut.Length && _soldOut[_selectedIndex])
        {
            Debug.Log("[Shop] Item already purchased.");
            PlaySfx(cannotAffordSfx);
            return;
        }

        var item = _items[_selectedIndex];
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

        Debug.Log($"[Shop] Bought '{item.itemName}' for {price} coins (debug only).");

        if (_soldOut != null && _selectedIndex < _soldOut.Length)
            _soldOut[_selectedIndex] = true;

        PlaySfx(purchaseSfx);
        StartCoroutine(FadeOutItemSlot(_selectedIndex));
    }

    public void Open(int currentCurrency = -1, Shopkeeper.ShopItem[] items = null)
    {
        if (_isOpen) return;

        _items = items;
        _soldOut = (_items != null) ? new bool[_items.Length] : null;
        SetOpen(true);

        _selectedIndex = 0;
        UpdateHighlight();
        RefreshItemLabels();

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
            string name = "";
            string price = "";
            Sprite icon = null;

            if (_items != null && i < _items.Length && _items[i] != null)
            {
                name  = _items[i].itemName;
                price = _items[i].price.ToString();
                icon  = _items[i].icon;
            }

            if (itemNameTexts != null && i < itemNameTexts.Length && itemNameTexts[i] != null)
                itemNameTexts[i].text = name;

            if (itemPriceTexts != null && i < itemPriceTexts.Length && itemPriceTexts[i] != null)
                itemPriceTexts[i].text = "¢" + price;

            if (itemIconImages != null && i < itemIconImages.Length && itemIconImages[i] != null)
            {
                itemIconImages[i].sprite  = icon;
                itemIconImages[i].enabled = (icon != null);
            }
        }
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
