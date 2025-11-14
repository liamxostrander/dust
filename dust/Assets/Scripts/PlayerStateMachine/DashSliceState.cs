using UnityEngine;

public class DashSliceState : State
{
    [Header("Attack Animations")]
    public AnimationClip sliceAnim;
    public float animStartTime = 0.3f;

    [Header("Sword Properties")]
    public Animator swordAnimator;
    public AnimationClip swordDashSliceAnim;
    public GameObject swordPivot;
    float timer;

    [Header("Attack SFX")]
    public AudioClip dashSliceSound; 
    private AudioSource audioSource;
    public override void Enter()
    {
        isComplete = false;
        Vector2 attackDir = Vector2.zero;

        if (Input.GetKey(KeyCode.UpArrow))
            attackDir.y += 1;
        if (Input.GetKey(KeyCode.DownArrow))
            attackDir.y -= 1;
        if (Input.GetKey(KeyCode.RightArrow))
            attackDir.x += 1;
        if (Input.GetKey(KeyCode.LeftArrow))
            attackDir.x -= 1;

        if (attackDir == Vector2.zero)
            attackDir = input.spriteRenderer.flipX ? Vector2.left : Vector2.right;

        attackDir.Normalize();
        input.spriteRenderer.flipX = attackDir.x < 0f;
        animator.Play(sliceAnim.name, 0, animStartTime);
        timer = sliceAnim.length - animStartTime;
        SwordAnim(attackDir);
        if (audioSource == null)
        {
            audioSource = input.audioSource;
        }

        audioSource.clip = dashSliceSound;
        audioSource.loop = false;
        audioSource.time = 0f;
        audioSource.pitch = 1f;
        if (!audioSource.isPlaying)
            audioSource.Play();
    }
    public override void Do()
    {
        if (input.isGrounded) input.control = input.sliceControl;
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            isComplete = true;
        }
    }
    private void SwordAnim(Vector2 attackDir)
    {
        swordPivot.SetActive(true);

        bool facingLeft = attackDir.x < 0f;
        swordPivot.transform.localScale = facingLeft ? new Vector3(1, -1, 1) : Vector3.one;

        float angle = Mathf.Atan2(attackDir.y, attackDir.x) * Mathf.Rad2Deg;
        swordPivot.transform.rotation = Quaternion.Euler(0, 0, angle);

        swordAnimator.Play(swordDashSliceAnim.name, 0, 0f);
    }
    public override void Exit()
    {
        swordPivot.SetActive(false);
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

}
