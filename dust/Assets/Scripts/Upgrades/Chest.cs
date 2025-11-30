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
    [Tooltip("Destroy the chest after the upgrade has been picked up.")]
    public bool destroyAfterOpen = true;
    public float postOpenDelay = 0.05f;

    [Header("Upgrade Pickup Visual")]
    [Tooltip("Prefab with a SpriteRenderer that will show the upgrade's sprite.")]
    public GameObject upgradeVisualPrefab;

    [Tooltip("Where the upgrade item should appear (e.g. an empty above the lid). If null, uses this transform.")]
    public Transform upgradeVisualAnchor;

    [Tooltip("How high the pickup bobs up/down.")]
    public float bobAmplitude = 0.25f;

    [Tooltip("How fast the pickup bobs up/down.")]
    public float bobFrequency = 2f;

    private ChestSpawner spawner;
    private PlayerUpgrades nearbyPlayer;
    private bool playerInRange = false;

    private bool opened = false;
    private bool upgradeCollected = false;
    private UpgradeSO pendingUpgrade;

    private GameObject spawnedUpgradeVisual;
    private float bobStartTime;

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
        if (upgradeVisualAnchor == null) upgradeVisualAnchor = transform;
    }

    void Update()
    {
        UpdateUpgradeVisualBob();

        if (!playerInRange || nearbyPlayer == null)
            return;

        if (!opened)
        {
            if (autoOpen || Input.GetKeyDown(interactKey))
            {
                OpenChest(nearbyPlayer);
            }
        }
        else if (pendingUpgrade != null && !upgradeCollected)
        {
            if (Input.GetKeyDown(interactKey))
            {
                CollectUpgrade();
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsOnMask(other.gameObject.layer)) return;

        var pu = TryGetPlayerUpgrades(other);
        if (!pu) return;

        nearbyPlayer = pu;
        playerInRange = true;

        // 🔹 Only show the chest's E prompt if it has NOT been opened yet
        if (interactPrompt && !opened)
            interactPrompt.SetActive(true);

        if (autoOpen && !opened)
        {
            OpenChest(pu);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!IsOnMask(other.gameObject.layer)) return;

        var pu = TryGetPlayerUpgrades(other);
        if (pu != nearbyPlayer) return;

        playerInRange = false;
        nearbyPlayer = null;

        if (interactPrompt)
            interactPrompt.SetActive(false);
    }

    bool IsOnMask(int layer) => ((1 << layer) & playerMask) != 0;

    PlayerUpgrades TryGetPlayerUpgrades(Collider2D col)
    {
        return col.GetComponent<PlayerUpgrades>()
            ?? col.GetComponentInParent<PlayerUpgrades>()
            ?? col.GetComponentInChildren<PlayerUpgrades>();
    }

    void OpenChest(PlayerUpgrades pu)
    {
        if (opened) return;
        opened = true;

        // 🔹 As soon as the chest opens, hide its E prompt
        if (interactPrompt)
            interactPrompt.SetActive(false);

        pendingUpgrade = RollUpgrade();

        if (pendingUpgrade != null)
        {
            SpawnUpgradeVisual(pendingUpgrade);
        }
        else
        {
            upgradeCollected = true;
        }

        if (sfx && openSfx)
            sfx.PlayOneShot(openSfx);

        if (animator && !string.IsNullOrEmpty(openTrigger))
        {
            animator.ResetTrigger(openTrigger);
            animator.SetTrigger(openTrigger);
        }

        if (spawner) spawner.NotifyChestConsumed(gameObject);
    }

    void CollectUpgrade()
    {
        if (pendingUpgrade == null || upgradeCollected || nearbyPlayer == null)
            return;

        upgradeCollected = true;

        nearbyPlayer.AddUpgrade(pendingUpgrade);

        if (spawnedUpgradeVisual != null)
        {
            Destroy(spawnedUpgradeVisual);
            spawnedUpgradeVisual = null;
        }

        var col = GetComponent<Collider2D>();
        if (col) col.enabled = false;

        if (interactPrompt)
            interactPrompt.SetActive(false);

        if (destroyAfterOpen)
        {
            StartCoroutine(WaitForOpenAnimationThenDestroy());
        }
    }

    void SpawnUpgradeVisual(UpgradeSO upgrade)
    {
        if (upgradeVisualPrefab == null || upgradeVisualAnchor == null)
            return;

        spawnedUpgradeVisual = Instantiate(
            upgradeVisualPrefab,
            upgradeVisualAnchor.position,
            Quaternion.identity,
            transform
        );

        var sr = spawnedUpgradeVisual.GetComponentInChildren<SpriteRenderer>();
        if (sr != null && upgrade.worldSprite != null)
        {
            sr.sprite = upgrade.worldSprite;
        }

        bobStartTime = Time.time;
    }

    void UpdateUpgradeVisualBob()
    {
        if (spawnedUpgradeVisual == null || upgradeVisualAnchor == null || upgradeCollected)
            return;

        float t = Time.time - bobStartTime;
        float offsetY = Mathf.Sin(t * bobFrequency) * bobAmplitude;

        var basePos = upgradeVisualAnchor.position;
        spawnedUpgradeVisual.transform.position = basePos + Vector3.up * offsetY;
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

        while (animator &&
               animator.GetCurrentAnimatorStateInfo(0).IsName(openStateName) &&
               animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
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
