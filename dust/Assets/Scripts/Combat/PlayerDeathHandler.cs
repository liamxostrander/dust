using UnityEngine;

public class PlayerDeathHandler : MonoBehaviour
{
    private IsDamageable damageable;

    void Start()
    {
        damageable = GetComponent<IsDamageable>();
        damageable.OnDeath.AddListener(HandlePlayerDeath);
    }

    private void HandlePlayerDeath()
    {
        var pm = GetComponent<PlayerMovementSM>();
        if (pm != null)
            pm.isDead = true;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }
}
