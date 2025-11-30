using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HelpSign : MonoBehaviour
{
    [Header("Interaction")]
    public string playerTag = "Player";
    public KeyCode interactKey = KeyCode.E;
    public GameObject interactPrompt;

    private bool _playerInRange = false;

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
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!IsPlayer(other)) return;

        _playerInRange = false;
        if (interactPrompt != null)
            interactPrompt.SetActive(false);
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
        if (HelpMenuUI.Instance == null) return;

        if (Input.GetKeyDown(interactKey))
        {
            if (!HelpMenuUI.Instance.IsOpen)
            {
                HelpMenuUI.Instance.Open();
            }
            else
            {
                HelpMenuUI.Instance.Close();
            }
        }
    }
}
