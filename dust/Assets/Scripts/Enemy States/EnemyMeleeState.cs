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
        
        // Flying enemies store their position before swooping
        if (stateMachine.isFlying)
        {
            stateMachine.preSwoopPosition = stateMachine.transform.position;
        }
        else
        {
            // Stop movement during ground attack
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
        if (stateMachine.isFlying)
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
                // Check if returned to pre-swoop position
                float distanceToReturn = Vector2.Distance(stateMachine.transform.position, stateMachine.preSwoopPosition);
                if (distanceToReturn <= 0.5f)
                {
                    isComplete = true;
                }
            }
        }
        else
        {
            // Ground enemy attack (original behavior)
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
        if (stateMachine.isFlying)
        {
            if (swoopPhase == SwoopPhase.Swooping)
            {
                // Swoop towards player
                Vector2 directionToPlayer = (stateMachine.player.position - stateMachine.transform.position).normalized;
                stateMachine.rb.linearVelocity = directionToPlayer * stateMachine.swoopSpeed;
            }
            else if (swoopPhase == SwoopPhase.Returning)
            {
                // Return to pre-swoop position
                Vector2 directionToReturn = (stateMachine.preSwoopPosition - (Vector2)stateMachine.transform.position).normalized;
                stateMachine.rb.linearVelocity = directionToReturn * stateMachine.swoopReturnSpeed;
            }
        }
        else
        {
            // Ground enemy stays still during attack
            stateMachine.rb.linearVelocity = new Vector2(0, stateMachine.rb.linearVelocity.y);
        }
    }
    
    public override void Exit()
    {
        stateMachine.justFinishedAttack = true;
    }
}

