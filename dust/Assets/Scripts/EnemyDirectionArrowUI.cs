using UnityEngine;
using UnityEngine.UI;

public class EnemyDirectionArrowUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Player transform (usually the root of the player).")]
    public Transform player;

    [Tooltip("Camera")]
    public Camera worldCamera;

    [Tooltip("RectTransform of the arrow image in the UI.")]
    public RectTransform arrowRect;

    [Header("Behavior")]
    [Tooltip("Key to toggle the arrow on/off.")]
    public KeyCode toggleKey = KeyCode.L;

    [Tooltip("Hide the arrow if enemy is extremely close.")]
    public float minDistanceToShow = 0.25f;

    [Tooltip("Hide the arrow when no living enemies exist.")]
    public bool hideWhenNoEnemies = true;

    private bool arrowEnabled = true;

    private Image arrowImage;

    void Awake()
    {
        if (worldCamera == null)
            worldCamera = Camera.main;

        if (arrowRect == null)
            arrowRect = GetComponent<RectTransform>();

        if (arrowRect != null)
            arrowImage = arrowRect.GetComponent<Image>();
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            arrowEnabled = !arrowEnabled;

            if (arrowImage != null)
                arrowImage.enabled = arrowEnabled;
        }

        if (!arrowEnabled || player == null || worldCamera == null || arrowRect == null)
        {
            if (arrowImage != null)
                arrowImage.enabled = false;
            return;
        }

        EnemyStateMachine target = FindClosestLivingEnemy();
        if (target == null)
        {
            if (hideWhenNoEnemies && arrowImage != null)
                arrowImage.enabled = false;
            return;
        }

        if (arrowImage != null && !arrowImage.enabled)
            arrowImage.enabled = true;

        Vector3 playerScreen = worldCamera.WorldToScreenPoint(player.position);
        Vector3 enemyScreen  = worldCamera.WorldToScreenPoint(target.transform.position);

        Vector2 dir = (enemyScreen - playerScreen);
        if (dir.sqrMagnitude < minDistanceToShow * minDistanceToShow)
        {
            if (arrowImage != null)
                arrowImage.enabled = false;
            return;
        }

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        arrowRect.localRotation = Quaternion.Euler(0f, 0f, angle);
    }

    private EnemyStateMachine FindClosestLivingEnemy()
    {
        EnemyStateMachine[] enemies = FindObjectsOfType<EnemyStateMachine>();
        if (enemies == null || enemies.Length == 0)
            return null;

        EnemyStateMachine closest = null;
        float closestSqrDist = Mathf.Infinity;
        Vector3 playerPos = player.position;

        foreach (var e in enemies)
        {
            if (e == null) continue;

            IsDamageable dmg = e.GetComponent<IsDamageable>();
            if (dmg == null || !dmg.IsAlive)
                continue;

            float sqr = (e.transform.position - playerPos).sqrMagnitude;
            if (sqr < closestSqrDist)
            {
                closestSqrDist = sqr;
                closest = e;
            }
        }

        return closest;
    }
}
