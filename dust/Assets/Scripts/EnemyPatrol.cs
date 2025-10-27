using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private LayerMask groundLayer;
    
    [Header("Edge Detection")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float edgeCheckDistance = 0.5f;
    
    [Header("Lunge Settings")]
    [SerializeField] private float lungeForce = 10f;
    [SerializeField] private float lungeUpwardForce = 5f;
    
    [Header("Ground Check")]
    [SerializeField] private float groundCheckRadius = 0.2f;
    
    private Rigidbody2D rb;
    private Transform player;
    private bool isGrounded;
    private bool hasLunged = false;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        
        if (groundCheckPoint == null)
        {
            GameObject checkPoint = new GameObject("GroundCheckPoint");
            checkPoint.transform.parent = transform;
            checkPoint.transform.localPosition = new Vector3(0.5f, -0.5f, 0f);
            groundCheckPoint = checkPoint.transform;
        }
        
        Collider2D col = GetComponent<Collider2D>();
        if (col != null && col.sharedMaterial == null)
        {
            PhysicsMaterial2D material = new PhysicsMaterial2D();
            material.friction = 0f;
            col.sharedMaterial = material;
        }
    }
    
    void Update()
    {
        isGrounded = Physics2D.OverlapCircle(transform.position, groundCheckRadius, groundLayer);
        
        if (!hasLunged && player != null && isGrounded)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            if (distanceToPlayer <= detectionRange)
            {
                LungeAtPlayer();
                hasLunged = true;
            }
        }
    }
    
    void FixedUpdate()
    {
        if (!isGrounded)
        {
            return; 
        }
        
        if (player != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            
            if (distanceToPlayer <= detectionRange)
            {
                ChasePlayer();
            }
            else
            {
                Patrol();
            }
        }
        else
        {
            Patrol();
        }
    }
    
    void Patrol()
    {
        float currentDirection = rb.linearVelocity.x;
        if (Mathf.Abs(currentDirection) < 0.1f)
        {
            currentDirection = 1f; 
        }
        
        float moveDirection = Mathf.Sign(currentDirection);
        
        Vector2 edgeCheckPos = new Vector2(
            transform.position.x + moveDirection * edgeCheckDistance,
            transform.position.y - 0.5f
        );
        RaycastHit2D edgeHit = Physics2D.Raycast(edgeCheckPos, Vector2.down, edgeCheckDistance, groundLayer);
        
        if (!edgeHit.collider)
        {
            moveDirection *= -1;
        }
        
        rb.linearVelocity = new Vector2(moveDirection * patrolSpeed, rb.linearVelocity.y);
    }
    
    void LungeAtPlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        
        rb.linearVelocity = new Vector2(direction.x * lungeForce, lungeUpwardForce);
    }
    
    void ChasePlayer()
    {
        float directionToPlayer = Mathf.Sign(player.position.x - transform.position.x);
        rb.linearVelocity = new Vector2(directionToPlayer * chaseSpeed, rb.linearVelocity.y);
    }
    
    void OnDrawGizmosSelected()
    {
        // Detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Ground check
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, groundCheckRadius);
        
        // Edge check
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.blue;
            float dir = rb != null && Mathf.Abs(rb.linearVelocity.x) > 0.1f ? Mathf.Sign(rb.linearVelocity.x) : 1f;
            Vector2 edgeCheckPos = new Vector2(
                transform.position.x + dir * edgeCheckDistance,
                transform.position.y - 0.5f
            );
            Gizmos.DrawLine(edgeCheckPos, edgeCheckPos + Vector2.down * edgeCheckDistance);
        }
        
        // Draw current state
        string state = "Patrol";
        if (player != null && Vector2.Distance(transform.position, player.position) <= detectionRange)
        {
            state = hasLunged ? "Chase" : "Ready to Lunge";
        }
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, state);
    }
}
