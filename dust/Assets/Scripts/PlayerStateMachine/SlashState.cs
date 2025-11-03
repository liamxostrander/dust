using UnityEngine;

public class SlashState : State
{
    [Header("Attack Animations")]
    public AnimationClip attack_1_anim;
    public AnimationClip attack_2_anim;
    int currentAttack = 0;

    [Header("Attack Properties")]
    public float attack1Impulse = 4f;
    public float attack2Impulse = 3f;
    public float reducedControlFactor = 0.2f;
    public float comboWindow = 0.25f;

    float timer;
    bool queuedNext;
    bool canQueueNext;
    public override void Enter()
    {
        isComplete = false;
        currentAttack = 0;

        animator.Play(attack_1_anim.name, 0, 0f);
        timer = attack_1_anim.length;
        
        queuedNext = false;
        canQueueNext = false;

        input.control *= reducedControlFactor;
        Vector2 dir = input.spriteRenderer.flipX ? Vector2.left : Vector2.right;
        input.rb.AddForce(dir * attack1Impulse, ForceMode2D.Impulse);
    }
    public override void Do()
    {
        timer -= Time.deltaTime;

        if (timer < comboWindow && !canQueueNext && currentAttack == 0)
        {
            canQueueNext = true;
        }
        if (canQueueNext && Input.GetKeyDown(KeyCode.J))
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
    public override void Exit()
    {
        input.isSlashing = false;
        input.control = input.groundControl;
        currentAttack = 0;
    }
    
}
