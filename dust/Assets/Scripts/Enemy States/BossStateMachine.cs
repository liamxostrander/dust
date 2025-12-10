using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BossStateMachine : MonoBehaviour
{
    public Transform player;
    public Rigidbody2D rb;
    public Animator animator;

    [Header("Arena")]
    public Collider2D worldBoundsCollider;
    public Vector2 arenaCenterOffset; // optional offset from bounds center
    public float teleportHeight = 0f; // Y override when teleport to center

    [Header("Teleporting")]
    [Tooltip("Optional preset anchor points for boss teleport destinations")]
    public Transform[] teleportAnchors;
    [Tooltip("Minimum distance from current position before teleport is allowed (prevents redundant teleports)")]
    public float minTeleportDelta = 0.25f;
    public AudioClip teleportSfx;

    [Header("Phases & Timers")]
    public float attackPause = 1.0f;
    public float bulletHellCooldown = 10f;
    public float spikeCooldown = 8f;
    public float laserCooldown = 12f;

    [HideInInspector] public float lastBulletHell;
    [HideInInspector] public float lastSpike;
    [HideInInspector] public float lastLaser;

    [Header("States")]
    public BossIdleState idleState;
    public BossBulletHellState bulletHellState;
    public BossSpikeAttackState spikeState;
    public BossLaserSweepState laserState;
    public BossDeathState deathState;

    [Header("Debug")]
    public bool showGizmos = true;

    private BossState current;
    private int lastAttackIndex = -1; // Track which attack was used last
    private Coroutine activeTeleportRoutine; // Track active teleport to prevent overlaps

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (player == null) player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // Make only this boss ignore collisions with ground objects
        int groundLayer = LayerMask.NameToLayer("Ground");
        if (groundLayer != -1)
        {
            // Find all ground colliders and ignore them specifically for this boss
            Collider2D[] groundColliders = FindObjectsByType<Collider2D>(FindObjectsSortMode.None);
            foreach (var col in groundColliders)
            {
                if (col.gameObject.layer == groundLayer)
                {
                    Collider2D bossCollider = GetComponent<Collider2D>();
                    if (bossCollider != null)
                    {
                        Physics2D.IgnoreCollision(bossCollider, col, true);
                    }
                }
            }
            Debug.Log($"BossStateMachine: Ignoring ground collisions for this boss only");
        }

        // Auto-find world bounds collider if not assigned
        if (worldBoundsCollider == null)
        {
            var boundObj = GameObject.FindGameObjectWithTag("WorldBounds");
            if (boundObj == null) boundObj = GameObject.Find("WorldBounds");
            if (boundObj != null) worldBoundsCollider = boundObj.GetComponent<Collider2D>();
        }

        // Auto-find teleport anchors from BossTeleports children if not assigned
        if (teleportAnchors == null || teleportAnchors.Length == 0)
        {
            GameObject teleportsParent = GameObject.Find("BossTeleports");
            if (teleportsParent != null)
            {
                int childCount = teleportsParent.transform.childCount;
                if (childCount > 0)
                {
                    teleportAnchors = new Transform[childCount];
                    for (int i = 0; i < childCount; i++)
                    {
                        teleportAnchors[i] = teleportsParent.transform.GetChild(i);
                    }
                    Debug.Log($"BossStateMachine: Auto-found {childCount} teleport anchors from BossTeleports");
                }
            }
            else
            {
                Debug.LogWarning("BossStateMachine: BossTeleports GameObject not found. Boss will teleport to center instead.");
            }
        }

        // Initialize cooldown timers to allow attacks immediately
        lastBulletHell = -bulletHellCooldown;
        lastSpike = -spikeCooldown;
        lastLaser = -laserCooldown;

        idleState?.Initialize(this);
        bulletHellState?.Initialize(this);
        spikeState?.Initialize(this);
        laserState?.Initialize(this);
        deathState?.Initialize(this);

        // Connect IsDamageable events
        IsDamageable damageable = GetComponent<IsDamageable>();
        if (damageable != null)
        {
            damageable.OnDeath.AddListener(HandleDeath);
            Debug.Log("BossStateMachine: Connected to IsDamageable.OnDeath event");
        }
        else
        {
            Debug.LogWarning("BossStateMachine: No IsDamageable component found! Boss will not die automatically.");
        }

        // Check if at least one state is assigned
        if (bulletHellState == null && spikeState == null && laserState == null)
        {
            Debug.LogError("BossStateMachine: No attack states assigned! Please assign at least one state in the Inspector.");
            return;
        }

        // Configure idle state duration to match attack pause
        if (idleState != null)
        {
            idleState.idleDuration = attackPause;
        }

        // Start with idle state, then select first attack
        current = idleState;
        if (current != null)
        {
            current.Enter();
        }
        else
        {
            // If no idle state, select attack immediately
            SelectNextAttack();
        }
    }

    void Update()
    {
        current?.Do();

        if (current != null && current.isComplete)
        {
            // Don't transition out of death state
            if (current == deathState)
            {
                return;
            }
            
            Debug.Log($"State {current.GetType().Name} completed, transitioning...");
            current.Exit();
            SelectNextState();
        }
    }

    void FixedUpdate()
    {
        current?.FixedDo();
    }

    public void Switch(BossState next)
    {
        // Death state is uncancellable - prevent any transitions out of it
        if (current == deathState)
        {
            Debug.LogWarning($"BossStateMachine.Switch: Cannot switch from death state to {next?.GetType().Name}");
            return;
        }
        
        Debug.Log($"BossStateMachine.Switch: {current?.GetType().Name} -> {next?.GetType().Name}");
        current?.Exit();
        current = next;
        current?.Enter();
    }

    /// Decide the next state: either return to idle or select a new attack
    void SelectNextState()
    {
        // Never transition from death state
        if (current == deathState)
        {
            Debug.LogWarning("SelectNextState: Cannot transition from death state");
            return;
        }
        
        Debug.Log($"SelectNextState called from {current?.GetType().Name}");
        // If coming from an attack state, go to idle
        if (current == bulletHellState || current == spikeState || current == laserState)
        {
            Debug.Log("Coming from attack state, transitioning to idle");
            if (idleState != null)
            {
                Switch(idleState);
            }
            else
            {
                // No idle state, select next attack immediately
                TeleportToNearestAnchor();
                StartCoroutine(WaitForTeleportThenAttack());
            }
        }
        // If coming from idle, teleport to nearest anchor then select next attack
        else if (current == idleState)
        {
            Debug.Log("Coming from idle, starting teleport");
            // Teleport to position boss for next attack
            TeleportToNearestAnchor();
            // Wait for teleport to complete, then select attack
            StartCoroutine(WaitForTeleportThenAttack());
        }
        else
        {
            Debug.Log($"Unexpected state transition from {current?.GetType().Name}, going to idle");
            // Fallback: go to idle or select attack
            if (idleState != null)
            {
                Switch(idleState);
            }
            else
            {
                TeleportToNearestAnchor();
                StartCoroutine(WaitForTeleportThenAttack());
            }
        }
    }

    System.Collections.IEnumerator WaitForTeleportThenAttack()
    {
        // Wait for teleport to complete (1 second total: 0.5 out + 0.5 in)
        yield return new WaitForSeconds(1.2f);
        SelectNextAttack();
    }

    void SelectNextAttack()
    {
        Debug.Log($"SelectNextAttack called. Last attack index: {lastAttackIndex}");
        
        // Build list of available attacks in order: bullet hell (0), spikes (1), laser (2)
        BossState[] availableAttacks = new BossState[3];
        availableAttacks[0] = bulletHellState;
        availableAttacks[1] = spikeState;
        availableAttacks[2] = laserState;

        // Find next attack in cycle
        int attempts = 0;
        int nextIndex = lastAttackIndex;
        BossState nextAttack = null;

        // Try to find the next available attack in the cycle (max 3 attempts)
        while (nextAttack == null && attempts < 3)
        {
            nextIndex = (nextIndex + 1) % 3;
            if (availableAttacks[nextIndex] != null)
            {
                nextAttack = availableAttacks[nextIndex];
                lastAttackIndex = nextIndex;
            }
            attempts++;
        }

        // If we found an attack, use it
        if (nextAttack != null)
        {
            Switch(nextAttack);
            string attackName = nextIndex == 0 ? "Bullet Hell" : (nextIndex == 1 ? "Spikes" : "Laser");
            Debug.Log($"Choosing {attackName} (cycle index {nextIndex})");
        }
        else
        {
            Debug.LogError("No attacks available!");
        }
    }

    public Vector2 GetArenaCenter()
    {
        if (worldBoundsCollider == null)
            return (Vector2)transform.position; // fallback

        var c = worldBoundsCollider.bounds.center;
        var center = new Vector2(c.x, teleportHeight == 0f ? c.y : teleportHeight);
        return center + arenaCenterOffset;
    }

    public void TeleportToCenter()
    {
        Vector2 center = GetArenaCenter();
        TeleportTo(center);
    }

    // New: generic teleport with SFX and velocity reset
    public void TeleportTo(Vector2 position)
    {
        if (Vector2.Distance(transform.position, position) < minTeleportDelta) return;

        // Stop any active teleport before starting a new one
        if (activeTeleportRoutine != null)
        {
            StopCoroutine(activeTeleportRoutine);
            activeTeleportRoutine = null;
        }

        activeTeleportRoutine = StartCoroutine(TeleportRoutine(position));
    }

    System.Collections.IEnumerator TeleportRoutine(Vector2 destination)
    {
        // Play teleport out animation
        if (animator != null)
            animator.Play("Boss_TeleportOut");

        PlayTeleportSfx();

        // Wait for teleport out animation to complete (adjust timing as needed)
        yield return new WaitForSeconds(0.5f);

        // Move to destination using Rigidbody2D to avoid physics glitches
        rb.linearVelocity = Vector2.zero;
        rb.position = destination;

        // Play teleport in animation
        if (animator != null)
            animator.Play("Boss_TeleportIn");

        PlayTeleportSfx();
        
        // Wait for teleport in to finish, then return to idle
        yield return new WaitForSeconds(0.5f);
        if (animator != null)
            animator.Play("Boss_Idle");
        
        activeTeleportRoutine = null; // Clear the reference when done
    }

    // New: teleport to a random predefined anchor
    public void TeleportToRandomAnchor()
    {
        if (teleportAnchors == null || teleportAnchors.Length == 0)
        {
            TeleportToCenter();
            return;
        }
        int idx = Random.Range(0, teleportAnchors.Length);
        TeleportTo(teleportAnchors[idx].position);
    }

    // Teleport between player and arena center (or at center if player is close)
    public void TeleportToNearestAnchor()
    {
        if (player == null)
        {
            TeleportToCenter();
            return;
        }
        
        Vector2 arenaCenter = GetArenaCenter();
        Vector2 playerPos = player.position;
        
        // Calculate distance from player to center
        float distToCenter = Vector2.Distance(playerPos, arenaCenter);
        
        // If player is very close to center, just teleport to center
        if (distToCenter < 3f)
        {
            TeleportTo(arenaCenter);
            return;
        }
        
        // Teleport somewhere between player and center, but closer to player
        // Random factor between 0.1 (very close to player) and 0.4 (still relatively close to player)
        float lerpFactor = Random.Range(0.2f, 0.5f);
        Vector2 destination = Vector2.Lerp(playerPos, arenaCenter, lerpFactor);
        
        // Clamp to arena bounds just in case
        destination = ClampToArena(destination);
        
        TeleportTo(destination);
    }

    // New: teleport near the player (with clamp inside arena bounds)
    public void TeleportNearPlayer(float xOffset = 3f, float yOffset = 0f)
    {
        if (player == null)
        {
            TeleportToCenter();
            return;
        }
        Vector2 target = player.position + new Vector3(xOffset * Mathf.Sign(Random.Range(-1f, 1f)), yOffset, 0f);
        TeleportTo(ClampToArena(target));
    }

    // New: teleport to screen corners (inside arena bounds)
    public void TeleportToCorner(bool top, bool right, float margin = 0.5f)
    {
        if (worldBoundsCollider == null)
        {
            TeleportToCenter();
            return;
        }
        var b = worldBoundsCollider.bounds;
        float x = right ? (float)b.max.x - margin : (float)b.min.x + margin;
        float y = top ? (float)b.max.y - margin : (float)b.min.y + margin;
        TeleportTo(new Vector2(x, teleportHeight == 0f ? y : teleportHeight));
    }

    // Helpers
    Vector2 ClampToArena(Vector2 pos)
    {
        if (worldBoundsCollider == null) return pos;
        var b = worldBoundsCollider.bounds;
        float x = Mathf.Clamp(pos.x, (float)b.min.x, (float)b.max.x);
        float y = teleportHeight == 0f ? Mathf.Clamp(pos.y, (float)b.min.y, (float)b.max.y) : teleportHeight;
        return new Vector2(x, y);
    }

    void PlayTeleportSfx()
    {
        if (teleportSfx == null) return;
        var audio = GetComponent<AudioSource>();
        if (audio) audio.PlayOneShot(teleportSfx, 0.8f);
    }

    /// <summary>
    /// Triggers the boss death state. Call this when the boss health reaches zero.
    /// </summary>
    public void Die()
    {
        if (deathState != null && current != deathState)
        {
            Debug.Log("Boss.Die() called - switching to death state");
            Switch(deathState);
        }
    }

    private void HandleDeath()
    {
        Debug.Log("BossStateMachine.HandleDeath() - Boss has died");
        
        // Death must interrupt everything immediately
        // Force exit current state to stop any active coroutines or animations
        if (current != null && current != deathState)
        {
            current.Exit();
            current = null;
        }
        
        // Stop all coroutines on this state machine to ensure no leftover routines interfere
        StopAllCoroutines();
        
        // Reset animator to ensure clean transition
        if (animator != null)
        {
            animator.speed = 1f;
        }
        
        // Now switch to death state
        Switch(deathState);

        // Optional: Add any boss-specific death rewards here
        var playerUpgrades = FindFirstObjectByType<PlayerUpgrades>();
        if (playerUpgrades != null && playerUpgrades.CurrentMods.healOnKill > 0f)
        {
            var playerDamageable = playerUpgrades.GetComponent<IsDamageable>();
            if (playerDamageable != null && playerDamageable.IsAlive)
            {
                playerDamageable.Heal(playerUpgrades.CurrentMods.healOnKill);
            }
        }
    }

    void OnDestroy()
    {
        // Clean up event listeners
        IsDamageable damageable = GetComponent<IsDamageable>();
        if (damageable != null)
        {
            damageable.OnDeath.RemoveListener(HandleDeath);
        }
    }

    // Editor gizmos
    void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        // Try to find bounds collider if not set
        Collider2D boundsCollider = worldBoundsCollider;
        if (boundsCollider == null)
        {
            var boundObj = GameObject.FindGameObjectWithTag("WorldBounds");
            if (boundObj == null) boundObj = GameObject.Find("WorldBounds");
            if (boundObj != null) boundsCollider = boundObj.GetComponent<Collider2D>();
        }

        // Arena bounds
        if (boundsCollider != null)
        {
            var b = boundsCollider.bounds;
            Gizmos.color = new Color(0f, 0.6f, 1f, 0.25f);
            Gizmos.DrawCube(b.center, b.size);

            // Corners (with margin example)
            Gizmos.color = Color.cyan;
            Vector3 min = b.min;
            Vector3 max = b.max;
            float yUseMin = teleportHeight == 0f ? (float)min.y : teleportHeight;
            float yUseMax = teleportHeight == 0f ? (float)max.y : teleportHeight;

            Vector3 topLeft = new Vector3((float)min.x, yUseMax, 0f);
            Vector3 topRight = new Vector3((float)max.x, yUseMax, 0f);
            Vector3 bottomLeft = new Vector3((float)min.x, yUseMin, 0f);
            Vector3 bottomRight = new Vector3((float)max.x, yUseMin, 0f);

            float r = 0.25f;
            Gizmos.DrawSphere(topLeft, r);
            Gizmos.DrawSphere(topRight, r);
            Gizmos.DrawSphere(bottomLeft, r);
            Gizmos.DrawSphere(bottomRight, r);

            // Arena center
            Gizmos.color = Color.yellow;
            var center = GetArenaCenter();
            Gizmos.DrawSphere(center, 0.3f);

            // Arena center offset arrow
            Gizmos.color = Color.white;
            Gizmos.DrawLine(b.center, new Vector3(center.x, center.y, 0f));
        }

        // Teleport anchors
        if (teleportAnchors != null)
        {
            Gizmos.color = Color.magenta;
            foreach (var t in teleportAnchors)
            {
                if (t == null) continue;
                Gizmos.DrawSphere(t.position, 0.2f);
                Gizmos.DrawLine(transform.position, t.position);
            }
        }

        // Preview: TeleportNearPlayer offset positions
        if (player != null)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.9f);
            float previewXOffset = 3f;
            float previewYOffset = 0f;
            Vector2 left = ClampToArena(player.position + new Vector3(-previewXOffset, previewYOffset));
            Vector2 right = ClampToArena(player.position + new Vector3(previewXOffset, previewYOffset));
            Gizmos.DrawSphere(left, 0.2f);
            Gizmos.DrawSphere(right, 0.2f);
            Gizmos.DrawLine(left, (Vector2)player.position);
            Gizmos.DrawLine(right, (Vector2)player.position);

            // Attack range indicators
            float closeRange = 8f;
            float farRange = 15f;

            // Close range (Spikes) - Red
            Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
            Gizmos.DrawWireSphere(transform.position, closeRange);
            
            // Far range (Laser) - Blue
            Gizmos.color = new Color(0f, 0.5f, 1f, 0.2f);
            Gizmos.DrawWireSphere(transform.position, farRange);
            
            // Medium range (Bullet Hell) - Yellow ring
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            for (float angle = 0; angle < 360; angle += 10)
            {
                float rad1 = angle * Mathf.Deg2Rad;
                float rad2 = (angle + 10) * Mathf.Deg2Rad;
                Vector3 p1 = transform.position + new Vector3(Mathf.Cos(rad1) * closeRange, Mathf.Sin(rad1) * closeRange, 0);
                Vector3 p2 = transform.position + new Vector3(Mathf.Cos(rad2) * closeRange, Mathf.Sin(rad2) * closeRange, 0);
                Vector3 p3 = transform.position + new Vector3(Mathf.Cos(rad1) * farRange, Mathf.Sin(rad1) * farRange, 0);
                Vector3 p4 = transform.position + new Vector3(Mathf.Cos(rad2) * farRange, Mathf.Sin(rad2) * farRange, 0);
                Gizmos.DrawLine(p1, p2);
            }

            // Player distance indicator
            float distToPlayer = Vector2.Distance(transform.position, player.position);
            Color rangeColor = distToPlayer <= closeRange ? Color.red : 
                              distToPlayer >= farRange ? Color.blue : Color.yellow;
            Gizmos.color = rangeColor;
            Gizmos.DrawLine(transform.position, player.position);
        }

        // Current position marker
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.25f);
    }
}