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

    [Header("Summon Settings")]
    public GameObject skeletonSummonPrefab;
    public float summonSideOffset = 1.0f; // distance to the side of necromancer
    public Transform summonPoint; // optional child transform, if set overrides side offset
    
    private bool hascastSpell = false;
    
    public override void Enter()
    {
        base.Enter();
        isComplete = false;
        hascastSpell = false;
        
        // Stop movement during spell cast
        stateMachine.rb.linearVelocity = new Vector2(0, stateMachine.rb.linearVelocity.y);
        
        if (stateMachine.animator != null && castAnim != null)
        {
            stateMachine.animator.Play(castAnim.name);
            castDuration = castAnim.length;
        }
    }
    
    public override void Do()
    {
        stateMachine.rb.linearVelocity = new Vector2(0, stateMachine.rb.linearVelocity.y);
        
        if (!hascastSpell && time >= castDuration * spellCastTiming)
        {
            CastSpell();
            hascastSpell = true;
        }
        
        if (time >= castDuration)
        {
            isComplete = true;
        }
    }
    
    private void CastSpell()
    {
        stateMachine.PlaySound(stateMachine.spellCastSound);
        
        if (stateMachine.player == null)
        {
            Debug.LogWarning("Cannot cast spell - player not found!");
            return;
        }

        // Decide which spell to cast (example: alternate, or driven by stateMachine)
        // Replace this selection logic with your own trigger/flag.
        bool castLightning = stateMachine.ShouldCastLightning; // e.g., a method/flag on your state machine

        GameObject prefabToSpawn = castLightning ? lightningSpellPrefab : skeletonSummonPrefab;
        if (prefabToSpawn == null)
        {
            Debug.LogError("Spell prefab not assigned!");
            return;
        }

        Vector3 spawnPos;
        Quaternion spawnRot = Quaternion.identity;

        if (castLightning)
        {
            // Lightning spawns above player's current position
            Vector3 playerPosition = stateMachine.player.position;
            spawnPos = new Vector3(playerPosition.x, playerPosition.y + 5f, playerPosition.z);

            GameObject spell = Instantiate(prefabToSpawn, spawnPos, spawnRot);

            // Configure lightning
            LightningSpell lightningScript = spell.GetComponent<LightningSpell>();
            if (lightningScript != null)
            {
                lightningScript.damage = stateMachine.spellDamage;
                lightningScript.targetPosition = playerPosition;
            }

            // Removed debug log
        }
        else
        {
            // Skeleton spawns next to necromancer (or at summonPoint if provided)
            Transform necro = stateMachine.transform;
            if (summonPoint != null)
            {
                spawnPos = summonPoint.position;
            }
            else
            {
                float facingSign = Mathf.Sign(necro.localScale.x); // assumes scale X indicates facing
                Vector3 sideOffset = new Vector3(summonSideOffset * facingSign, 0f, 0f);
                spawnPos = necro.position + sideOffset;
            }

            GameObject summon = Instantiate(prefabToSpawn, spawnPos, spawnRot);

            // Removed debug log
        }
    }
    
    public override void Exit()
    {
        base.Exit();
        
        stateMachine.lastSpellCastTime = Time.time;
    }
}
