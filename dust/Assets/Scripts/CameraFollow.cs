using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    [Header("Follow")]
    public Transform target;
    [Range(0f, 1f)] public float smooth = 0.15f;

    [Header("World Bounds (in world units)")]
    public Vector2 minBounds;
    public Vector2 maxBounds;

    [Header("Optional: drive from a BoxCollider2D")]
    public BoxCollider2D worldBoundsCollider;

    Camera cam;
    Vector3 velocity;

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (worldBoundsCollider != null)
        {
            var b = worldBoundsCollider.bounds;
            minBounds = b.min;
            maxBounds = b.max;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desired = new Vector3(target.position.x, target.position.y, transform.position.z);

        Vector3 pos = Vector3.SmoothDamp(transform.position, desired, ref velocity, smooth);

        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        float clampedX = Mathf.Clamp(pos.x, minBounds.x + halfWidth,  maxBounds.x - halfWidth);
        float clampedY = Mathf.Clamp(pos.y, minBounds.y + halfHeight, maxBounds.y - halfHeight);

        if (maxBounds.x - minBounds.x < halfWidth * 2f) clampedX = (minBounds.x + maxBounds.x) * 0.5f;
        if (maxBounds.y - minBounds.y < halfHeight * 2f) clampedY = (minBounds.y + maxBounds.y) * 0.5f;

        transform.position = new Vector3(clampedX, clampedY, pos.z);
    }

    public void RefreshBoundsFromCollider()
    {
        if (worldBoundsCollider == null) return;
        var b = worldBoundsCollider.bounds;
        minBounds = b.min;
        maxBounds = b.max;
    }
}
