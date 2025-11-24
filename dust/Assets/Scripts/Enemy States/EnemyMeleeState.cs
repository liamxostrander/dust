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
    
    private bool hasAttacked = false;
    
    // Swoop attack states for flying enemies
    private enum SwoopPhase { Swooping, Returning }
    private SwoopPhase swoopPhase = SwoopPhase.Swooping;
    
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
            // Handle swoop attack for flying enemies
            if (swoopPhase == SwoopPhase.Swooping)
            {
                // Check if close enough to player to trigger attack
                float distanceToPlayer = Vector2.Distance(stateMachine.transform.position, stateMachine.player.position);
                if (!hasAttacked && distanceToPlayer <= stateMachine.attackHitboxRadius * 2f)
                {
                    stateMachine.PerformAttack();
                    hasAttacked = true;
                    swoopPhase = SwoopPhase.Returning;
                }
            }
            else if (swoopPhase == SwoopPhase.Returning)
            {
                Vector2 targetReturnPosition = new Vector2(
                    stateMachine.player.position.x,
                    stateMachine.player.position.y + stateMachine.flyingHeight
                );
                float distanceToReturn = Vector2.Distance(stateMachine.transform.position, targetReturnPosition);
                if (distanceToReturn <= 0.5f)
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
                // Swoop towards player
                Vector2 directionToPlayer = (stateMachine.player.position - stateMachine.transform.position).normalized;
                stateMachine.rb.linearVelocity = directionToPlayer * stateMachine.swoopSpeed;
            }
            else if (swoopPhase == SwoopPhase.Returning)
            {
                Vector2 targetReturnPosition = new Vector2(
                    stateMachine.player.position.x,
                    stateMachine.player.position.y + stateMachine.flyingHeight
                );
                Vector2 directionToReturn = (targetReturnPosition - (Vector2)stateMachine.transform.position).normalized;
                stateMachine.rb.linearVelocity = directionToReturn * stateMachine.swoopReturnSpeed;
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

