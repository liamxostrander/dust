using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovementOld : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float moveSpeed = 10f;
    [SerializeField] float airControl = 0.7f;

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
    [SerializeField, Range(0f, 1f)] float groundNormalMinY = 0.5f;

    [Header("Gravity")]
    [SerializeField] float fallMultiplier = 2.0f;
    [SerializeField] float lowJumpMultiplier = 2.5f;
    [SerializeField] float fastFallMultiplier = 3.0f;
    [SerializeField] float maxFallSpeed = -25f;

    [Header("Animation")]
    [SerializeField] Animator animator;

    Rigidbody2D rb;
    readonly ContactPoint2D[] tmpContacts = new ContactPoint2D[16];
    float moveX;
    float lastJumpPressTimer = 0f;
    bool isGrounded = false;
    int jumpsRemaining;
    bool isDashing = false;
    bool canDash = true;
    float lastLeftTap = -999f, lastRightTap = -999f;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        jumpsRemaining = maxJumps;
    }

    void Update()
    {
        Debug.Log(animator.GetCurrentAnimatorStateInfo(0).fullPathHash);
        moveX = Input.GetAxisRaw("Horizontal");
        animator.SetBool("isWalking", false);
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            lastJumpPressTimer = jumpBufferTime;

        // double-tap left
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            animator.SetBool("isWalking", true);
            spriteRenderer.flipX = true;
            float now = Time.time;
            if (now - lastLeftTap <= doubleTapWindow) TryDash(-1);
            lastLeftTap = now;
        }
        // double-tap right
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            animator.SetBool("isWalking", true);
            spriteRenderer.flipX = false;
            float now = Time.time;
            if (now - lastRightTap <= doubleTapWindow) TryDash(+1);
            lastRightTap = now;
        }

        if (lastJumpPressTimer > 0f) lastJumpPressTimer -= Time.deltaTime;
    }

    void FixedUpdate()
    {
        isGrounded = CheckGroundedByNormal();

        if (isGrounded) jumpsRemaining = maxJumps;

        if (isDashing) return;

        float control = isGrounded ? 1f : airControl;
        float targetVX = moveX * moveSpeed * control;
        rb.linearVelocity = new Vector2(Mathf.Lerp(rb.linearVelocity.x, targetVX, 0.35f), rb.linearVelocity.y);

        if (lastJumpPressTimer > 0f)
        {
            if (isGrounded || jumpsRemaining > 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
                rb.AddForce(Vector2.up * jumpImpulse, ForceMode2D.Impulse);

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

    bool CheckGroundedByNormal()
    {
        int count = rb.GetContacts(tmpContacts);
        for (int i = 0; i < count; i++)
        {
            var cp = tmpContacts[i];
            if ((groundMask.value & (1 << cp.collider.gameObject.layer)) == 0) continue;

            if (cp.normal.y >= groundNormalMinY) return true; // touching a walkable surface (not a wall)
        }
        return false;
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
}
