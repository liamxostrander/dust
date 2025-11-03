using UnityEngine;

public class DashSliceState : State
{
    [Header("Attack Animations")]
    public AnimationClip attack_anim;
    float timer;
    public override void Enter()
    {
        isComplete = false;
        Debug.Log(attack_anim.name + " " + animator);
        animator.Play(attack_anim.name, 0, 0f);
        timer = attack_anim.length;
    }
    public override void Do()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            isComplete = true;
        }
    }
    public override void Exit() { }

}
