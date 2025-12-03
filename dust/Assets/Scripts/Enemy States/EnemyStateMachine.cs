using UnityEngine;
using UnityEngine.XR;
using System.Collections;

/// Main enemy state machine that controls enemy AI behavior.
/// Manages state transitions and provides helper functions for states.
[RequireComponent(typeof(AudioSource))]
public class EnemyStateMachine : MonoBehaviour
{
    private EnemyState currentState;
    
    [Header("Component References")]
    public Rigidbody2D rb;
    public Transform player;
    public LayerMask groundLayer;
    public LayerMask playerLayer;
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    public AudioSource audioSource;
    
    [Header("Audio")]
    public AudioClip meleeAttackSound;
    public AudioClip rangedAttackSound;
    public AudioClip spellCastSound;
    public AudioClip hurtSound;
    public AudioClip deathSound;
    public AudioClip idleGruntSound;
    [Range(0f, 1f)]
    public float sfxVolume = 0.5f;
    [Tooltip("Enable subtle random pitch variation for SFX")]
    public bool enablePitchVariation = true;
    [Range(0f, 0.5f)]
    [Tooltip("Max pitch deviation from 1.0 (e.g., 0.05 = ±5%)")]
    public float pitchVariance = 0.05f;
    [Tooltip("Minimum time between idle grunts in seconds")]
    public float minIdleGruntInterval = 3f;
    [Tooltip("Maximum time between idle grunts in seconds")]
    public float maxIdleGruntInterval = 8f;
    [HideInInspector] public float nextIdleGruntTime;
    
