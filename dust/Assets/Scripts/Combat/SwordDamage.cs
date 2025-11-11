using UnityEngine;

public class SwordDamage : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private float damageAmount = 25f;
    [SerializeField] private LayerMask enemyLayer;
    
    [Header("Knockback Settings")]
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private float knockbackUpwardForce = 0.3f; // Upward component
    [SerializeField] private float playerRecoilForce = 2000f;      // Recoil applied to player
    [SerializeField] private float screenShakeMagnitude = 0.15f;
    [SerializeField] private float screenShakeDuration = 0.1f;

    [Header("Hit SFX")]
    public AudioClip hitSound; 
    private AudioSource audioSource;
    private HitStop hitStop;
    private Rigidbody2D playerRb;
    void Start() {
        hitStop = FindFirstObjectByType<HitStop>();
        playerRb = transform.root.GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & enemyLayer) == 0)
            return;
        
        // Deal damage with knockback
        IsDamageable damageable = other.GetComponent<IsDamageable>();
        if (damageable != null && damageable.IsAlive)
        {
            StartCoroutine(hitStop.DoHitStop(0.04f));
            if (CameraShake.Instance != null)
                CameraShake.Instance.ShakeOnce(screenShakeDuration, screenShakeMagnitude);
            // Calculate direction from player to enemy
            Transform playerTransform = transform.root;
            Vector2 directionToEnemy = (other.transform.position - playerTransform.position).normalized;
            
            // Apply knockback in that direction with upward force
            Vector2 knockback = new Vector2(
                directionToEnemy.x * knockbackForce,
                knockbackUpwardForce * knockbackForce
            );
            
            // Debug.Log($"Applying knockback: {knockback}, direction to enemy: {directionToEnemy}");

            damageable.TakeDamage(damageAmount, knockback);
            
            if (playerRb != null)
            {

                Vector2 recoil = -directionToEnemy * playerRecoilForce;
                Debug.Log(directionToEnemy + " " + playerRecoilForce);
                playerRb.linearVelocity = new Vector2(0f, playerRb.linearVelocity.y); // cancel horizontal momentum
                playerRb.AddForce(recoil, ForceMode2D.Impulse);
                playerRb.GetComponent<PlayerMovementSM>()?.DisableMovement(0.1f);
                audioSource.PlayOneShot(hitSound, 0.3f);
            }
        }
    }
}
