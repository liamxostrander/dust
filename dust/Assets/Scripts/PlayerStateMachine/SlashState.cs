using UnityEngine;

public class SlashState : State
{
    [Header("Attack Animations")]
    public AnimationClip slashAnim;
    public float animStartTime = 0.3f;

    [Header("Attack Properties")]
    public float attack1Impulse = 4f;
    public float comboWindow = 0.25f;

    [Header("Sword Properties")]
    public Animator swordAnimator;
    public AnimationClip swordSlashAnim;
    public GameObject swordPivot;
    float timer;

    [Header("Attack SFX")]
    public AudioClip slashSound; 
    private AudioSource audioSource;
    public override void Enter()
    {
        isComplete = false;
        if (input.isGrounded) input.control = input.slashControl;
        // Vector2 mousePos = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
        // Vector2 attackDir = (mousePos - (Vector2)swordPivot.transform.position).normalized;
        // input.spriteRenderer.flipX = attackDir.x < 0 ? true : false;
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
        // input.spriteRenderer.flipX = attackDir.x < 0 ? true : false;
        attackDir.Normalize();
        input.spriteRenderer.flipX = attackDir.x < 0;

        timer = slashAnim.length - animStartTime;
        animator.Play(slashAnim.name, 0, animStartTime);
        

        SwordAnim(attackDir);
        attackDir.y = 0;
        input.rb.AddForce(attackDir * attack1Impulse, ForceMode2D.Impulse);

        if (audioSource == null)
        {
            audioSource = input.GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = input.gameObject.AddComponent<AudioSource>();
        }

        audioSource.clip = slashSound;
        audioSource.loop = false;
        audioSource.volume = 0.8f;
        audioSource.time = 0f;
        audioSource.pitch = 1f;
        if (!audioSource.isPlaying)
            audioSource.Play();
    }
    public override void Do()
    {
        timer -= Time.deltaTime;

        if (input.isGrounded) input.control = input.slashControl;

        if (timer < comboWindow && (
            Input.GetKeyDown(KeyCode.UpArrow) ||
            Input.GetKeyDown(KeyCode.DownArrow) ||
            Input.GetKeyDown(KeyCode.LeftArrow) ||
            Input.GetKeyDown(KeyCode.RightArrow)))
        {
            Enter();
        }

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

        swordAnimator.Play(swordSlashAnim.name, 0, 0f);
        // swordPivot.SetActive(true);

        // bool facingLeft = attackDir.x < 0f;

        // if (facingLeft)
        // {
        //     swordPivot.transform.localScale = new Vector3(1, -1, 1);
        // }
        // else
        // {
        //     swordPivot.transform.localScale = Vector3.one;
        // }

        // float angle = Mathf.Atan2(attackDir.y, attackDir.x) * Mathf.Rad2Deg;
        // swordPivot.transform.rotation = Quaternion.Euler(0, 0, angle);

        // swordAnimator.Play(swordSlashAnim.name, 0, 0f);

    }
    public override void Exit()
    {
        swordPivot.SetActive(false);
        input.isSlashing = false;
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
    
}
