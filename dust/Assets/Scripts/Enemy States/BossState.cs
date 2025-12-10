using UnityEngine;

public abstract class BossState : MonoBehaviour
{
    protected BossStateMachine boss;

    public bool isComplete { get; protected set; }
    protected float startTime;
    public float time => Time.time - startTime;

    public virtual void Initialize(BossStateMachine machine) => boss = machine;
    public virtual void Enter() 
    { 
        startTime = Time.time;
        isComplete = false; 
    }
    public virtual void Do() { }
    public virtual void FixedDo() { }
    public virtual void Exit() { }
}