    [Header("Movement Settings")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    public float detectionRange = 5f;
    public float visionConeAngle = 90f;
    public bool useVisionCone = true;
    [Tooltip("If true, enemy will flee from player instead of chasing")]
    public bool isSkittish = false;
    
    [Header("Flying Enemy Settings")]
    public bool isFlying = false;
    public float flyingHeight = 3f;
    [Tooltip("Minimum height above ground to maintain")]
    public float minFlyingHeight = 2f;
    public float verticalPatrolAmplitude = 1f;
    public float verticalPatrolSpeed = 1f;
    public float flyingChaseVerticalSpeed = 3f;
    public float flyingWallCheckDistance = 1f;
    [Tooltip("How far down to check for ground")]
    public float groundCheckDistance = 3f;
    [Tooltip("Collider2D component from WorldBounds object that defines arena boundaries")]
    public Collider2D worldBoundsCollider;
    [Tooltip("Speed at which enemy returns to arena center when out of bounds")]
    public float returnToArenaSpeed = 5f;
    
    [Header("Flying Attack (Swoop) Settings")]
    [Tooltip("If true, flying enemies will perform swoop attacks (melee only)")]
    public bool useSwoopAttack = true;
    public float swoopSpeed = 8f;
    public float swoopReturnSpeed = 4f;
    public float minHeightAbovePlayerForSwoop = 1f;
    [HideInInspector] public Vector2 preSwoopPosition;
    [HideInInspector] public float spawnYPosition;
    // Add drift settings for skittish/flee-capable flying enemies
    [Header("Skittish Drift Settings")]
    [Tooltip("Horizontal drift speed toward player when outside flee safe distance")]
    public float skittishDriftSpeed = 1f;
    [Tooltip("Vertical drift speed toward player when outside flee safe distance")]
    public float skittishDriftVerticalSpeed = 0.5f;
    
    [Header("Spell Casting Settings")]
    public bool canCastSpells = false;
    public float spellCastRange = 100f; // Can cast from anywhere
    public float spellCooldown = 10f;
    public int spellDamage = 2;
    [HideInInspector] public float lastSpellCastTime = -999f;

    [Header("Edge Detection")]
    public Transform groundCheckPoint;
    public float edgeCheckDistance = 0.5f;
    [HideInInspector] public bool shouldReverseDirection = false;
    public float lastMoveDirection = 1f;

    [Header("Ground Check")]
    public BoxCollider2D groundCheck;
    public float groundCheckRadius = 0.2f;
    [HideInInspector] public bool isGrounded;
    
    [Header("Attack Settings")]
    public float attackRange = 1.5f;
    public float attackConeAngle = 90f;
    public bool useAttackCone = true;
    public float attackHitboxRadius = 0.75f;
    public float attackCooldown = 1.0f;
    public int attackDamage = 1;
    private float lastAttackTime = -999f;
    [HideInInspector] public bool justFinishedAttack = false;
    [Tooltip("If true, enemy will not be interrupted during attacks when taking damage")]
    public bool uncancellableAttacks = false;
    
    [Header("Ranged Attack Settings")]
    [Tooltip("Enable ranged attacks instead of melee")]
    public bool useRangedAttack = false;
    [Tooltip("Range for ranged attacks (typically longer than melee)")]
    public float rangedAttackRange = 8f;

    [Header("States")]
    public EnemyIdleState idleState;
    public EnemyPatrolState patrolState;
    public EnemyChaseState chaseState;
    public EnemyFleeState fleeState;
    public EnemyMeleeState attackState;
    public EnemyRangedState rangedAttackState;
    public EnemySpellCastState spellCastState;
    public EnemyDeathState deathState;
    public EnemyHurtState hurtState;
    
    [Header("Debug")]
    public bool showGizmos = true;

    private Vector2 pendingKnockback;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        // Store spawn position for flying enemies to maintain relative height
        if (isFlying)
        {
            spawnYPosition = transform.position.y;
        }
        
        // Auto-find WorldBounds for flying enemies if not assigned
        if (isFlying && worldBoundsCollider == null)
        {
            GameObject worldBoundsObj = GameObject.Find("VisualFX/WorldBounds");
            if (worldBoundsObj == null)
            {
                // Try without the path in case it's at root level
                worldBoundsObj = GameObject.Find("WorldBounds");
            }
            
            if (worldBoundsObj != null)
            {
                worldBoundsCollider = worldBoundsObj.GetComponent<Collider2D>();
            }
            else
            {
                Debug.LogWarning($"Flying enemy {gameObject.name} could not find WorldBounds object in scene!");
            }
        }
        
        if (animator == null) animator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        if (idleState == null) idleState = GetComponent<EnemyIdleState>();
        if (patrolState == null) patrolState = GetComponent<EnemyPatrolState>();
        if (chaseState == null) chaseState = GetComponent<EnemyChaseState>();
        if (fleeState == null) fleeState = GetComponent<EnemyFleeState>();
        if (attackState == null) attackState = GetComponent<EnemyMeleeState>();
        if (rangedAttackState == null) rangedAttackState = GetComponent<EnemyRangedState>();
        if (spellCastState == null) spellCastState = GetComponent<EnemySpellCastState>();
        if (deathState == null) deathState = GetComponent<EnemyDeathState>();
        if (hurtState == null) hurtState = GetComponent<EnemyHurtState>();

        if (idleState != null) idleState.Initialize(this);
        if (patrolState != null) patrolState.Initialize(this);
        if (chaseState != null) chaseState.Initialize(this);
        if (fleeState != null) fleeState.Initialize(this);
        if (attackState != null) attackState.Initialize(this);
        if (rangedAttackState != null) rangedAttackState.Initialize(this);
        if (spellCastState != null) spellCastState.Initialize(this);
        if (deathState != null) deathState.Initialize(this);
        if (hurtState != null) hurtState.Initialize(this);

        // Connect IsDamageable events
        IsDamageable damageable = GetComponent<IsDamageable>();
        if (damageable != null)
        {
            damageable.OnDamagedWithKnockback.AddListener(HandleDamaged);
            damageable.OnDeath.AddListener(HandleDeath);
        }

        // Initialize idle grunt timer
        nextIdleGruntTime = Time.time + Random.Range(minIdleGruntInterval, maxIdleGruntInterval);
        
        // Set initial state
        currentState = patrolState;
        if (currentState != null)
        {
            currentState.Enter();
        }
        else
        {
            Debug.LogError("PatrolState is null! Add EnemyPatrolState component.");
        }
    }

    void Update()
    {
        CheckGrounded();
        UpdateSpriteDirection();
        CheckIdleGrunt();
        
        if (currentState != null)
        {
            currentState.Do();
            
            // Check if we should interrupt current state for spell casting
            // (Don't interrupt spell cast, hurt, death, or attack states)
            if (currentState != spellCastState && currentState != hurtState && 
                currentState != deathState && currentState != attackState && currentState != rangedAttackState)
            {
                if (canCastSpells && IsPlayerInSpellRange() && CanCastSpell() && spellCastState != null)
                {
                    currentState.Exit();
                    SwitchState(spellCastState);
                    return;
                }
            }
            
            if (currentState.isComplete && currentState != deathState)
            {
                currentState.Exit();
                SelectState();
            }
        }
    }

