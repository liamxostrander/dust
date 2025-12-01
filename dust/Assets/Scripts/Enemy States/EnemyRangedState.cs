using UnityEngine;

/// Ranged attack state where the enemy shoots a projectile at the player.
/// The enemy stops, aims, and fires a bullet prefab at specified intervals.
public class EnemyRangedState : EnemyState
{
    [Header("Animation")]
    public AnimationClip shootAnim;
    public float shootDuration = 0.5f;
    
    [Header("Shoot Timing")]
    [Range(0f, 1f)]
    public float shootTiming = 0.5f; // When in animation to spawn bullet (0-1)
    
    [Header("Bullet Settings")]
    public GameObject bulletPrefab;
    public Transform shootPoint; // Optional: specific point to spawn bullet from
    public float bulletSpawnOffset = 0.5f; // Distance in front of enemy to spawn bullet
    
    private bool hasShot = false;
    
    public override void Enter()
    {
        base.Enter();
        isComplete = false;
        hasShot = false;
        
        stateMachine.rb.linearVelocity = new Vector2(0, stateMachine.rb.linearVelocity.y);
        
        if (stateMachine.animator != null && shootAnim != null)
        {
            stateMachine.animator.Play(shootAnim.name);
            shootDuration = shootAnim.length;
        }
    }
    
    public override void Do()
    {
        stateMachine.rb.linearVelocity = new Vector2(0, stateMachine.rb.linearVelocity.y);
        
        if (!hasShot && time >= shootDuration * shootTiming)
        {
            ShootBullet();
            hasShot = true;
        }
        
        if (time >= shootDuration)
        {
            isComplete = true;
        }
    }
    
    private void ShootBullet()
    {
        stateMachine.PlaySound(stateMachine.rangedAttackSound);
        
        if (bulletPrefab == null)
        {
            Debug.LogError("Bullet prefab not assigned to EnemyRangedState!");
            return;
        }
        
        if (stateMachine.player == null)
        {
            Debug.LogWarning("Cannot shoot - player not found!");
            return;
        }
        
        Vector3 spawnPosition;
        if (shootPoint != null)
        {
            spawnPosition = shootPoint.position;
        }
        else
        {
            Vector2 forward = stateMachine.spriteRenderer.flipX ? Vector2.left : Vector2.right;
            spawnPosition = stateMachine.transform.position + (Vector3)(forward * bulletSpawnOffset);
        }
        
        Vector2 directionToPlayer = (stateMachine.player.position - spawnPosition).normalized;
        
        GameObject bullet = Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);
        
        EnemyBullet bulletScript = bullet.GetComponent<EnemyBullet>();
        if (bulletScript != null)
        {
            bulletScript.Initialize(directionToPlayer, stateMachine.attackDamage);
        }
        else
        {
            Debug.LogWarning("Bullet prefab doesn't have EnemyBullet component!");
        }
        
        Debug.Log($"Enemy shot bullet towards player");
    }
    
    public override void Exit()
    {
        base.Exit();
        
        stateMachine.justFinishedAttack = true;
    }
}
