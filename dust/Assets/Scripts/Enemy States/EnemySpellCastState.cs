using UnityEngine;

/// Spell casting state for miniboss enemies.
/// The enemy channels a spell for a duration, then casts it at the player's position.
public class EnemySpellCastState : EnemyState
{
    [Header("Animation")]
    public AnimationClip castAnim;
    public float castDuration = 1.5f;
    
    [Header("Spell Timing")]
    [Range(0f, 1f)]
    public float spellCastTiming = 0.7f; // When in animation to trigger spell (0-1)
    
    [Header("Spell Prefab")]
    public GameObject lightningSpellPrefab;
    
    private bool hascastSpell = false;
    
    public override void Enter()
    {
        base.Enter();
        isComplete = false;
        hascastSpell = false;
        
        // Stop movement during spell cast
        stateMachine.rb.linearVelocity = new Vector2(0, stateMachine.rb.linearVelocity.y);
        
        // Play casting animation
        if (stateMachine.animator != null && castAnim != null)
        {
            stateMachine.animator.Play(castAnim.name);
            castDuration = castAnim.length;
        }
    }
    
    public override void Do()
    {
        // Keep enemy stationary during cast
        stateMachine.rb.linearVelocity = new Vector2(0, stateMachine.rb.linearVelocity.y);
        
        // Trigger spell at the specified timing in the animation
        if (!hascastSpell && time >= castDuration * spellCastTiming)
        {
            CastSpell();
            hascastSpell = true;
        }
        
        // Complete state when animation finishes
        if (time >= castDuration)
        {
            isComplete = true;
        }
    }
    
    private void CastSpell()
    {
        if (stateMachine.player == null)
        {
            Debug.LogWarning("Cannot cast spell - player not found!");
            return;
        }
        
        if (lightningSpellPrefab == null)
        {
            Debug.LogError("Lightning spell prefab not assigned!");
            return;
        }
        
        // Spawn lightning above player's current position
        Vector3 playerPosition = stateMachine.player.position;
        Vector3 spellSpawnPosition = new Vector3(playerPosition.x, playerPosition.y + 5f, playerPosition.z);
        
        GameObject spell = Instantiate(lightningSpellPrefab, spellSpawnPosition, Quaternion.identity);
        
        // Optional: Pass damage value to the spell
        LightningSpell lightningScript = spell.GetComponent<LightningSpell>();
        if (lightningScript != null)
        {
            lightningScript.damage = stateMachine.spellDamage;
            lightningScript.targetPosition = playerPosition;
        }
        
        Debug.Log($"Miniboss cast lightning spell at player position: {playerPosition}");
    }
    
    public override void Exit()
    {
        base.Exit();
        
        // Reset the spell cooldown in the state machine
        stateMachine.lastSpellCastTime = Time.time;
    }
}