    void FixedUpdate()
    {
        if (currentState != null)
        {
            currentState.FixedDo();
        }
    }

    void OnDestroy()
    {
        IsDamageable damageable = GetComponent<IsDamageable>();
        if (damageable != null)
        {
            damageable.OnDamagedWithKnockback.RemoveListener(HandleDamaged);
            damageable.OnDeath.RemoveListener(HandleDeath);
        }
    }

    private void HandleKnockback(Vector2 knockback)
    {
        pendingKnockback = knockback;
        Debug.Log($"Knockback received: {knockback}");
    }

    private void HandleDamaged(float damage, Vector2 knockback)
    {
        if (currentState == deathState)
            return;

        // If uncancellable attacks is enabled and enemy is attacking, skip hurt state
        // Enemy still takes damage (handled by IsDamageable), but won't be interrupted
        if (uncancellableAttacks && (currentState == attackState || currentState == rangedAttackState || currentState == spellCastState))
        {
            Debug.Log("Enemy hit during attack but has uncancellable attacks - no interrupt");
            return;
        }

        if (hurtState == null)
        {
            Debug.LogWarning("EnemyHurtState not assigned!");
            return;
        }

        // Set the knockback immediately
        hurtState.SetKnockbackDirection(knockback);
        
        SwitchState(hurtState);
    }
    private void HandleDeath()
    {
        SwitchState(deathState);

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

    private void UpdateSpriteDirection()
    {
        // Face the player during chase state
        if (player != null && currentState == chaseState)
        {
            float directionToPlayer = player.position.x - transform.position.x;
            if (Mathf.Abs(directionToPlayer) > 0.1f)
            {
                if (directionToPlayer > 0)
                {
                    transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
                }
                else
                {
                    transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
                }
            }
        }
        else
        {
            // Default behavior: face based on velocity
            if (rb.linearVelocity.x > 0.1f)
            {
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
            else if (rb.linearVelocity.x < -0.1f)
            {
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
        }
    }


    /// Determine the next state based on current conditions.
    /// Priority: spell cast -> attack recovery -> attack range -> chase/flee -> edge detection -> patrol
    void SelectState()
    {
        EnemyState newState = currentState;
        
        bool playerInAttackRange = IsPlayerInAttackRange();
        bool playerDetected = IsPlayerDetected();
        bool playerInSpellRange = IsPlayerInSpellRange();
        bool canCastSpellNow = CanCastSpell();
        
        // Spell casting has highest priority (can cast from anywhere if in spell range)
        if (canCastSpellNow && canCastSpells && playerInSpellRange && spellCastState != null)
        {
            newState = spellCastState;
        }
        else if (justFinishedAttack)
        {
            newState = idleState;
        }
        // Skittish enemies have different behavior
        else if (isSkittish && playerDetected)
        {
            // Check if player is too close (invading safe space)
            float distanceToPlayer = player != null ? Vector2.Distance(transform.position, player.position) : float.MaxValue;
            float safeShootingDistance = fleeState != null ? fleeState.safeDistance : 8f;
            float fleeThreshold = safeShootingDistance * 0.7f;
            
            // Player is too close - flee!
            if (distanceToPlayer < fleeThreshold)
            {
                newState = fleeState != null ? fleeState : patrolState;
            }
            // In the safe zone - can attack if ready
            else if (distanceToPlayer >= fleeThreshold && distanceToPlayer <= safeShootingDistance)
            {
                // Check if we can attack (cooldown ready and within effective range)
                float effectiveRange = useRangedAttack ? rangedAttackRange : attackRange;
                bool canAttackNow = Time.time >= lastAttackTime + attackCooldown && distanceToPlayer <= effectiveRange;
                
                if (canAttackNow)
                {
                    if (useRangedAttack && rangedAttackState != null)
                    {
                        newState = rangedAttackState;
                    }
                    else
                    {
                        newState = attackState;
                    }
                }
                else
                {
                    // Wait in idle until ready to attack
                    newState = idleState;
                }
            }
            // Beyond safe distance - maintain position (idle)
            else
            {
                newState = idleState;
            }
        }
        // Normal enemy behavior (not skittish)
        else if (playerInAttackRange && Time.time >= lastAttackTime + attackCooldown)
        {
            // Choose between ranged and melee attack based on settings
            if (useRangedAttack)
            {
                if (rangedAttackState != null)
                {
                    newState = rangedAttackState;
                }
                else
                {
                    Debug.LogWarning($"{gameObject.name} has useRangedAttack enabled but no EnemyRangedState component!");
                    newState = idleState; 
                }
            }
            else if (attackState != null)
            {
                newState = attackState;
            }
        }
        else if (currentState == idleState && playerInAttackRange)
        {
            newState = idleState;
        }
        else if (playerDetected && !playerInAttackRange)
        {
            newState = chaseState;
        }
        else if (shouldReverseDirection)
        {
            newState = idleState;
        }
        else
        {
            newState = patrolState;
        }
        
        if (newState != currentState)
        {
            currentState = newState;
            currentState.Enter();
        }
    }
    
    public bool IsPlayerDetected()
    {
        if (player == null) return false;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer > detectionRange)
            return false;
        
        if (!useVisionCone)
            return true;
        
        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        Vector2 facingDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        float angleToPlayer = Vector2.Angle(facingDirection, directionToPlayer);
        
        return angleToPlayer <= visionConeAngle / 2f;
    }

    public bool IsPlayerInAttackRange()
    {
        if (player == null) return false;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // Use different range for ranged attacks
        float effectiveRange = useRangedAttack ? rangedAttackRange : attackRange;
        if (distanceToPlayer > effectiveRange)
            return false;
        
        if (!useAttackCone)
            return true;
        
        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        Vector2 facingDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        float angleToPlayer = Vector2.Angle(facingDirection, directionToPlayer);
        
        return angleToPlayer <= attackConeAngle / 2f;
    }

    /// Manually switch to a new state (used for special cases like death).
    public void SwitchState(EnemyState newState)
    {
        if (currentState != null)
        {
            currentState.Exit();
        }
        
        currentState = newState;
        currentState.Enter();
    }
    
    public void ChasePlayer()
    {
        float directionToPlayer = Mathf.Sign(player.position.x - transform.position.x);
        
        if (isFlying)
        {
            // Check if out of bounds - if so, reverse toward center
            if (IsOutOfBounds())
            {
                ReturnToArenaCenter();
                return;
            }
            
            // All flying enemies chase directly toward the player
            Vector2 directionVector = (player.position - transform.position).normalized;
            Vector2 velocity = new Vector2(
                directionVector.x * chaseSpeed,
                directionVector.y * flyingChaseVerticalSpeed
            );
            
            rb.linearVelocity = velocity;
        }
        else
        {
            // Ground enemy edge detection
            Vector2 rayStart;
            if (groundCheck != null)
            {
                rayStart = new Vector2(
                    groundCheck.bounds.center.x + directionToPlayer * edgeCheckDistance,
                    groundCheck.bounds.center.y
                );
            }
            else if (groundCheckPoint != null)
            {
                rayStart = new Vector2(
                    transform.position.x + directionToPlayer * edgeCheckDistance,
                    groundCheckPoint.position.y
                );
            }
            else
            {
                rayStart = new Vector2(
                    transform.position.x + directionToPlayer * edgeCheckDistance,
                    transform.position.y - 0.5f
                );
            }
            
            RaycastHit2D edgeHit = Physics2D.Raycast(rayStart, Vector2.down, edgeCheckDistance * 2, groundLayer);
            
            // If there's an edge ahead, stop moving
            if (!edgeHit.collider)
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                return;
            }
            
            // Ground enemies only chase horizontally
            rb.linearVelocity = new Vector2(directionToPlayer * chaseSpeed, rb.linearVelocity.y);
        }
    }
    
    public void FleeFromPlayer()
    {
        if (player == null) return;
        
        // Move in opposite direction from player
        float directionAwayFromPlayer = -Mathf.Sign(player.position.x - transform.position.x);
        
        if (isFlying)
        {
            // Check if out of bounds - if so, return to arena center
            if (IsOutOfBounds())
            {
                ReturnToArenaCenter();
                return;
            }
            
            // Calculate flee direction
            Vector2 fleeDirection = (transform.position - player.position).normalized;
            
            // Normal flee: move away from player
            rb.linearVelocity = new Vector2(
                fleeDirection.x * chaseSpeed,
                fleeDirection.y * flyingChaseVerticalSpeed
            );
        }
        else
        {
            // Ground enemy edge detection
            Vector2 rayStart;
            if (groundCheck != null)
            {
                rayStart = new Vector2(
                    groundCheck.bounds.center.x + directionAwayFromPlayer * edgeCheckDistance,
                    groundCheck.bounds.center.y
                );
            }
            else if (groundCheckPoint != null)
            {
                rayStart = new Vector2(
                    transform.position.x + directionAwayFromPlayer * edgeCheckDistance,
                    groundCheckPoint.position.y
                );
            }
            else
            {
                rayStart = new Vector2(
                    transform.position.x + directionAwayFromPlayer * edgeCheckDistance,
                    transform.position.y - 0.5f
                );
            }
            
            RaycastHit2D edgeHit = Physics2D.Raycast(rayStart, Vector2.down, edgeCheckDistance * 2, groundLayer);
            
            // If there's an edge ahead, stop moving (trapped)
            if (!edgeHit.collider)
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                return;
            }
            
            // Ground enemies flee horizontally away from player
            rb.linearVelocity = new Vector2(directionAwayFromPlayer * chaseSpeed, rb.linearVelocity.y);
        }
    }
    
