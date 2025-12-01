using UnityEngine;

/// Death state where the enemy plays a death animation and then is destroyed.
/// Makes the enemy non-interactive and removes it after the animation.
public class EnemyDeathState : EnemyState
{
    [Header("Death Settings")]
    public float deathAnimationTime = 2f;
    public AnimationClip deathAnim;
    
    public override void Enter()
    {
        base.Enter();
        
        stateMachine.PlaySound(stateMachine.deathSound);
        
        if (stateMachine.animator != null && deathAnim != null)
        {
            stateMachine.animator.Play(deathAnim.name);
            deathAnimationTime = deathAnim.length;
        }
        
        stateMachine.rb.linearVelocity = Vector2.zero;
        stateMachine.rb.bodyType = RigidbodyType2D.Kinematic;
        
        Collider2D collider = stateMachine.GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }
    }
    
    public override void Do()
    {
        if (time >= deathAnimationTime)
        {
            GameObject.Destroy(stateMachine.gameObject);
        }
    }
    
    public override void Exit()
    {
        // Death state should never be exited
    }
}