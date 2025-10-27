using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class OutOfBounds : MonoBehaviour
{
    [Header("Out-of-bounds zones")]
    [SerializeField] private List<GameObject> outOfBoundsObjects = new List<GameObject>();

    [Header("Respawn")]
    [SerializeField] private Transform respawnPoint;

    [SerializeField] private bool resetVelocity = true;

    [SerializeField] private float respawnCooldown = 0.2f;

    private readonly HashSet<Collider2D> oob2D = new HashSet<Collider2D>();

    private Rigidbody2D rb2D;
    private float cooldownUntil;

    private void Awake()
    {
        rb2D = GetComponent<Rigidbody2D>();
        RebuildOobSet();
    }

    private void RebuildOobSet()
    {
        oob2D.Clear();
        foreach (var go in outOfBoundsObjects)
        {
            if (!go) continue;
            foreach (var c in go.GetComponentsInChildren<Collider2D>(includeInactive: true))
            {
                if (c) oob2D.Add(c);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (oob2D.Contains(other)) HandleOutOfBounds();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (oob2D.Contains(collision.collider)) HandleOutOfBounds();
    }

    private void HandleOutOfBounds()
    {
        if (Time.time < cooldownUntil) return;
        if (respawnPoint == null)
        {
            Debug.LogWarning($"OutOfBounds: No respawn point set on {name}.");
            return;
        }

        transform.position = respawnPoint.position;

        if (resetVelocity && rb2D)
        {
            rb2D.linearVelocity = Vector2.zero;
            rb2D.angularVelocity = 0f;
        }

        cooldownUntil = Time.time + Mathf.Max(0f, respawnCooldown);
    }
}