    /// Perform melee attack
    public void PerformAttack()
    {
        // lastAttackTime = Time.time;
        
        // Vector2 attackDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        // Vector2 attackPosition = (Vector2)transform.position + attackDirection * (attackHitboxRadius);
        
        // Collider2D[] hits = Physics2D.OverlapCircleAll(attackPosition, attackHitboxRadius, playerLayer);
        
        // if (hits.Length > 0)
        // {
        //     foreach (Collider2D hit in hits)
        //     {
        //         // Debug.Log($"Enemy attack hit player: {hit.gameObject.name}!");
        //     }
        // }
    }
    
    public bool CanAttack()
    {
        return Time.time >= lastAttackTime + attackCooldown;
    }
    
    public bool CanCastSpell()
    {
        return Time.time >= lastSpellCastTime + spellCooldown;
    }
    
    public bool IsPlayerInSpellRange()
    {
        if (player == null) return false;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        return distanceToPlayer <= spellCastRange;
    }
    
    /// Patrol behavior
    public void Patrol()
    {
        float currentDirection = rb.linearVelocity.x;
        
        if (Mathf.Abs(currentDirection) < 0.1f)
        {
            currentDirection = lastMoveDirection;
        }
        
        float moveDirection = Mathf.Sign(currentDirection);
        lastMoveDirection = moveDirection;
        
        if (isFlying)
        {
            // When skittish or capable of fleeing, override normal patrol to drift slowly toward player
            // but only when the player is outside the flee safe distance.
            bool canFlee = isSkittish || fleeState != null;
            if (canFlee && player != null)
            {
                float safeDistance = fleeState != null ? fleeState.safeDistance : 8f;
                float distanceToPlayer = Vector2.Distance(transform.position, player.position);

                if (distanceToPlayer > safeDistance)
                {
                    // Drift gently toward the player
                    if (IsOutOfBounds())
                    {
                        ReturnToArenaCenter();
                        return;
                    }

                    Vector2 dir = (player.position - transform.position).normalized;
                    rb.linearVelocity = new Vector2(
                        dir.x * skittishDriftSpeed,
                        dir.y * skittishDriftVerticalSpeed
                    );
                    return; // Override normal patrol
                }
            }

            // Check if out of bounds - if so, reverse direction
            if (IsOutOfBounds())
            {
                // Reverse direction when hitting bounds
                lastMoveDirection = -lastMoveDirection;
                moveDirection = lastMoveDirection;
            }
            
            // Normal flying patrol with sine wave vertical movement
            // Use current Y position as base instead of spawn position for natural continuation
            float verticalOffset = Mathf.Sin(Time.time * verticalPatrolSpeed) * verticalPatrolAmplitude;
            float baseY = transform.position.y;
            float targetY = baseY + verticalOffset;
            float verticalVelocity = (targetY - transform.position.y) * 2f; // Simple proportional controller
            
            rb.linearVelocity = new Vector2(moveDirection * patrolSpeed, verticalVelocity);
            return;
        }
        
        // Ground enemy edge detection
        Vector2 rayStart;
        if (groundCheck != null)
        {
            rayStart = new Vector2(
                groundCheck.bounds.center.x + moveDirection * edgeCheckDistance,
                groundCheck.bounds.center.y
            );
        }
        else if (groundCheckPoint != null)
        {
            rayStart = new Vector2(
                transform.position.x + moveDirection * edgeCheckDistance,
                groundCheckPoint.position.y
            );
        }
        else
        {
            rayStart = new Vector2(
                transform.position.x + moveDirection * edgeCheckDistance,
                transform.position.y - 0.5f
            );
        }
        
        RaycastHit2D edgeHit = Physics2D.Raycast(rayStart, Vector2.down, edgeCheckDistance * 2, groundLayer);
        
        if (!edgeHit.collider)
        {
            shouldReverseDirection = true;
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }
        
        rb.linearVelocity = new Vector2(moveDirection * patrolSpeed, rb.linearVelocity.y);
    }

