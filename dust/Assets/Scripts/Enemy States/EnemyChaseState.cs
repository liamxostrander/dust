using UnityEngine;

/// Chase state where the enemy pursues the player.
/// Jumps when entering from patrol, then chases until in attack range.
public class EnemyChaseState : EnemyState
{
    [Header("Animation")]
    public AnimationClip chaseAnim;
    public float animationSpeedMultiplier = 1.5f;
    
    [Header("Jump Settings")]
    public float jumpForce = 8f;
    
    private EnemyState previousState;

    public override void Enter()
    {
        base.Enter();
        isComplete = false;
        
        // Play chase animation at increased speed
        if (stateMachine.animator != null && chaseAnim != null)
        {
            stateMachine.animator.Play(chaseAnim.name);
            stateMachine.animator.speed = animationSpeedMultiplier;
        }
        
        // Jump when entering chase from patrol (not from attack recovery)
        if (stateMachine.isGrounded && !stateMachine.justFinishedAttack)
        {
            stateMachine.rb.linearVelocity = new Vector2(0, jumpForce);
        }
    }
    
    public override void Do()
    {
        // Check if player is in attack range and cooldown is ready
        float distanceToPlayer = Vector2.Distance(stateMachine.transform.position, stateMachine.player.position);
        if (distanceToPlayer <= stateMachine.attackRange && stateMachine.CanAttack())
        {
            isComplete = true;
            return;
        }
        
        if (!stateMachine.IsPlayerDetected())
        {
            isComplete = true;
        }
    }

    public override void FixedDo()
    {
        // Chase player
        stateMachine.ChasePlayer();
    }
    
    public override void Exit()
    {        
        // Reset animation speed
        if (stateMachine.animator != null)
        {
            stateMachine.animator.speed = 1f;
        }
    }
}