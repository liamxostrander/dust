using System;
using UnityEngine;
using UnityEngine.Rendering;

public class RunState : State
{
    public AnimationClip anim;
    public override void Enter()
    {
        isComplete = false;
        animator.Play(anim.name);
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
