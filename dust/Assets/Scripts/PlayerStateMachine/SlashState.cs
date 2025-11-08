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
    public override void Enter()
    {
        isComplete = false;
        if (input.isGrounded) input.control = input.slashControl;
        Vector2 mousePos = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 attackDir = (mousePos - (Vector2)swordPivot.transform.position).normalized;
        input.spriteRenderer.flipX = attackDir.x < 0 ? true : false;
        timer = slashAnim.length - animStartTime;
        animator.Play(slashAnim.name, 0, animStartTime);
        

        SwordAnim(attackDir);
        attackDir.y = 0;
        input.rb.AddForce(attackDir * attack1Impulse, ForceMode2D.Impulse);
    }
    public override void Do()
    {
        timer -= Time.deltaTime;

        if (input.isGrounded) input.control = input.slashControl;

        if (timer < comboWindow && Input.GetMouseButtonDown(0))
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

        swordAnimator.Play(swordSlashAnim.name, 0, 0f);

    }
    public override void Exit()
    {
        swordPivot.SetActive(false);
        input.isSlashing = false;
    }
    
}
