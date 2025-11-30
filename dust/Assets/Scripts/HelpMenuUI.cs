using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HelpMenuUI : MonoBehaviour
{
    public static HelpMenuUI Instance { get; private set; }

    [Header("Root")]
    [Tooltip("Root object of the help menu (can be this GameObject).")]
    public GameObject root;

    [Header("Behavior")]
    public bool pauseGameOnOpen = false;

    [Header("Optional Close Hint")]
    public KeyCode closeKey = KeyCode.Escape;

    private bool _isOpen = false;
    private float _previousTimeScale = 1f;

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

        if (Input.GetKeyDown(closeKey))
        {
            Close();
        }
    }

    public void Open()
    {
        if (_isOpen) return;
        SetOpen(true);
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

    public void CloseFromButton()
    {
        Close();
    }
}
