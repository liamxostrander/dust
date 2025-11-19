using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopMenuUI : MonoBehaviour
{
    public static ShopMenuUI Instance { get; private set; }

    [Header("Root")]
    [Tooltip("The root GameObject for the shop UI (often the panel or the canvas).")]
    public GameObject root;

    [Header("Item Slots (3 tall squares)")]
    public Image[] itemSlots;
    public Color normalColor = Color.white;
    public Color selectedColor = Color.yellow;

    [Header("Currency Display")]
    public TMP_Text currencyText;
    public string currencyLabel = "Coins";
    [Tooltip("Just a placeholder value for now.")]
    public int debugCurrencyAmount = 123;

    [Header("Behavior")]
    [Tooltip("If true, game time is paused while the shop is open.")]
    public bool pauseGameOnOpen = false;

    bool _isOpen = false;
    int _selectedIndex = 0;
    float _previousTimeScale = 1f;

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
            UpdateHighlight();
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
        Debug.Log($"[Shop] Bought item in slot #{_selectedIndex} (placeholder).");
    }

    public void Open(int currentCurrency = -1)
    {
        if (_isOpen) return;

        SetOpen(true);

        _selectedIndex = 0;
        UpdateHighlight();

        if (currencyText != null)
        {
            int amount = (currentCurrency >= 0) ? currentCurrency : debugCurrencyAmount;
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
}
