using UnityEngine;

// Patrol state where the enemy walks back and forth.
// Detects edges and player presence to transition to other states.
public class EnemyPatrolState : EnemyState
{
    [Header("Animation")]
    public AnimationClip walkAnim;
    
    public override void Enter()
    {
        base.Enter();
        isComplete = false;
        
        if (stateMachine.animator != null && walkAnim != null)
        {
            stateMachine.animator.Play(walkAnim.name);
        }
    }
    
    public override void Do()
    {
        if (stateMachine.shouldReverseDirection)
        {
            isComplete = true;
            return;
        }
        
        if (stateMachine.IsPlayerDetected())
        {
            isComplete = true;
        }
    }

    public override void FixedDo()
    {
        // Flying enemies check for walls ahead
        if (stateMachine.isFlying && stateMachine.IsWallAhead())
        {
            stateMachine.shouldReverseDirection = true;
        }

        // Flying enemies always patrol, ground enemies only when grounded
        if (stateMachine.isFlying || stateMachine.isGrounded)
        {
            stateMachine.Patrol();
        }
    }
    
    public override void Exit()
    {
        stateMachine.rb.linearVelocity = new Vector2(0, stateMachine.rb.linearVelocity.y);
    }
}