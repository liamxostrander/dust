using UnityEngine;

public class SlashStateOld : State
{
    [Header("Attack Animations")]
    public AnimationClip attack_1_anim;
    public AnimationClip attack_2_anim;
    int currentAttack = 0;

    [Header("Attack Properties")]
    public float attack1Impulse = 4f;
    public float attack2Impulse = 3f;
    public float slash_ctrl = 0.2f;
    private float prev_ctrl;
    public float comboWindow = 0.25f;

    [Header("Sword Properties")]
    public Animator swordAnimator;
    public AnimationClip slash_anim_1;
    public GameObject swordPivot;
    float timer;
    bool queuedNext;
    bool canQueueNext;
    public override void Enter()
    {
        swordPivot.SetActive(true);
        isComplete = false;
        currentAttack = 0;
        timer = attack_1_anim.length;

        queuedNext = false;
        canQueueNext = false;

        prev_ctrl = input.control;
        if (input.isGrounded) input.control = slash_ctrl;
        Vector2 mousePos = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 attackDir = (mousePos - (Vector2)swordPivot.transform.position).normalized;
        input.spriteRenderer.flipX = attackDir.x < 0 ? true : false;
        animator.Play(attack_1_anim.name, 0, 0f);
        SwordAnim(attackDir);
        attackDir.y = 0;
        input.rb.AddForce(attackDir * attack1Impulse, ForceMode2D.Impulse);
    }
    public override void Do()
    {
        timer -= Time.deltaTime;

        if (timer < comboWindow && !canQueueNext && currentAttack == 0)
        {
            canQueueNext = true;
        }
        if (canQueueNext && Input.GetMouseButtonDown(0))
        {
            queuedNext = true;
        }

        if (timer <= 0f)
        {
            if (queuedNext)
            {
                queuedNext = false;
                canQueueNext = false;
                currentAttack++;
                timer = attack_2_anim.length;
                Vector2 dir = input.spriteRenderer.flipX ? Vector2.left : Vector2.right;
                input.rb.AddForce(-dir * attack2Impulse, ForceMode2D.Impulse);
                animator.Play(attack_2_anim.name, 0, 0f);
            }
            else
            {
                isComplete = true;
            }
        }
    }

    private void SwordAnim(Vector2 attackDir)
    {
        float angle = Mathf.Atan2(attackDir.y, attackDir.x) * Mathf.Rad2Deg;
        swordPivot.transform.rotation = Quaternion.Euler(0, 0, angle);

        swordAnimator.Play(slash_anim_1.name, 0, 0f);
    }
    public override void Exit()
    {
        swordPivot.SetActive(false);
        input.isSlashing = false;
        input.control = prev_ctrl;
        currentAttack = 0;
    }

}
