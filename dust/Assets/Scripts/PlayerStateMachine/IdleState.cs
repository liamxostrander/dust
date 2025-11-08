using UnityEngine;

public class IdleState : State
{
    public AnimationClip idleAnim;
    public override void Enter()
    {
        isComplete = false;
    }
    public override void Do()
    {
        animator.Play(idleAnim.name);
        if (!input.isGrounded || input.moveX != 0)
        {
            isComplete = true;
        }
    }
    public override void Exit() { }
}
