using UnityEngine;

public class IdleState : State
{
    public AnimationClip anim;
    public override void Enter()
    {
        isComplete = false;
    }
    public override void Do()
    {
        animator.Play(anim.name);
        if (!input.isGrounded || input.moveX != 0)
        {
            isComplete = true;
        }
    }
    public override void Exit() { }
}
