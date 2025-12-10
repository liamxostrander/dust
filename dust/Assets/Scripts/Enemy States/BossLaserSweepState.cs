using UnityEngine;

public class BossLaserSweepState : BossState
{
    [Header("Laser Sweep")]
    public GameObject laserPrefab;           // wide prefab that translates across screen
    public int laserCount = 3;               // number of lasers in the sweep
    public float laserSpacing = 2.5f;        // vertical spacing between lasers
    public float laserSpeed = 8f;            // units per second
    public float sweepDuration = 3.0f;       // how long the sweep lasts (overridden if calculated from speed)
    public float laserLifetime = 5.0f;       // auto-destroy safety
    public bool moveHorizontally = true;     // if true, sweep left-to-right; if false, top-to-bottom

    [Header("Timing")]
    public float damageEnableDelay = 0.1f;  // delay before collider enabled
    public float laserSpawnDelay = 0.3f;    // time between spawning each laser in sequence

    [Header("FX")]
    public AudioClip laserSfx;

    private Coroutine activeRoutine;

    public override void Enter()
    {
        base.Enter();
        isComplete = false; // Reset completion flag
        boss.lastLaser = Time.time;

        Debug.Log("BossLaserSweepState.Enter() - Starting laser sweep");

        // Stop any existing routine
        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
            activeRoutine = null;
        }

        // Boss is already positioned by state machine before entering this state
        activeRoutine = StartCoroutine(SweepRoutine());
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

    System.Collections.IEnumerator SweepRoutine()
    {
        // Small delay before starting the attack
        yield return new WaitForSeconds(0.3f);
        
        if (boss.animator) boss.animator.Play("Boss_LaserPrep");
        if (laserSfx) PlaySfx(laserSfx);

        // Check for null references
        if (boss.worldBoundsCollider == null)
        {
            Debug.LogError("BossLaserSweepState: worldBoundsCollider is null!");
            activeRoutine = null;
            isComplete = true;
            yield break;
        }

        if (laserPrefab == null)
        {
            Debug.LogError("BossLaserSweepState: laserPrefab is not assigned!");
            activeRoutine = null;
            isComplete = true;
            yield break;
        }

        // Calculate sweep distance based on arena size
        var b = boss.worldBoundsCollider.bounds;
        float sweepDistance = moveHorizontally ? (b.size.x * 1.5f) : (b.size.y * 1.5f);

        Debug.Log($"Spawning {laserCount} lasers");

        // Spawn lasers sequentially with stagger
        GameObject[] spawned = new GameObject[laserCount];
        for (int i = 0; i < laserCount; i++)
        {
            spawned[i] = SpawnLaser(i, b);
            Debug.Log($"Spawned laser {i} at position {spawned[i]?.transform.position}");
            if (i < laserCount - 1)
                yield return new WaitForSeconds(laserSpawnDelay);
        }

        // Let them sweep for a brief period (lasers will auto-destroy after laserLifetime anyway)
        float waitTime = sweepDuration;
        Debug.Log($"BossLaserSweepState: Waiting {waitTime:F2}s for lasers to sweep");
        yield return new WaitForSeconds(waitTime);

        Debug.Log("BossLaserSweepState: Sweep complete, marking state as done");
        activeRoutine = null;
        isComplete = true;
    }

    GameObject SpawnLaser(int index, Bounds arenaBounds)
    {
        if (laserPrefab == null) return null;

        Vector3 spawnPos;
        float angle = 0f;
        
        if (moveHorizontally)
        {
            // Enter from left edge, move right with varied height and angle
            float x = (float)arenaBounds.min.x - 5f; // spawn off-screen left
            
            // Distribute lasers more evenly across the vertical space
            float heightRange = (float)arenaBounds.size.y * 0.8f; // Use 80% of arena height
            float startY = (float)arenaBounds.center.y - heightRange / 2f;
            float y = startY + (heightRange / (laserCount - 1)) * index;
            
            spawnPos = new Vector3(x, y, boss.transform.position.z);
            
            // Add slight angle variation (-15 to +15 degrees from horizontal)
            angle = Random.Range(-15f, 15f);
        }
        else
        {
            // Enter from top edge, move down
            float x = (float)arenaBounds.min.x + index * laserSpacing;
            x = Mathf.Clamp(x, (float)arenaBounds.min.x, (float)arenaBounds.max.x);
            float y = (float)arenaBounds.max.y + 5f; // spawn off-screen top
            spawnPos = new Vector3(x, y, boss.transform.position.z);
            
            // Add slight angle variation for vertical lasers
            angle = Random.Range(-15f, 15f) + 90f; // 90 degrees is straight down
        }

        var laser = GameObject.Instantiate(laserPrefab, spawnPos, Quaternion.Euler(0f, 0f, angle));

        // Add movement component
        var mover = laser.AddComponent<LaserBeamMover>();
        mover.speed = laserSpeed;
        mover.moveHorizontal = moveHorizontally;
        mover.sweepDistance = (float)arenaBounds.size.x * 1.5f;
        mover.angle = angle;

        // Enable damage after delay
        var atk = laser.GetComponentInChildren<EnemyAttackDamage>();
        if (atk != null)
        {
            atk.attacker = boss.transform;
            var col = atk.GetComponent<Collider2D>();
            if (col)
            {
                col.enabled = false; // start disabled
                StartCoroutine(EnableColliderAfterDelay(col, damageEnableDelay));
            }
            Debug.Log($"Laser damage component configured: collider={col != null}");
        }
        else
        {
            Debug.LogWarning("Laser prefab is missing EnemyAttackDamage component!");
        }

        GameObject.Destroy(laser, laserLifetime);
        return laser;
    }

    System.Collections.IEnumerator EnableColliderAfterDelay(Collider2D col, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (col != null) col.enabled = true;
    }

    void PlaySfx(AudioClip clip)
    {
        var audio = boss.GetComponent<AudioSource>();
        if (audio && clip) audio.PlayOneShot(clip, 0.9f);
    }
}

// Helper component to move lasers across the screen
public class LaserBeamMover : MonoBehaviour
{
    public float speed = 8f;
    public bool moveHorizontal = true;
    public float sweepDistance = 15f;
    public float angle = 0f;

    private float distanceTraveled;
    private Vector3 moveDirection;

    void Start()
    {
        // Calculate movement direction based on angle
        if (moveHorizontal)
        {
            // For horizontal movement, angle is deviation from straight right
            float radians = angle * Mathf.Deg2Rad;
            moveDirection = new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0f).normalized;
        }
        else
        {
            // For vertical movement, angle is already set (90 = down)
            float radians = angle * Mathf.Deg2Rad;
            moveDirection = new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0f).normalized;
        }
    }

    void FixedUpdate()
    {
        float delta = speed * Time.fixedDeltaTime;
        distanceTraveled += delta;

        transform.position += moveDirection * delta;

        if (distanceTraveled >= sweepDistance)
        {
            Destroy(gameObject);
        }
    }
}

