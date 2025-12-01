using UnityEngine;

/// State where the enemy plays a hurt/hit reaction animation when damaged.
/// Briefly interrupts current action before returning to combat.
public class EnemyHurtState : EnemyState
{
    [Header("Hurt Settings")]
    public float hurtDuration = 0.3f;
    public AnimationClip hurtAnim;
    
    private Vector2 knockbackVelocity;
    
    public void SetKnockbackDirection(Vector2 knockback)
    {
        knockbackVelocity = knockback;
        Debug.Log($"Knockback set in hurt state: {knockback}");
    }
    
    public override void Enter()
    {
        base.Enter();
        
        stateMachine.PlaySound(stateMachine.hurtSound);
        
        if (stateMachine.animator != null && hurtAnim != null)
        {
            stateMachine.animator.Play(hurtAnim.name);
            hurtDuration = hurtAnim.length;
        }
        
        // Apply knockback as velocity
        if (stateMachine.rb != null)
        {
            Debug.Log($"Applying knockback velocity: {knockbackVelocity} to rb");
            stateMachine.rb.linearVelocity = knockbackVelocity;
        }
        else
        {
            Debug.LogWarning("No Rigidbody2D found on enemy!");
        }
    }
    
    public override void Do()
    {
        if (time >= hurtDuration)
        {
            isComplete = true;
        }
    }
    
    public override void FixedDo()
    {
    }
    
    public override void Exit()
    {
        knockbackVelocity = Vector2.zero;
    }
}