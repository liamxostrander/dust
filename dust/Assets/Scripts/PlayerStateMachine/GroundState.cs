using System;
using UnityEngine;

public class GroundState : State
{
    public AnimationClip landAnim;
    public AudioClip landSound; 
    private AudioSource audioSource;
    float timer;
    [SerializeField] float controlLerpDuration = 0.25f;
    [SerializeField] float maxFallSpeedForScaling = 15f;
    public override void Enter()
    {
        isComplete = false;
        float fallSpeed = Mathf.Abs(input.rb.linearVelocity.y);
        float t = Mathf.Clamp01(input.lastFallSpeed / maxFallSpeedForScaling);
        float landingControl = Mathf.Lerp(input.groundControl, input.landControl, t);
        input.control = landingControl;
        timer = landAnim.length / 2;
        animator.Play(landAnim.name, 0, 0f);
        if (audioSource == null)
        {
            audioSource = input.GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = input.gameObject.AddComponent<AudioSource>();
        }

        audioSource.clip = landSound;
        audioSource.loop = false;
        audioSource.volume = 0.6f;
        audioSource.time = 0.05f;
        audioSource.pitch = 1.5f;

        if (!audioSource.isPlaying)
            audioSource.Play();
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
