using UnityEngine;

public class SwordAiming : MonoBehaviour
{
    public Transform player;
    public Animator swordAnimator;
    public AnimationClip swordSlashAnim;

    void Update()
    {
        Vector2 dir = Vector2.zero;

        if (Input.GetKey(KeyCode.UpArrow))
            dir.y += 1;
        if (Input.GetKey(KeyCode.DownArrow))
            dir.y -= 1;
        if (Input.GetKey(KeyCode.RightArrow))
            dir.x += 1;
        if (Input.GetKey(KeyCode.LeftArrow))
            dir.x -= 1;

        if (dir != Vector2.zero)
        {
            dir.Normalize();

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            transform.rotation = Quaternion.Euler(0, 0, angle);

            if (Input.GetKeyDown(KeyCode.UpArrow) ||
                Input.GetKeyDown(KeyCode.DownArrow) ||
                Input.GetKeyDown(KeyCode.LeftArrow) ||
                Input.GetKeyDown(KeyCode.RightArrow))
            {
                swordAnimator.Play(swordSlashAnim.name, 0, 0f);
            }
        }
        // if (Input.GetMouseButtonDown(0))
        // {
        //     Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //     mouseWorld.z = 0;
        //     Vector2 dir = (mouseWorld - player.position).normalized;

        //     float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        //     transform.rotation = Quaternion.Euler(0, 0, angle);

        //     swordAnimator.Play(swordSlashAnim.name, 0, 0f);

        // }
    }
}