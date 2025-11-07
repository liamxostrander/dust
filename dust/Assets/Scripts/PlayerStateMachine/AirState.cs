using UnityEngine;

public class AirState : State
{
    public AnimationClip fall_anim;
    public AnimationClip jump_anim;
    private bool isFalling = false;

    public float jumpSpeed;
    public override void Enter()
    {
        isComplete = false;
        input.control = input.airControl;
        input.hasLanded = false;
        if (input.rb.linearVelocity.y <= 0)
        {
            animator.Play(fall_anim.name);
            isFalling = true;
        }
        else
        {
            animator.Play(jump_anim.name);
            isFalling = false;
        }
        
    }
    public override void Do()
    {
        if (!isFalling && input.rb.linearVelocity.y < -0.01f)
        {
            animator.CrossFadeInFixedTime(fall_anim.name, 0.1f);
            isFalling = true;
        }
        if (isFalling && input.rb.linearVelocity.y > -0.01f)
        {
            animator.CrossFadeInFixedTime(jump_anim.name, 0.1f);
            isFalling = false;
        }
        if (input.isGrounded)
        {
            isComplete = true;
        }
        if (input.isGrounded)
        {
            isComplete = true;
        }
    }
    public override void Exit() { }
}
