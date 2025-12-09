using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEditor.ShaderGraph.Internal;


public class DeathState : State
{
    public AnimationClip deathAnim;
    [Header("Pause Menu UI")]
    public CanvasGroup deathCanvasGroup;
    public GameObject deathMenuUI;
    public Image deathMenuBackground;
    public TextMeshProUGUI deathText;
    public float fadeDuration = 1.5f;
    public float timer;
    public override void Enter()
    {
        isComplete = false;
        animator.Play(deathAnim.name, 0, 0f);
        timer = deathAnim.length;
        deathMenuUI.SetActive(true);
        StartCoroutine(FadeRoutine());
    }   
    public override void Do()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f){
            animator.speed = 0f;
        }
    }
    public override void Exit(){ }

    private IEnumerator FadeRoutine()
    {
        // Fade screen to black
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            deathCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }
    }
    
}
