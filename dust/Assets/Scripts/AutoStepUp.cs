using UnityEngine;

/// Attach to the Player (requires a Rigidbody2D + a collider).
[DefaultExecutionOrder(50)]
[RequireComponent(typeof(Rigidbody2D))]
[DisallowMultipleComponent]
public class AutoStepUp : MonoBehaviour
{
    [Header("Step Settings")]
    public float stepHeight = 1.0f;    
    public float checkDistance = 0.08f;
    public float forwardNudge = 0.06f; 
    public float skin = 0.02f;         

    [Header("Collision")]
    public LayerMask groundMask;       
    public Collider2D bodyCollider;    

    Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (!bodyCollider) bodyCollider = GetComponent<Collider2D>();
    }

    void FixedUpdate()
    {
        if (!bodyCollider || !rb) return;
        float vx = rb.linearVelocity.x;
        if (Mathf.Abs(vx) < 0.01f) return;

        int dir = vx > 0f ? +1 : -1;
        Bounds b = bodyCollider.bounds;

        float footY  = b.min.y + skin;
        float headY  = footY + stepHeight;
        float frontX = (dir > 0) ? b.max.x : b.min.x;

        Vector2 footBoxCenter = new Vector2(frontX + dir * (checkDistance * 0.5f), footY);
        Vector2 headBoxCenter = new Vector2(frontX + dir * (checkDistance * 0.5f), headY);
        Vector2 thinBox       = new Vector2(checkDistance, Mathf.Max(0.06f, (b.size.y * 0.08f)));

        bool footBlocked = Physics2D.OverlapBox(footBoxCenter, thinBox, 0f, groundMask);
        bool headClear   = !Physics2D.OverlapBox(headBoxCenter, thinBox, 0f, groundMask);

        if (footBlocked && headClear)
        {
            float maxLift = stepHeight;
            float inc = Mathf.Max(0.02f, stepHeight / 10f);
            float chosenLift = 0f;
            Vector2 boundsSize2D   = new Vector2(b.size.x, b.size.y) - Vector2.one * (skin * 2f);
            Vector2 boundsCenter2D = (Vector2)b.center;
            Vector2 centerOffset   = boundsCenter2D - rb.position;

            for (float lift = inc; lift <= maxLift + 0.0001f; lift += inc)
            {
                Vector2 newPos = rb.position + new Vector2(dir * forwardNudge, lift);
                Vector2 probeCenter = newPos + centerOffset;

                bool blocked = Physics2D.OverlapBox(probeCenter, boundsSize2D, 0f, groundMask);
                if (!blocked)
                {
                    chosenLift = lift;
                    break;
                }
            }
            if (chosenLift > 0f)
            {
                rb.position += new Vector2(dir * forwardNudge, chosenLift);
            }
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying || !bodyCollider || !rb) return;

        Bounds b = bodyCollider.bounds;
        float vx = rb.linearVelocity.x;
        int dir = vx >= 0 ? +1 : -1;

        float footY  = b.min.y + skin;
        float headY  = footY + stepHeight;
        float frontX = (dir > 0) ? b.max.x : b.min.x;

        Vector2 footBoxCenter = new Vector2(frontX + dir * (checkDistance * 0.5f), footY);
        Vector2 headBoxCenter = new Vector2(frontX + dir * (checkDistance * 0.5f), headY);
        Vector2 thinBox       = new Vector2(checkDistance, Mathf.Max(0.06f, (b.size.y * 0.08f)));

        Gizmos.color = Color.red;   Gizmos.DrawWireCube(footBoxCenter, thinBox);
        Gizmos.color = Color.green; Gizmos.DrawWireCube(headBoxCenter, thinBox);
    }
#endif
}
