using System;
using UnityEngine;

public class GroundState : State
{
    public AnimationClip landAnim;
    float timer;
    [SerializeField] float controlLerpDuration = 0.25f;
    [SerializeField] float maxFallSpeedForScaling = 15f;
    public override void Enter()
    {
        isComplete = false;
        float fallSpeed = Mathf.Abs(input.rb.linearVelocity.y);
        float t = Mathf.Clamp01(input.lastFallSpeed / maxFallSpeedForScaling);
        float landingControl = Mathf.Lerp(input.groundControl, input.landControl, t);
        Debug.Log(input.groundControl + " " + input.landControl + " " + input.lastFallSpeed + " " + t);
        input.control = landingControl;
        timer = landAnim.length / 2;
        animator.Play(landAnim.name, 0, 0f);
    }
    public override void Do()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f || !input.isGrounded)
        {
            isComplete = true;
        }
    }
    public override void Exit()
    {
        input.hasLanded = true;
        // input.control = input.groundControl;
        input.StartCoroutine(input.SmoothControlTransition(input.landControl, input.groundControl, controlLerpDuration));
    }
}
