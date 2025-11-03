using UnityEngine;

/// Main enemy state machine that controls enemy AI behavior.
/// Manages state transitions and provides helper functions for states.
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
    
    [Header("Movement Settings")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    public float detectionRange = 5f;
    public float visionConeAngle = 90f;
    public bool useVisionCone = true;

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

    [Header("States")]
    public EnemyIdleState idleState;
    public EnemyPatrolState patrolState;
    public EnemyChaseState chaseState;
    public EnemyMeleeState attackState;
    public EnemyDeathState deathState;
    
    [Header("Debug")]
    public bool showGizmos = true;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        if (animator == null) animator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        if (idleState == null) idleState = GetComponent<EnemyIdleState>();
        if (patrolState == null) patrolState = GetComponent<EnemyPatrolState>();
        if (chaseState == null) chaseState = GetComponent<EnemyChaseState>();
        if (attackState == null) attackState = GetComponent<EnemyMeleeState>();
        if (deathState == null) deathState = GetComponent<EnemyDeathState>();

        if (idleState != null) idleState.Initialize(this);
        if (patrolState != null) patrolState.Initialize(this);
        if (chaseState != null) chaseState.Initialize(this);
        if (attackState != null) attackState.Initialize(this);
        if (deathState != null) deathState.Initialize(this);

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
        
        if (currentState != null)
        {
            currentState.Do();
            
            if (currentState.isComplete)
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

    private void UpdateSpriteDirection()
    {
        if (rb.linearVelocity.x > 0.1f)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (rb.linearVelocity.x < -0.1f)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }


    /// Determine the next state based on current conditions.
    /// Priority: attack recovery -> attack range -> chase -> edge detection -> patrol
    void SelectState()
    {
        EnemyState newState = currentState;
        
        bool playerInAttackRange = IsPlayerInAttackRange();
        bool playerDetected = IsPlayerDetected();
        
        if (justFinishedAttack)
        {
            newState = idleState;
        }
        else if (playerInAttackRange && Time.time >= lastAttackTime + attackCooldown)
        {
            newState = attackState;
        }
        else if (currentState == idleState && playerInAttackRange)
        {
            newState = idleState;
        }
        else if (playerDetected)
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
    
    /// Check if player is within detection range and vision cone.
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

    /// Check if player is within attack range and attack cone.
    public bool IsPlayerInAttackRange()
    {
        if (player == null) return false;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer > attackRange)
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
        rb.linearVelocity = new Vector2(directionToPlayer * chaseSpeed, rb.linearVelocity.y);
    }
    
    /// Perform melee attack
    public void PerformAttack()
    {
        lastAttackTime = Time.time;
        
        Vector2 attackDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        Vector2 attackPosition = (Vector2)transform.position + attackDirection * (attackHitboxRadius);
        
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPosition, attackHitboxRadius, playerLayer);
        
        if (hits.Length > 0)
        {
            foreach (Collider2D hit in hits)
            {
                Debug.Log($"Enemy attack hit player: {hit.gameObject.name}!");
            }
        }
    }
    
    public bool CanAttack()
    {
        return Time.time >= lastAttackTime + attackCooldown;
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
        
        // Edge detection
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
    
    #region Gizmos
    void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;
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
        
        // Ground check visualization using BoxCollider2D bounds
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

        // Edge check visualization - show both left and right rays
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
        
        // Draw attack range cone or sphere
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
        
        // Draw current state and reverse direction flag
        string state = currentState != null ? currentState.GetType().Name : "No State";
        if (shouldReverseDirection)
        {
            state += " (Will Reverse)";
        }
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, state);
    }
    #endregion
}