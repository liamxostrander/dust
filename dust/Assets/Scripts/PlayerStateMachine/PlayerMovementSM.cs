using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovementSM : MonoBehaviour
{

    [Header("States")]
    public AirState airState;
    public IdleState idleState;
    public RunState runState;
    public GroundState groundState;
    public SlashState slashState;
    public DashSliceState dashSliceState;
    public DeathState deathState;
    public HitState hitState;
    public bool isDead = false;
    State state;

    [Header("Animator")]
    public Animator animator;
    public SpriteRenderer spriteRenderer;

    [Header("FX")]
    public GameObject jumpFX;
    public GameObject landFX;
    public GameObject dashFX;
    public Transform fxSpawnPoint;
    public Transform fxDashSpawnPoint;

    [Header("Audio")]
    [SerializeField] public AudioSource audioSource;
    public static AudioSource GlobalSFXSource;

    [Header("Movement")]
    [SerializeField] float moveSpeed = 10f;
    [SerializeField] public float airControl = 0.7f;
    [SerializeField] public float groundControl = 1.0f;
    [SerializeField] public float landControl = 0.3f;
    [SerializeField] public float slashControl = 0.05f;
    [SerializeField] public float sliceControl = 0.1f;
    [SerializeField] public float control = 1.0f;
    private bool canMove = true;

    [Header("Jumping")]
    [SerializeField] float jumpImpulse = 14f;
    [SerializeField] int maxJumps = 2;
    [SerializeField] float jumpBufferTime = 0.12f;
    [SerializeField] public AudioClip jumpSound; 
    public float lastFallSpeed;

    [Header("Dash")]
    [SerializeField] float doubleTapWindow = 0.25f;
    [SerializeField] float dashSpeed = 22f;
    [SerializeField] float dashDuration = 0.15f;
    [SerializeField] float dashCooldown = 0.35f;
    [SerializeField] bool allowAirDash = true;
    [SerializeField] public AudioClip dashSound; 

    [Header("Grounding")]
    [SerializeField] LayerMask groundMask;
    [SerializeField] BoxCollider2D groundCheck;

    [Header("Wall Movement")]
    [SerializeField] float wallSlideSpeed = -2.5f;
    [SerializeField] LayerMask wallMask;
    [SerializeField] public CapsuleCollider2D playerCollider;

    bool isTouchingWallLeft;
    bool isTouchingWallRight;
    bool isWallSliding;

    [Header("Gravity")]
    [SerializeField] float fallMultiplier = 2.0f;
    [SerializeField] float lowJumpMultiplier = 2.5f;
    [SerializeField] float fastFallMultiplier = 3.0f;
    [SerializeField] float maxFallSpeed = -25f;

    [Header("Other")]
    public Rigidbody2D rb;
    private IsDamageable damageable;
    float lastJumpPressTimer = 0f;
    public bool isGrounded { get; private set; }
    public float moveX { get; private set; }
    int jumpsRemaining;
    bool isDashing = false;
    public bool hasLanded = false;
    bool canDash = true;
    public bool isSlashing = false;
    float lastLeftTap = -999f, lastRightTap = -999f;

    [Header("Upgrade Modifiers")]
    public float speedMultiplier = 1f;
    public float jumpMultiplier = 1f;

    [Header("Melee Weapons")]
    public PlayerWeaponController playerWeaponController;
    public GameObject currentSword;
    public Transform swordPivot;

    [Header("Spell Spawners")]
    public Transform iceSpellSpawner;




    void Awake()
    {
        damageable = GetComponent<IsDamageable>();
        damageable.OnDamagedWithKnockback.AddListener(ApplyKnockback);
        damageable.OnDeath.AddListener(HandlePlayerDeath);
        rb = GetComponent<Rigidbody2D>();
        playerWeaponController = GetComponent<PlayerWeaponController>();
        rb.freezeRotation = true;
        jumpsRemaining = maxJumps;
        idleState.Setup(rb, animator, this);
        runState.Setup(rb, animator, this);
        airState.Setup(rb, animator, this);
        groundState.Setup(rb, animator, this);
        slashState.Setup(rb, animator, this);
        dashSliceState.Setup(rb, animator, this);
        deathState.Setup(rb, animator, this);
        hitState.Setup(rb, animator, this);
        state = idleState;
        GlobalSFXSource = audioSource;
    }

    void SelectState()
    {
        State newState = state;
        if (isGrounded)
        {
            if (!hasLanded)
            {
                newState = groundState;
            }
            else if (moveX == 0)
            {
                newState = idleState;
            }
            else
            {
                newState = runState;
            }
        }
        else
        {
            newState = airState;
        }
        if (newState != state)
        {
            state = newState;
            state.Enter();
        }
    }

    void CheckInput()
    {
        float x = 0f;
        if (Input.GetKey(KeyCode.A)) x = -1f;
        if (Input.GetKey(KeyCode.D)) x = 1f;
        moveX = x;

        if (moveX > 0 && !isSlashing)
        {
            spriteRenderer.flipX = false;
        }
        else if (moveX< 0 && !isSlashing)
        {
            spriteRenderer.flipX = true;
        }

    }
    void Update()
    {
        if (state == deathState)
        {
            moveX = 0;
            state.Do();
            return;
        }
            
        if (ShopMenuUI.Instance != null && ShopMenuUI.Instance.IsOpen)
        {
            moveX = 0f;
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        CheckInput();

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W))
            lastJumpPressTimer = jumpBufferTime;

        // double-tap left
        if (Input.GetKeyDown(KeyCode.A))
        {
            float now = Time.time;
            if (now - lastLeftTap <= doubleTapWindow) TryDash(-1);
            lastLeftTap = now;
        }
        // double-tap right
        if (Input.GetKeyDown(KeyCode.D))
        {
            float now = Time.time;
            if (now - lastRightTap <= doubleTapWindow) TryDash(+1);
            lastRightTap = now;
        }

        bool arrowPressed =
            Input.GetKeyDown(KeyCode.LeftArrow) ||
            Input.GetKeyDown(KeyCode.RightArrow) ||
            Input.GetKeyDown(KeyCode.UpArrow) ||
            Input.GetKeyDown(KeyCode.DownArrow);

        if (arrowPressed && !isDashing)
        {
            if (!isSlashing)
            {
                isSlashing = true;
                state.Exit();
                state = slashState;
                state.Enter();
            }
        }
        if (arrowPressed && isDashing)
        {
            state.Exit();
            state = dashSliceState;
            state.Enter();
        }
        
        if (lastJumpPressTimer > 0f) lastJumpPressTimer -= Time.deltaTime;
        

        if (state.isComplete)
        {
            state.Exit();
            SelectState();
        }
        state.Do();
    }
    void FixedUpdate()
    {
        CheckGrounded();
        CheckWalls();
        if (isGrounded) jumpsRemaining = maxJumps;
        if (isDashing) return;

        HandleWallSlide();
        HandleXMovement();
        HandleJump();
    }

    void HandleXMovement()
    {
        if (!canMove)
            return;

        float targetVX = moveX * moveSpeed * control * speedMultiplier;
        float currentVX = rb.linearVelocity.x;

        if (Mathf.Abs(moveX) > 0.01f)
        {
            float newVX = Mathf.Lerp(currentVX, targetVX, 0.35f);
            rb.linearVelocity = new Vector2(newVX, rb.linearVelocity.y);
        }
        else
        {
            // if (isGrounded)
            // {
            float friction = 0.90f; // tweak this
            rb.linearVelocity = new Vector2(currentVX * friction, rb.linearVelocity.y);
            // }
            // else
            // {
            //     // In air → do NOTHING so knockback continues naturally
            // }
        }
    }
    void HandleJump()
    {
        // ===============================
        // WALL JUMP
        // ===============================
        if (lastJumpPressTimer > 0f && isWallSliding)
        {
            lastJumpPressTimer = 0f;

            bool left = isTouchingWallLeft;

            // Push player *off* the wall immediately
            float jumpDir = left ? 1f : -1f;

            Vector2 newVel = new Vector2(jumpDir * 10f, jumpImpulse * 1.0f);

            rb.linearVelocity = newVel;

            // Temporarily disable X movement to avoid re-sticking
            StartCoroutine(DisableXMovementFor(0.12f));

            // Play jump sound
            audioSource.clip = jumpSound;
            audioSource.pitch = 1.15f;
            audioSource.loop = false;
            audioSource.Play();

            jumpsRemaining = maxJumps - 1;
            return;
        }

        // ===============================
        // NORMAL JUMP (existing logic)
        // ===============================
        if (lastJumpPressTimer > 0f)
        {
            if (isGrounded || jumpsRemaining > 0)
            {
                audioSource.clip = jumpSound;
                audioSource.loop = false;
                audioSource.pitch = 1.3f;

                if (audioSource.isPlaying)
                    audioSource.Stop();

                audioSource.Play();

                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
                rb.AddForce(Vector2.up * (jumpImpulse * jumpMultiplier), ForceMode2D.Impulse);
                if (jumpFX && fxSpawnPoint)
                {
                    Instantiate(jumpFX, fxSpawnPoint.position, Quaternion.identity);
                }

                if (isGrounded) jumpsRemaining = maxJumps - 1;
                else jumpsRemaining--;

                lastJumpPressTimer = 0f;
            }
        }
        bool jumpHeld = Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.W);
        bool fastFall = Input.GetKey(KeyCode.S);
        Vector2 v = rb.linearVelocity;

        if (v.y < 0f)
        {
            float mult = fastFall ? fastFallMultiplier : fallMultiplier;
            v.y += Physics2D.gravity.y * (mult - 1f) * Time.fixedDeltaTime;
        }
        else if (v.y > 0f && !jumpHeld)
        {
            v.y += Physics2D.gravity.y * (lowJumpMultiplier - 1f) * Time.fixedDeltaTime;
        }

        if (v.y < maxFallSpeed) v.y = maxFallSpeed;
        rb.linearVelocity = v;
    }

    void HandleWallSlide()
    {
        isWallSliding = false;

        if (isGrounded) return;

        bool touchingWall = (isTouchingWallLeft && moveX < 0) || (isTouchingWallRight && moveX > 0);
        
        if (touchingWall)
        {
            isWallSliding = true;

            // Disable horizontal control while sliding
            float vY = rb.linearVelocity.y;

            // Clamp falling speed
            if (vY < wallSlideSpeed)
                vY = wallSlideSpeed;

            rb.linearVelocity = new Vector2(0f, vY);
        }
    }



    void CheckGrounded()
    {
        isGrounded = Physics2D.OverlapAreaAll(groundCheck.bounds.min, groundCheck.bounds.max, groundMask).Length > 0;
    }

    void CheckWalls()
    {
        Collider2D col = playerCollider;
        Vector2 center = col.bounds.center;
        float height = col.bounds.size.y;
        float width = col.bounds.size.x;

        // Use a generous overlap width — guaranteed to hit the wall collider
        float sideOffset = width * 0.55f;  // extends past your capsule edge

        Vector2 boxSize = new Vector2(0.2f, height * 0.9f);

        isTouchingWallLeft  = Physics2D.OverlapBox(center + Vector2.left  * sideOffset, boxSize, 0, wallMask);
        isTouchingWallRight = Physics2D.OverlapBox(center + Vector2.right * sideOffset, boxSize, 0, wallMask);

    }


    void TryDash(int dir)
    {
        if (!canDash) return;
        if (!allowAirDash && !isGrounded) return;
        SpawnDashFX(dir);
        StartCoroutine(DashRoutine(dir));
        audioSource.clip = dashSound;
        audioSource.loop = false;
        audioSource.pitch = 1.3f;
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        audioSource.Play();
    }

    IEnumerator DashRoutine(int dir)
    {
        canDash = false;
        isDashing = true;

        float xSign = Mathf.Sign(dir);

        // preserve current vertical velocity
        rb.linearVelocity = new Vector2(dashSpeed * xSign, rb.linearVelocity.y);

        float t = 0f;
        while (t < dashDuration)
        {
            rb.linearVelocity = new Vector2(dashSpeed * xSign, rb.linearVelocity.y);
            t += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    void SpawnDashFX(int dir)
    {
        if (dashFX == null) return;

        // Base spawn position
        Vector3 pos = fxDashSpawnPoint != null ? fxDashSpawnPoint.position : transform.position;

        float xSign = Mathf.Sign(dir);

        // Rotate effect since your art faces UP originally
        float zRotation = xSign > 0 ? 90f : -90f;
        Quaternion rot = Quaternion.Euler(0f, 0f, zRotation);

        // Offset behind player depending on dash direction
        pos += new Vector3(-xSign * 0.25f, 0f, 0f);

        // IMPORTANT: instantiate WITHOUT parenting → world-space effect
        Instantiate(dashFX, pos, rot);
    }




    public IEnumerator SmoothControlTransition(float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            control = Mathf.Lerp(from, to, t);
            yield return null;
        }
        control = to;
    }
    

    public void DisableMovement(float duration)
    {
        StartCoroutine(ReenableMovement(duration));
    }

    private IEnumerator ReenableMovement(float duration)
    {
        canMove = false;
        yield return new WaitForSeconds(duration);
        canMove = true;
    }

    private void ApplyKnockback(float damage, Vector2 knockback)
    {
 
        if (rb == null || state == deathState) return;
        state.Exit();
        state = hitState;
        state.Enter();
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);

        rb.AddForce(knockback, ForceMode2D.Impulse);
    }
    private void HandlePlayerDeath()
    {
        state.Exit();
        state = deathState;
        state.Enter();
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }
    IEnumerator DisableXMovementFor(float duration)
    {
        float oldControl = control;
        control = 0f;

        yield return new WaitForSeconds(duration);

        control = oldControl;
    }
}
