using System;
using UnityEngine;
using UnityEngine.Rendering;

public class RunState : State
{
    public AnimationClip runAnim;
    public AudioClip runLoopSound; 
    private AudioSource audioSource;
    public override void Enter()
    {
        input.control = input.groundControl;
        isComplete = false;
        animator.Play(runAnim.name);

        if (audioSource == null)
        {
            audioSource = input.GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = input.gameObject.AddComponent<AudioSource>();
        }

        audioSource.clip = runLoopSound;
        audioSource.loop = true;
        audioSource.volume = 0.3f;
        audioSource.time = 0.1f;
        audioSource.pitch = 1.5f;

        if (!audioSource.isPlaying)
            audioSource.Play();
    }
    public override void Do()
    {
        float velX = body.linearVelocity.x;
        if (!input.isGrounded || Mathf.Abs(velX) < 0.1f)
        {
            isComplete = true;
        }
    }
    public override void Exit()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}
