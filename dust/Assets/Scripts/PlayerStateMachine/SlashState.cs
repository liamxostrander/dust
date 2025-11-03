using UnityEngine;

public class SlashState : State
{
    [Header("Attack Animations")]
    public AnimationClip attack_1_anim;
    public AnimationClip attack_2_anim;
    private AnimationClip[] attack_anims;
    int currentAttack = 0;

    [Header("Attack Properties")]
    public float attack1Impulse = 4f;
    public float attack2Impulse = 3f;
    public float reducedControlFactor = 0.2f;
    public float comboWindow = 0.25f;
    float inputBufferTimer = 0f;
    public float inputBufferTime = 0.2f;

    float timer;
    bool queuedNext;
    bool canQueueNext;
    public override void Enter()
    {
        isComplete = false;
        attack_anims = new AnimationClip[] { attack_1_anim, attack_2_anim };
        if (currentAttack >= attack_anims.Length)
            currentAttack = 0;

        animator.Play(attack_anims[currentAttack].name, 0, 0f);
        timer = attack_anims[currentAttack].length;
        
        queuedNext = false;
        canQueueNext = false;

        input.control *= reducedControlFactor;
        Vector2 dir = input.spriteRenderer.flipX ? Vector2.left : Vector2.right;
        input.rb.AddForce(dir * attack1Impulse, ForceMode2D.Impulse);

    }
    public override void Do()
    {
        timer -= Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.J))
            inputBufferTimer = inputBufferTime;
        if (inputBufferTimer > 0f)
            inputBufferTimer -= Time.deltaTime;

        if (timer < comboWindow && !canQueueNext)
        {
            canQueueNext = true;
        }
        if (canQueueNext && inputBufferTimer > 0f)
        {
            queuedNext = true;
            inputBufferTimer = 0f;
        }

        if (timer <= 0f)
        {
            if (queuedNext && currentAttack + 1 < attack_anims.Length)
            {
                currentAttack++;
                timer = attack_anims[currentAttack].length;
                Vector2 dir = input.spriteRenderer.flipX ? Vector2.left : Vector2.right;
                input.rb.AddForce(-dir * attack2Impulse, ForceMode2D.Impulse);
                animator.Play(attack_anims[currentAttack].name, 0, 0f);
            }
            else
            {
                isComplete = true;
            }
        }
    }
    public override void Exit()
    {
        input.control = input.groundControl;
        currentAttack = 0;
    }
    
}
