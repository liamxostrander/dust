using UnityEngine;

/// Idle state where the enemy pauses before taking another action.
/// Used for edge detection recovery and post-attack cooldown.
public class EnemyIdleState : EnemyState
{
    [Header("Idle Settings")]
    public float idleDuration = 1f;
    public float attackIdleDuration = 1f;
    public AnimationClip idleAnim;
   
    public override void Enter()
    {
        base.Enter();
        isComplete = false;
        
        stateMachine.rb.linearVelocity = new Vector2(0, stateMachine.rb.linearVelocity.y);
        
        // Shorten idle duration after an attack for faster recovery
        if (stateMachine.justFinishedAttack)
        {
            idleDuration = attackIdleDuration;
        }
        
        if (stateMachine.animator != null && idleAnim != null)
        {
            stateMachine.animator.Play(idleAnim.name);
        }
    }
    
    public override void Do()
    {
        // Wait for idle duration to complete
        if (time >= idleDuration)
        {
            isComplete = true;
        }
    }

    public override void FixedDo()
    {
        stateMachine.rb.linearVelocity = new Vector2(0, stateMachine.rb.linearVelocity.y);
    }
    
    public override void Exit()
    {
        // Handle edge detection
        if (stateMachine.shouldReverseDirection)
        {
            stateMachine.lastMoveDirection *= -1;
            stateMachine.shouldReverseDirection = false;
        }
        
        // Clear attack recovery flags
        if (stateMachine.justFinishedAttack)
        {
            stateMachine.justFinishedAttack = false;
        }
    }
}
