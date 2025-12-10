using UnityEngine;

public class BossSpikeAttackState : BossState
{
    [Header("Spikes")]
    public GameObject spikePrefab;
    public LayerMask groundLayer;
    public int spikeWaves = 8;
    public float spikeSpacing = 1.5f;
    public float waveDelay = 0.2f;
    [Tooltip("Time before spike becomes active and can damage")]
    public float spikeWarningTime = 0.5f;
    public float spikeLifetime = 3f;
    public AudioClip spikeSfx;

    private Coroutine activeRoutine;

    public override void Enter()
    {
        base.Enter();
        isComplete = false; // Reset completion flag
        boss.lastSpike = Time.time;
        
        Debug.Log($"BossSpikeAttackState.Enter() called.");
        
        // Stop any existing spike routine
        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
            activeRoutine = null;
        }
        
        // Boss is already positioned by state machine before entering this state
        activeRoutine = StartCoroutine(SpikeRoutine());
    }

    public override void Exit()
    {
        base.Exit();
        // Stop the coroutine when exiting state
        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
            activeRoutine = null;
        }
    }

    System.Collections.IEnumerator SpikeRoutine()
    {
        // Small delay before starting the attack
        yield return new WaitForSeconds(0.3f);
        
        if (boss.animator) boss.animator.Play("Boss_SummonSpikes");
        
        // Get horizontal direction from boss to player (X only)
        Vector2 bossPos = boss.transform.position;
        Vector2 playerPos = boss.player.position;
        float directionX = Mathf.Sign(playerPos.x - bossPos.x);
        
        // Spawn spikes in a line from boss towards player along the ground
        for (int wave = 0; wave < spikeWaves; wave++)
        {
            // Calculate horizontal position, let raycast find the ground
            float xPos = bossPos.x + directionX * (wave * spikeSpacing);
            Vector2 spikePos = new Vector2(xPos, bossPos.y);
            SpawnSpikeAtPosition(spikePos);
            if (spikeSfx) PlaySfx(spikeSfx);
            yield return new WaitForSeconds(waveDelay);
        }
        
        activeRoutine = null;
        isComplete = true;
    }

    Vector2? GetGroundPoint(Vector2 origin)
    {
        // Raycast down to find ground
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, 20f, groundLayer);
        return hit.collider ? (Vector2?)hit.point : null;
    }

    void SpawnSpikeAtPosition(Vector2 position)
    {
        if (spikePrefab == null) return;
        
        // Use GetGroundPoint to find the ground below this X position
        Vector2? groundPos = GetGroundPoint(position);
        
        // Skip spawning if no ground found
        if (!groundPos.HasValue)
        {
            Debug.Log($"No ground found for spike at X={position.x}, skipping");
            return;
        }
        
        var spike = GameObject.Instantiate(spikePrefab, groundPos.Value, Quaternion.identity);
        
        // Configure damage component if present
        var damageComponent = spike.GetComponentInChildren<EnemyAttackDamage>();
        if (damageComponent)
        {
            damageComponent.attacker = boss.transform;
        }
        
        // Disable spike damage initially for warning phase
        var collider = spike.GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
            StartCoroutine(EnableSpikeAfterDelay(collider, spikeWarningTime));
        }
        
        GameObject.Destroy(spike, spikeLifetime);
    }

    System.Collections.IEnumerator EnableSpikeAfterDelay(Collider2D collider, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (collider != null) collider.enabled = true;
    }

    void PlaySfx(AudioClip clip)
    {
        var audio = boss.GetComponent<AudioSource>();
        if (audio && clip) audio.PlayOneShot(clip, 0.8f);
    }
}