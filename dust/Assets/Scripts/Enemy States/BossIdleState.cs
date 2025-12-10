using UnityEngine;

/// Idle state for the boss between attacks.
/// Plays idle animation and provides a brief pause before the next attack.
public class BossIdleState : BossState
{
    [Header("Idle Settings")]
    public float idleDuration = 1.0f;
    
    private float idleTimer;
    
    public override void Enter()
    {
        base.Enter();
        isComplete = false; // Reset completion flag
        idleTimer = 0f;
        
        Debug.Log("BossIdleState.Enter() - Starting idle period");
        
        // Play idle animation
        if (boss.animator != null)
        {
            boss.animator.Play("Boss_Idle");
        }
        
        // Stop movement
        if (boss.rb != null)
        {
            boss.rb.linearVelocity = Vector2.zero;
        }
    }
    
    public override void Do()
    {
        base.Do();
        idleTimer += Time.deltaTime;
        
        if (idleTimer >= idleDuration)
        {
            Debug.Log($"BossIdleState complete after {idleTimer:F2}s");
            isComplete = true;
        }
    }
    
    public override void Exit()
    {
        base.Exit();
    }
}
