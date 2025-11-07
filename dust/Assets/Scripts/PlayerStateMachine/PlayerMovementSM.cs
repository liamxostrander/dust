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
    State state;

    [Header("Animator")]
    public Animator animator;
    public SpriteRenderer spriteRenderer;

    [Header("Movement")]
    [SerializeField] float moveSpeed = 10f;
    [SerializeField] public float airControl = 0.7f;
    [SerializeField] public float groundControl = 1.0f;
    [SerializeField] public float landControl = 0.6f;
    [SerializeField] public float control = 1.0f;

    [Header("Jumping")]
    [SerializeField] float jumpImpulse = 14f;
    [SerializeField] int maxJumps = 2;
    [SerializeField] float jumpBufferTime = 0.12f;

    [Header("Dash")]
    [SerializeField] float doubleTapWindow = 0.25f;
    [SerializeField] float dashSpeed = 22f;
    [SerializeField] float dashDuration = 0.15f;
    [SerializeField] float dashCooldown = 0.35f;
    [SerializeField] bool allowAirDash = true;

    [Header("Grounding")]
    [SerializeField] LayerMask groundMask;
    [SerializeField] BoxCollider2D groundCheck;


    [Header("Gravity")]
    [SerializeField] float fallMultiplier = 2.0f;
    [SerializeField] float lowJumpMultiplier = 2.5f;
    [SerializeField] float fastFallMultiplier = 3.0f;
    [SerializeField] float maxFallSpeed = -25f;
    public Rigidbody2D rb;
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

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        jumpsRemaining = maxJumps;
        idleState.Setup(rb, animator, this);
        runState.Setup(rb, animator, this);
        airState.Setup(rb, animator, this);
        groundState.Setup(rb, animator, this);
        slashState.Setup(rb, animator, this);
        dashSliceState.Setup(rb, animator, this);
        state = idleState;
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
    void Update()
    {
        CheckInput();

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            lastJumpPressTimer = jumpBufferTime;

        // double-tap left
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            float now = Time.time;
            if (now - lastLeftTap <= doubleTapWindow) TryDash(-1);
            lastLeftTap = now;
        }
        // double-tap right
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            float now = Time.time;
            if (now - lastRightTap <= doubleTapWindow) TryDash(+1);
            lastRightTap = now;
        }
        if (Input.GetMouseButtonDown(0) && !isDashing)
        {
            if (!isSlashing)
            {
                isSlashing = true;
                state.Exit();
                state = slashState;
                state.Enter();
            }
        }
        if (Input.GetMouseButtonDown(0) && isDashing)
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
        rb.linearVelocity = new Vector2(Mathf.Lerp(rb.linearVelocity.x, targetVX, 0.35f), rb.linearVelocity.y);
    }
    void HandleJump()
    {
        if (lastJumpPressTimer > 0f)
        {
            if (isGrounded || jumpsRemaining > 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
                rb.AddForce(Vector2.up * (jumpImpulse * jumpMultiplier), ForceMode2D.Impulse);

                if (isGrounded) jumpsRemaining = maxJumps - 1;
                else jumpsRemaining--;

                lastJumpPressTimer = 0f;
            }
        }
        bool jumpHeld = Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
        bool fastFall = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
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
}
