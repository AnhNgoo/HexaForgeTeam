using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimation : MonoBehaviour
{
    [SerializeField] private Animator animator;
    public Animator Animator => animator;
    private string currentState;

    public void Init(Animator animator)
    {
        SetAnimator(animator);
    }
    public void SetAnimator(Animator animator)
    {
        this.animator = animator;
    }

    public void CrossFade(string stateName, float transitionDuration = 0.1f)
    {
        Debug.Log($"CrossFading to {stateName}");
        animator.CrossFade(stateName, transitionDuration);
    }

    public void CrossFadeOnshot(string stateName, float transitionDuration = 0.1f)
    {
        if (currentState == stateName)
            return;

        animator.CrossFade(stateName, transitionDuration);
        currentState = stateName;
    }

    public void ResetState()
    {
        currentState = null;
    }
}
