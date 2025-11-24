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
    
    public override void Enter()
    {
        base.Enter();
        isComplete = false;
        
        if (stateMachine.animator != null && chaseAnim != null)
        {
            stateMachine.animator.Play(chaseAnim.name);
            stateMachine.animator.speed = animationSpeedMultiplier;
        }
        
        // Jump when entering chase from patrol (not from attack recovery)
        // Flying enemies don't need to jump
        if (!stateMachine.isFlying && stateMachine.isGrounded && !stateMachine.justFinishedAttack)
        {
            stateMachine.rb.linearVelocity = new Vector2(0, jumpForce);
        }
    }
    
    public override void Do()
    {
        if (stateMachine.IsPlayerInAttackRange() && stateMachine.CanAttack())
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
        stateMachine.ChasePlayer();
    }
    
    public override void Exit()
    {        
        if (stateMachine.animator != null)
        {
            stateMachine.animator.speed = 1f;
        }
    }
}