    /// Check if enemy is grounded using box collider.
    void CheckGrounded()
    {
        if (isFlying)
        {
            isGrounded = false;
            return;
        }
        
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapAreaAll(groundCheck.bounds.min, groundCheck.bounds.max, groundLayer).Length > 0;
        }
        else
        {
            Vector2 groundCheckPosition = new Vector2(transform.position.x, transform.position.y - 0.5f);
            isGrounded = Physics2D.OverlapCircle(groundCheckPosition, groundCheckRadius, groundLayer);
        }
    }
    
    /// Play a sound effect using this enemy's AudioSource, keeping pitch during playback
    public void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            StartCoroutine(PlaySoundWithPitch(clip));
        }
    }

    private IEnumerator PlaySoundWithPitch(AudioClip clip)
    {
        float originalPitch = audioSource.pitch;
        if (enablePitchVariation && pitchVariance > 0f)
        {
            float delta = Random.Range(-pitchVariance, pitchVariance);
            audioSource.pitch = Mathf.Clamp(1f + delta, 0.1f, 3f);
        }

        audioSource.PlayOneShot(clip, sfxVolume);

        // Wait for the clip duration (scaled by time scale)
        yield return new WaitForSeconds(clip.length);

        audioSource.pitch = originalPitch;
    }
    
    /// Check and play idle grunt at random intervals
    private void CheckIdleGrunt()
    {
        // Only play idle grunts when not in death or hurt state
        if (currentState == deathState || currentState == hurtState)
            return;
            
        if (idleGruntSound != null && Time.time >= nextIdleGruntTime)
        {
            PlaySound(idleGruntSound);
            nextIdleGruntTime = Time.time + Random.Range(minIdleGruntInterval, maxIdleGruntInterval);
        }
    }
    
    #region Gizmos
    void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;
        
        // === DETECTION RANGE ===
        if (useVisionCone)
        {
            // Determine facing direction
            Vector2 facingDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
            
            // Draw the vision cone
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f); // Semi-transparent yellow
            
            // Draw the cone edges
            float halfAngle = visionConeAngle / 2f;
            Vector2 coneEdge1 = Quaternion.Euler(0, 0, halfAngle) * facingDirection * detectionRange;
            Vector2 coneEdge2 = Quaternion.Euler(0, 0, -halfAngle) * facingDirection * detectionRange;
            
            // Draw lines for cone edges
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, (Vector2)transform.position + coneEdge1);
            Gizmos.DrawLine(transform.position, (Vector2)transform.position + coneEdge2);
            
            // Draw arc at the end of the cone
            Vector3 previousPoint = (Vector2)transform.position + coneEdge1;
            for (int i = 1; i <= 20; i++)
            {
                float angle = Mathf.Lerp(-halfAngle, halfAngle, i / 20f);
                Vector2 point = Quaternion.Euler(0, 0, angle) * facingDirection * detectionRange;
                Gizmos.DrawLine(previousPoint, (Vector2)transform.position + point);
                previousPoint = (Vector2)transform.position + point;
            }
        }
        else
        {
            // Draw sphere detection
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
        }
        
        // === RANGED ATTACK RANGE ===
        if (useRangedAttack)
        {
            // Draw ranged attack range in cyan/light blue
            Gizmos.color = new Color(0f, 1f, 1f, 0.5f); // Cyan
            Gizmos.DrawWireSphere(transform.position, rangedAttackRange);
            
            #if UNITY_EDITOR
            UnityEditor.Handles.color = new Color(0f, 1f, 1f, 0.2f);
            UnityEditor.Handles.DrawSolidDisc(transform.position, Vector3.forward, rangedAttackRange);
            #endif
        }
        else
        {
            // === MELEE ATTACK RANGE ===
            if (useAttackCone)
            {
                // Determine facing direction
                Vector2 facingDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
                
                // Draw the attack cone in red
                Gizmos.color = new Color(1f, 0f, 0f, 0.3f); // Semi-transparent red
                
                // Draw the cone edges
                float halfAngle = attackConeAngle / 2f;
                Vector2 coneEdge1 = Quaternion.Euler(0, 0, halfAngle) * facingDirection * attackRange;
                Vector2 coneEdge2 = Quaternion.Euler(0, 0, -halfAngle) * facingDirection * attackRange;
                
                // Draw lines for cone edges
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, (Vector2)transform.position + coneEdge1);
                Gizmos.DrawLine(transform.position, (Vector2)transform.position + coneEdge2);
                
                // Draw arc at the end of the cone
                Vector3 previousPoint = (Vector2)transform.position + coneEdge1;
                for (int i = 1; i <= 20; i++)
                {
                    float angle = Mathf.Lerp(-halfAngle, halfAngle, i / 20f);
                    Vector2 point = Quaternion.Euler(0, 0, angle) * facingDirection * attackRange;
                    Gizmos.DrawLine(previousPoint, (Vector2)transform.position + point);
                    previousPoint = (Vector2)transform.position + point;
                }
            }
            else
            {
                // Draw sphere attack range
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, attackRange);
            }
            
            // Draw attack hitbox (actual damage area)
            Vector2 attackDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
            Vector2 attackPosition = (Vector2)transform.position + attackDirection * attackHitboxRadius;
            Gizmos.color = new Color(1f, 0.5f, 0f, 1f); // Orange for hitbox
            Gizmos.DrawWireSphere(attackPosition, attackHitboxRadius);
        }
        
        // === SKITTISH BEHAVIOR RANGES ===
        if (isSkittish && fleeState != null)
        {
            float safeDistance = fleeState.safeDistance;
            float fleeThreshold = safeDistance * 0.7f;
            
            // Draw flee threshold (inner circle - red zone)
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f); // Red
            Gizmos.DrawWireSphere(transform.position, fleeThreshold);
            
            // Draw safe distance (outer circle - green zone)
            Gizmos.color = new Color(0f, 1f, 0f, 0.3f); // Green
            Gizmos.DrawWireSphere(transform.position, safeDistance);
            
            #if UNITY_EDITOR
            // Fill the safe zone between flee threshold and safe distance
            UnityEditor.Handles.color = new Color(0f, 1f, 0f, 0.1f);
            UnityEditor.Handles.DrawSolidDisc(transform.position, Vector3.forward, safeDistance);
            UnityEditor.Handles.color = new Color(1f, 0f, 0f, 0.1f);
            UnityEditor.Handles.DrawSolidDisc(transform.position, Vector3.forward, fleeThreshold);
            #endif
        }
        
        // === SPELL CAST RANGE ===
        if (canCastSpells && spellCastRange < 100f)
        {
            // Draw spell range in purple
            Gizmos.color = new Color(0.5f, 0f, 1f, 0.3f); // Purple
            Gizmos.DrawWireSphere(transform.position, spellCastRange);
        }
        
        // === GROUND CHECK ===
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireCube(groundCheck.bounds.center, groundCheck.bounds.size);
        }
        else
        {
            // Fallback visualization
            Vector2 groundCheckPosition = new Vector2(transform.position.x, transform.position.y - 0.5f);
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheckPosition, groundCheckRadius);
        }

        // === EDGE DETECTION ===
        if (!isFlying)
        {
            Gizmos.color = Color.blue;
            float dir = rb != null && Mathf.Abs(rb.linearVelocity.x) > 0.1f ? Mathf.Sign(rb.linearVelocity.x) : 1f;
            
            Vector2 rayStart;
            if (groundCheck != null)
            {
                rayStart = new Vector2(
                    groundCheck.bounds.center.x + dir * edgeCheckDistance,
                    groundCheck.bounds.center.y
                );
            }
            else if (groundCheckPoint != null)
            {
                rayStart = new Vector2(
                    transform.position.x + dir * edgeCheckDistance,
                    groundCheckPoint.position.y
                );
            }
            else
            {
                rayStart = new Vector2(
                    transform.position.x + dir * edgeCheckDistance,
                    transform.position.y - 0.5f
                );
            }
            
            // Draw the edge detection ray
            Gizmos.DrawLine(rayStart, rayStart + Vector2.down * (edgeCheckDistance * 2));
            Gizmos.DrawWireSphere(rayStart, 0.1f);
        }
        
        // === FLYING ENEMY OBSTACLE DETECTION ===
        if (isFlying)
        {
            // Draw ground detection ray
            bool tooClose = IsTooCloseToGround();
            Gizmos.color = tooClose ? Color.red : Color.green;
            Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
            
            // Draw minimum flying height zone
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.2f); // Orange
            Gizmos.DrawLine(transform.position + Vector3.left * 0.5f + Vector3.down * minFlyingHeight, 
                           transform.position + Vector3.right * 0.5f + Vector3.down * minFlyingHeight);
            
            // Draw wall detection
            float dir = rb != null && Mathf.Abs(rb.linearVelocity.x) > 0.1f ? Mathf.Sign(rb.linearVelocity.x) : 1f;

            Gizmos.DrawLine(transform.position, 
                           transform.position + Vector3.right * dir * flyingWallCheckDistance);
        }
        
        // === STATE LABEL ===
        #if UNITY_EDITOR
        string stateInfo = currentState != null ? currentState.GetType().Name.Replace("Enemy", "").Replace("State", "") : "No State";
        if (shouldReverseDirection) stateInfo += " (Reversing)";
        if (isSkittish) stateInfo += " [Skittish]";
        if (useRangedAttack) stateInfo += " [Ranged]";
        if (uncancellableAttacks) stateInfo += " [Uncancel]";
        
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, stateInfo);
        
        // Legend
        Vector3 legendPos = transform.position + Vector3.up * 3f + Vector3.right * 2f;
        string legend = "Yellow=Detection";
        if (useRangedAttack) legend += " | Cyan=RangedAtk";
        else legend += " | Red=MeleeAtk";
        if (isSkittish) legend += " | Red/Green=Flee/Safe";
        if (canCastSpells && spellCastRange < 100f) legend += " | Purple=Spell";
        
        UnityEditor.Handles.Label(legendPos, legend);
        #endif
    }
    #endregion

    public bool IsWallAhead()
    {
        if (!isFlying) return false;
        
        float direction = Mathf.Sign(rb.linearVelocity.x);
        if (direction == 0) direction = lastMoveDirection;
        
        Vector2 rayOrigin = transform.position;
        Vector2 rayDirection = Vector2.right * direction;
        
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDirection, flyingWallCheckDistance, groundLayer);
        
        return hit.collider != null;
    }
    
    public bool IsTooCloseToGround()
    {
        if (!isFlying) return false;
        
        Vector2 rayOrigin = transform.position;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, groundCheckDistance, groundLayer);
        
        if (hit.collider != null)
        {
            float heightAboveGround = hit.distance;
            return heightAboveGround < minFlyingHeight;
        }
        
        return false;
    }
    
    public bool IsOutOfBounds()
    {
        if (!isFlying || worldBoundsCollider == null) return false;
        
        Bounds bounds = worldBoundsCollider.bounds;
        Vector2 pos = transform.position;
        
        return pos.x < bounds.min.x || pos.x > bounds.max.x ||
            pos.y < bounds.min.y || pos.y > bounds.max.y;
    }

    public void ReturnToArenaCenter()
    {
        if (!isFlying || worldBoundsCollider == null) return;
        
        Vector2 arenaCenter = worldBoundsCollider.bounds.center;
        Vector2 directionToCenter = (arenaCenter - (Vector2)transform.position).normalized;
        
        rb.linearVelocity = directionToCenter * returnToArenaSpeed;
    }
}