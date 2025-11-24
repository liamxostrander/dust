using UnityEngine;

/// Flee state where the enemy runs away from the player.
/// Used for skittish enemies that avoid combat.
public class EnemyFleeState : EnemyState
{
    [Header("Animation")]
    public AnimationClip fleeAnim;
    public float animationSpeedMultiplier = 1.5f;
    
    [Header("Flee Settings")]
    public float safeDistance = 10f; // Distance to maintain from player
    
    public override void Enter()
    {
        base.Enter();
        isComplete = false;
        
        if (stateMachine.animator != null && fleeAnim != null)
        {
            stateMachine.animator.Play(fleeAnim.name);
            stateMachine.animator.speed = animationSpeedMultiplier;
        }
    }
    
    public override void Do()
    {
        if (!stateMachine.IsPlayerDetected())
        {
            isComplete = true;
            return;
        }
        
        // Check if we've reached the flee threshold (70% of safe distance)
        if (stateMachine.player != null)
        {
            float distanceToPlayer = Vector2.Distance(stateMachine.transform.position, stateMachine.player.position);
            float fleeThreshold = safeDistance * 0.7f;
            
            if (distanceToPlayer >= fleeThreshold)
            {
                isComplete = true;
            }
        }
    }

    public override void FixedDo()
    {
        stateMachine.FleeFromPlayer();
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
