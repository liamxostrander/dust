using UnityEngine;

public class HitState : State
{
    public AnimationClip hitAnim;
    private float timer;
    public float additionalKnockbackTime;

    public override void Enter()
    {
        isComplete = false;

        timer = hitAnim.length + additionalKnockbackTime;
        animator.Play(hitAnim.name, 0, 0f);
        input.DisableMovement(timer);
    }
    public override void Do()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            isComplete = true;
        }
    }
    public override void Exit()
    { }
    
}
