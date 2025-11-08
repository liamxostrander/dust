using UnityEngine;

public class SwordAiming : MonoBehaviour
{
    public Transform player;
    public Animator swordAnimator;
    public AnimationClip swordSlashAnim;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0;
            Vector2 dir = (mouseWorld - player.position).normalized;

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            swordAnimator.Play(swordSlashAnim.name, 0, 0f);

        }
    }
}