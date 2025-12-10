using UnityEngine;

/// Death state for the boss.
/// Plays death animation, disables the boss, and handles cleanup.
public class BossDeathState : BossState
{
    [Header("Death Settings")]
    public float deathAnimationTime = 2f;
    public AnimationClip deathAnim;
    
    [Header("Effects")]
    [Tooltip("Optional particle effect to spawn on death")]
    public GameObject deathParticles;
    
    public override void Enter()
    {
        base.Enter();
        
        Debug.Log("BossDeathState.Enter() - Boss is dying");
        
        // Play death sound through boss's audio system
        AudioSource audio = boss.GetComponent<AudioSource>();
        if (audio != null && audio.clip != null)
        {
            audio.Play();
        }
        
        // Force death animation to play immediately with normalized time 0 (start)
        // Layer 0, normalized time 0 ensures animation starts from beginning
        if (boss.animator != null && deathAnim != null)
        {
            boss.animator.Play(deathAnim.name, 0, 0f);
            deathAnimationTime = deathAnim.length;
        }
        else if (boss.animator != null)
        {
            // Fallback to animation name if no clip assigned
            boss.animator.Play("Boss_Death", 0, 0f);
        }
        
        // Stop all movement
        if (boss.rb != null)
        {
            boss.rb.linearVelocity = Vector2.zero;
            boss.rb.bodyType = RigidbodyType2D.Kinematic;
        }
        
        // Disable collider to prevent further damage
        Collider2D collider = boss.GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }
        
        // Spawn death particles
        if (deathParticles != null)
        {
            Instantiate(deathParticles, boss.transform.position, Quaternion.identity);
        }
    }
    
    public override void Do()
    {
        // Destroy boss GameObject after death animation completes
        if (time >= deathAnimationTime)
        {
            Debug.Log($"BossDeathState: Destroying boss after {time:F2}s");
            if (boss != null && boss.gameObject != null)
            {
                Destroy(boss.gameObject);
            }
        }
    }
    
    public override void Exit()
    {
        // Death state should never be exited
    }
}
