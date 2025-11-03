using UnityEngine;

// Melee attack state where the enemy performs a close-range attack.
// Triggers damage at a specific point in the animation.
public class EnemyMeleeState : EnemyState
{
    [Header("Animation")]
    public AnimationClip attackAnim;
    public float attackDuration = 0.5f;
    
    [Header("Attack Timing")]
    [Range(0f, 1f)]
    public float attackTiming = 0.3f; // When in animation to trigger damage (0-1)
    
    private bool hasAttacked = false;
    
    public override void Enter()
    {
        base.Enter();
        isComplete = false;
        hasAttacked = false;
        
        // Stop movement during attack
        stateMachine.rb.linearVelocity = new Vector2(0, stateMachine.rb.linearVelocity.y);
        
        if (stateMachine.animator != null && attackAnim != null)
        {
            stateMachine.animator.Play(attackAnim.name);
            attackDuration = attackAnim.length;
        }
    }
    
    public override void Do()
    {
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
    
    public override void FixedDo()
    {
        stateMachine.rb.linearVelocity = new Vector2(0, stateMachine.rb.linearVelocity.y);
    }
    
    public override void Exit()
    {
        stateMachine.justFinishedAttack = true;
    }
}

