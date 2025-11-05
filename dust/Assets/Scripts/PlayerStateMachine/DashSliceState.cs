using UnityEngine;

public class DashSliceState : State
{
    [Header("Attack Animations")]
    public AnimationClip attack_anim;
    float timer;
    public float start_time = 0.3f;
    public float slice_ctrl = 0.1f;
    private float prev_ctrl;

    public override void Enter()
    {
        isComplete = false;
        animator.Play(attack_anim.name, 0, start_time);
        timer = attack_anim.length - start_time;
        prev_ctrl = input.control;
        
    }
    public override void Do()
    {
        if (input.isGrounded) input.control = slice_ctrl;
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            isComplete = true;
        }
    }
    public override void Exit()
    {
        input.control = prev_ctrl;
    }

}
