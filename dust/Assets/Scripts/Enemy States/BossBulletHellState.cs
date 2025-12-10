using UnityEngine;

public class BossBulletHellState : BossState
{
    [Header("Bullet Hell")]
    public GameObject orbPrefab;
    public int rings = 3;
    public int orbsPerRing = 16;
    public float ringDelay = 0.3f;
    public float orbSpeed = 6f;
    public float orbLifetime = 6f;
    public AudioClip shootSfx;

    private Coroutine activeRoutine;

    public override void Enter()
    {
        base.Enter();
        isComplete = false; // Reset completion flag
        boss.lastBulletHell = Time.time;
        
        // Stop any existing routine
        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
            activeRoutine = null;
        }
        
        // Boss is already positioned by state machine before entering this state
        activeRoutine = StartCoroutine(FireRoutine());
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

    System.Collections.IEnumerator FireRoutine()
    {
        // Small delay before starting the attack
        yield return new WaitForSeconds(0.3f);
        
        if (boss.animator) boss.animator.Play("Boss_BulletHell");
        
        for (int r = 0; r < rings; r++)
        {
            FireRadial(orbsPerRing);
            if (shootSfx) PlaySfx(shootSfx);
            yield return new WaitForSeconds(ringDelay);
        }
        
        activeRoutine = null;
        isComplete = true;
    }

    void FireRadial(int count)
    {
        if (orbPrefab == null) return;
        Vector2 origin = boss.transform.position;
        for (int i = 0; i < count; i++)
        {
            float angle = (360f / count) * i;
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            var orb = GameObject.Instantiate(orbPrefab, origin, Quaternion.identity);
            var rb2d = orb.GetComponent<Rigidbody2D>();
            if (rb2d) rb2d.linearVelocity = dir * orbSpeed;
            
            // Configure damage component
            var damageComponent = orb.GetComponentInChildren<EnemyAttackDamage>();
            if (damageComponent)
            {
                damageComponent.attacker = boss.transform;
            }
            
            GameObject.Destroy(orb, orbLifetime);
        }
    }

    void PlaySfx(AudioClip clip)
    {
        var audio = boss.GetComponent<AudioSource>();
        if (audio && clip) audio.PlayOneShot(clip, 0.7f);
    }
}