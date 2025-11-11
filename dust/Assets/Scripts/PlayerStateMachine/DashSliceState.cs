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
        Vector2 mousePos = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 attackDir = (mousePos - (Vector2)swordPivot.transform.position).normalized;
        input.spriteRenderer.flipX = attackDir.x < 0 ? true : false;
        animator.Play(sliceAnim.name, 0, animStartTime);
        timer = sliceAnim.length - animStartTime;
        SwordAnim(attackDir);
        if (audioSource == null)
        {
            audioSource = input.GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = input.gameObject.AddComponent<AudioSource>();
        }

        audioSource.clip = dashSliceSound;
        audioSource.loop = false;
        audioSource.volume = 0.3f;
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

        if (facingLeft)
        {
            swordPivot.transform.localScale = new Vector3(1, -1, 1);
        }
        else
        {
            swordPivot.transform.localScale = Vector3.one;
        }


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
