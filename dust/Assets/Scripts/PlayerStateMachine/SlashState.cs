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
    // var weapon = input.currentSword;  
    // var swordAnimator = weapon.GetComponent<Animator>();
    private WeaponStats stats;
    private Animator swordAnimator;
    private AnimationClip swordSlashAnim;
    public GameObject swordPivot;
    float timer;

    [Header("Attack SFX")]
    public AudioClip slashSound; 
    private AudioSource audioSource;
    public override void Enter()
    {
        isComplete = false;
        GameObject swordObject = input.playerWeaponController.currentWeapon;
        SwordDamage swordDamage = swordObject.GetComponentInChildren<SwordDamage>();
        if (swordDamage != null)
        {
            swordDamage.ResetRecoil();
        }
        swordAnimator = swordObject.GetComponent<Animator>();
        stats = swordObject.GetComponent<WeaponStats>();
        swordSlashAnim = stats.slashAnimation;

        if (input.isGrounded) input.control = input.slashControl;
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
        input.spriteRenderer.flipX = attackDir.x < 0;

        timer = (swordSlashAnim.length > slashAnim.length) ? swordSlashAnim.length - animStartTime : slashAnim.length - animStartTime;
        animator.Play(slashAnim.name, 0, animStartTime);
        

        SwordAnim(attackDir);
        if (stats.isRanged)
        {
            ProjectileLauncher launcher = swordObject.GetComponent<ProjectileLauncher>();
            if (launcher != null)
                launcher.LaunchProjectile(attackDir, input.spriteRenderer.flipX);
        }
        attackDir.y = 0;
        input.rb.AddForce(attackDir * attack1Impulse, ForceMode2D.Impulse);

        if (audioSource == null)
        {
            audioSource = input.audioSource;
        }

        audioSource.clip = slashSound;
        audioSource.loop = false;
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
