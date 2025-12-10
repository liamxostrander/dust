using UnityEngine;

public class LandingFX : MonoBehaviour
{
    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    // Called immediately when instantiated
    public void SetPlaybackSpeed(float speed)
    {
        if (animator != null)
            animator.speed = speed;
    }
}
