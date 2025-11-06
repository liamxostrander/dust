using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class Chest : MonoBehaviour
{
    [Header("Loot")]
    public UpgradeSO[] possibleUpgrades;
    public AudioSource sfx;
    public AudioClip openSfx;

    [Header("Interaction")]
    public KeyCode interactKey = KeyCode.E;
    public LayerMask playerMask;
    public bool autoOpen = false;
    public GameObject interactPrompt;

    [Header("Animation")]
    public Animator animator;
    public string openTrigger = "Open";
    public string openStateName = "Open";
    public bool destroyAfterOpen = true;
    public float postOpenDelay = 0.05f;

    private ChestSpawner spawner;
    private PlayerUpgrades nearbyPlayer;
    private bool playerInRange = false;
    private bool opened = false;

    void OnValidate()
    {
        var col = GetComponent<Collider2D>();
        if (col) col.isTrigger = true;
    }

    void Awake()
    {
        var col = GetComponent<Collider2D>();
        if (col) col.isTrigger = true;

        if (!animator) animator = GetComponent<Animator>();
        spawner = FindFirstObjectByType<ChestSpawner>();
        if (interactPrompt) interactPrompt.SetActive(false);
    }

    void Update()
    {
        if (opened || !playerInRange || !nearbyPlayer) return;

        if (autoOpen || Input.GetKeyDown(interactKey))
        {
            OpenFor(nearbyPlayer);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsOnMask(other.gameObject.layer)) return;
        var pu = TryGetPlayerUpgrades(other);
        if (!pu) return;

        nearbyPlayer = pu;
        playerInRange = true;
        if (interactPrompt) interactPrompt.SetActive(true);

        if (autoOpen) OpenFor(pu);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!IsOnMask(other.gameObject.layer)) return;
        var pu = TryGetPlayerUpgrades(other);
        if (pu != nearbyPlayer) return;

        playerInRange = false;
        nearbyPlayer = null;
        if (interactPrompt) interactPrompt.SetActive(false);
    }

    bool IsOnMask(int layer) => ((1 << layer) & playerMask) != 0;

    PlayerUpgrades TryGetPlayerUpgrades(Collider2D col)
    {
        return col.GetComponent<PlayerUpgrades>()
            ?? col.GetComponentInParent<PlayerUpgrades>()
            ?? col.GetComponentInChildren<PlayerUpgrades>();
    }

    void OpenFor(PlayerUpgrades pu)
    {
        if (opened) return;
        opened = true;

        // stop further triggers
        var col = GetComponent<Collider2D>();
        if (col) col.enabled = false;
        if (interactPrompt) interactPrompt.SetActive(false);

        var u = RollUpgrade();
        if (u) pu.AddUpgrade(u);

        if (sfx && openSfx) sfx.PlayOneShot(openSfx);

        if (animator && !string.IsNullOrEmpty(openTrigger))
        {
            animator.ResetTrigger(openTrigger);
            animator.SetTrigger(openTrigger);
        }

        if (spawner) spawner.NotifyChestConsumed(gameObject);

        if (destroyAfterOpen)
            StartCoroutine(WaitForOpenAnimationThenDestroy());
    }

    IEnumerator WaitForOpenAnimationThenDestroy()
    {
        yield return null;

        float safety = 5f;
        while (safety > 0f)
        {
            if (!animator) break;

            var info = animator.GetCurrentAnimatorStateInfo(0);
            if (info.IsName(openStateName)) break;

            safety -= Time.unscaledDeltaTime;
            yield return null;
        }

        while (animator && animator.GetCurrentAnimatorStateInfo(0).IsName(openStateName)
               && animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            yield return null;
        }

        if (postOpenDelay > 0f)
            yield return new WaitForSeconds(postOpenDelay);

        Destroy(gameObject);
    }

    UpgradeSO RollUpgrade()
    {
        if (possibleUpgrades == null || possibleUpgrades.Length == 0) return null;
        return possibleUpgrades[Random.Range(0, possibleUpgrades.Length)];
    }
}
