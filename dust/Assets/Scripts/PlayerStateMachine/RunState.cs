using System;
using UnityEngine;
using UnityEngine.Rendering;

public class RunState : State
{
    public AnimationClip runAnim;
    public override void Enter()
    {
        input.control = input.groundControl;
        isComplete = false;
        animator.Play(runAnim.name);
    }
    public override void Do()
    {
        float velX = body.linearVelocity.x;
        if (!input.isGrounded || Mathf.Abs(velX) < 0.1f)
        {
            isComplete = true;
        }
    }
    public override void Exit() { }
}
