using UnityEngine;

// Melee attack state where the enemy performs a close-range attack.
// Triggers damage at a specific point in the animation.
// For flying enemies, performs a swoop attack.
public class EnemyMeleeState : EnemyState
{
    [Header("Animation")]
    public AnimationClip attackAnim;
    public float attackDuration = 0.5f;
    
    [Header("Attack Timing")]
    [Range(0f, 1f)]
    public float attackTiming = 0.3f; // When in animation to trigger damage (0-1)
    
    [Header("Swoop Settings")]
    [Tooltip("Distance multiplier - swoop cancels if player is this many times attack range away")]
    public float swoopCancelDistanceMultiplier = 3f;
    
    private bool hasAttacked = false;
    
    // Swoop attack states for flying enemies
    private enum SwoopPhase { Swooping, Returning }
    private SwoopPhase swoopPhase = SwoopPhase.Swooping;
    private Vector2 attackDirection; // Direction bat was moving when it attacked
    private float returnStartTime; // When the return phase started
    
    public override void Enter()
    {
        base.Enter();
        isComplete = false;
        hasAttacked = false;
        swoopPhase = SwoopPhase.Swooping;
        
        if (stateMachine.player != null)
        {
            float directionToPlayer = stateMachine.player.position.x - stateMachine.transform.position.x;
            if (Mathf.Abs(directionToPlayer) > 0.1f)
            {
                if (directionToPlayer > 0)
                {
                    stateMachine.transform.localScale = new Vector3(Mathf.Abs(stateMachine.transform.localScale.x), stateMachine.transform.localScale.y, stateMachine.transform.localScale.z);
                }
                else
                {
                    stateMachine.transform.localScale = new Vector3(-Mathf.Abs(stateMachine.transform.localScale.x), stateMachine.transform.localScale.y, stateMachine.transform.localScale.z);
                }
            }
        }
        
        if (stateMachine.isFlying && stateMachine.useSwoopAttack)
        {
            stateMachine.preSwoopPosition = stateMachine.transform.position;
        }
        else
        {
            stateMachine.rb.linearVelocity = new Vector2(0, stateMachine.rb.linearVelocity.y);
        }
        
        if (stateMachine.animator != null && attackAnim != null)
        {
            stateMachine.animator.Play(attackAnim.name);
            attackDuration = attackAnim.length;
        }
    }
    
    public override void Do()
    {
        if (stateMachine.isFlying && stateMachine.useSwoopAttack)
        {
            if (swoopPhase == SwoopPhase.Swooping)
            {
                // Check if enemy has intersected with or passed through player's position
                float distanceToPlayer = Vector2.Distance(stateMachine.transform.position, stateMachine.player.position);
                
                // Cancel swoop if player has moved too far away
                if (distanceToPlayer > stateMachine.attackRange * swoopCancelDistanceMultiplier)
                {
                    isComplete = true;
                    return;
                }
                
                // Trigger attack when enemy reaches the player's center position
                if (!hasAttacked && distanceToPlayer <= 0.1f)
                {
                    stateMachine.PlaySound(stateMachine.meleeAttackSound);
                    stateMachine.PerformAttack();
                    hasAttacked = true;
                    swoopPhase = SwoopPhase.Returning;
                    // Store the direction bat was moving for bounce-back
                    attackDirection = stateMachine.rb.linearVelocity.normalized;
                    returnStartTime = time;
                }
            }
            else if (swoopPhase == SwoopPhase.Returning)
            {
                // Bounce back for a short duration then complete
                if (time - returnStartTime >= 0.4f) // Bounce back for 0.4 seconds
                {
                    isComplete = true;
                }
            }
        }
        else
        {
            // Standard attack behavior (ground enemies or flying without swoop)
            // Trigger attack at the specified timing in the animation
            if (!hasAttacked && time >= attackDuration * attackTiming)
            {
                stateMachine.PlaySound(stateMachine.meleeAttackSound);
                stateMachine.PerformAttack();
                hasAttacked = true;
            }
            
            if (time >= attackDuration)
            {
                isComplete = true;
            }
        }
    }
    
    public override void FixedDo()
    {
        if (stateMachine.isFlying && stateMachine.useSwoopAttack)
        {
            if (swoopPhase == SwoopPhase.Swooping)
            {
                // Swoop directly toward player's center position for guaranteed intersection
                Vector2 playerCenter = stateMachine.player.position;
                Vector2 directionToPlayer = (playerCenter - (Vector2)stateMachine.transform.position).normalized;
                stateMachine.rb.linearVelocity = directionToPlayer * stateMachine.swoopSpeed;
            }
            else if (swoopPhase == SwoopPhase.Returning)
            {
                // Bounce back in opposite direction of attack (like knockback)
                Vector2 bounceDirection = -attackDirection;
                stateMachine.rb.linearVelocity = bounceDirection * stateMachine.swoopReturnSpeed;
            }
        }
        else
        {
            stateMachine.rb.linearVelocity = new Vector2(0, stateMachine.rb.linearVelocity.y);
        }
    }
    
    public override void Exit()
    {
        stateMachine.justFinishedAttack = true;
    }
}

