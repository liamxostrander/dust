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


    [Header("Gravity")]
    [SerializeField] float fallMultiplier = 2.0f;
    [SerializeField] float lowJumpMultiplier = 2.5f;
    [SerializeField] float fastFallMultiplier = 3.0f;
    [SerializeField] float maxFallSpeed = -25f;
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

    void Awake()
    {
        damageable = GetComponent<IsDamageable>();
        damageable.OnDamagedWithKnockback.AddListener(ApplyKnockback);
        damageable.OnDeath.AddListener(HandlePlayerDeath);
        rb = GetComponent<Rigidbody2D>();
        playerWeaponController = GetComponent<PlayerWeaponController>();
        playerWeaponController.EquipSword(0);
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
        moveX = Input.GetAxisRaw("Horizontal");

        if (moveX > 0 && !isSlashing)
        {
            spriteRenderer.flipX = false;
        }
        else if (moveX< 0 && !isSlashing)
        {
            spriteRenderer.flipX = true;
        }

    }

    void CheckWeaponEquipped()
    {
        if (Input.GetKey(KeyCode.Alpha1)){
            playerWeaponController.EquipSword(0);
        }
        else if (Input.GetKey(KeyCode.Alpha2))
        {
            playerWeaponController.EquipSword(1);
        }
        
        
    }
    void Update()
    {
        if (ShopMenuUI.Instance != null && ShopMenuUI.Instance.IsOpen)
        {
            moveX = 0f;
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        CheckInput();
        CheckWeaponEquipped();

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
        if (isGrounded) jumpsRemaining = maxJumps;
        if (isDashing) return;

        HandleXMovement();
        HandleJump();
    }

    void HandleXMovement()
    {
        float targetVX = moveX * moveSpeed * control * speedMultiplier;
        if (canMove)
            rb.linearVelocity = new Vector2(Mathf.Lerp(rb.linearVelocity.x, targetVX, 0.35f), rb.linearVelocity.y);
    }
    void HandleJump()
    {
        if (lastJumpPressTimer > 0f)
        {
            if (isGrounded || jumpsRemaining > 0)
            {
                audioSource.clip = jumpSound;
                audioSource.loop = false;
                audioSource.pitch = 1.3f;
                if (audioSource.isPlaying)
                {
                    audioSource.Stop();
                }
                audioSource.Play();
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
                rb.AddForce(Vector2.up * (jumpImpulse * jumpMultiplier), ForceMode2D.Impulse);

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

    void CheckGrounded()
    {
        isGrounded = Physics2D.OverlapAreaAll(groundCheck.bounds.min, groundCheck.bounds.max, groundMask).Length > 0;
    }

    void TryDash(int dir)
    {
        if (!canDash) return;
        if (!allowAirDash && !isGrounded) return;
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
 
        if (rb == null) return;
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
}
