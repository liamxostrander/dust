using UnityEngine;

public class DeathState : State
{
    public AnimationClip deathAnim;
    public override void Enter()
    {
        isComplete = false;
        animator.Play(deathAnim.name, 0, 0f);
    }
    public override void Do() { }
    public override void Exit(){ }
    
}
