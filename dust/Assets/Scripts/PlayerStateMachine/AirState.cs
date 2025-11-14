using UnityEngine;

public class AirState : State
{
    public AnimationClip fallAnim;
    public AnimationClip jumpAnim;
    private bool isFalling = false;
    public float jumpSpeed;
    public override void Enter()
    {
        isComplete = false;
        input.control = input.airControl;
        input.hasLanded = false;
        if (input.rb.linearVelocity.y <= 0)
        {
            animator.Play(fallAnim.name);
            isFalling = true;
        }
        else
        {
            animator.Play(jumpAnim.name);
            isFalling = false;
        }
        
    }
    public override void Do()
    {
        if (!isFalling && input.rb.linearVelocity.y < -0.01f)
        {
            animator.CrossFadeInFixedTime(fallAnim.name, 0.1f);
            isFalling = true;
        }
        if (isFalling && input.rb.linearVelocity.y > -0.01f)
        {
            animator.CrossFadeInFixedTime(jumpAnim.name, 0.1f);
            isFalling = false;
        }
        if (input.rb.linearVelocity.y < 0f)
        {
            if (Mathf.Abs(input.rb.linearVelocity.y) > 1f)
                input.lastFallSpeed = Mathf.Abs(input.rb.linearVelocity.y);
        }
        if (input.isGrounded)
        {
            isComplete = true;
        }
    }
    public override void Exit() { }
}
