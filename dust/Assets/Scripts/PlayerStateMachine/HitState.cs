using UnityEngine;

public class HitState : State
{
    public AnimationClip hitAnim;
    private float timer;

    public override void Enter()
    {
        isComplete = false;

        timer = hitAnim.length;
        animator.Play(hitAnim.name, 0, 0f);
    }
    public override void Do()
    {
        timer -= Time.deltaTime;

        input.control = 0f;

        if (timer <= 0f)
        {
            isComplete = true;
        }
    }
    public override void Exit()
    { }
    
}
