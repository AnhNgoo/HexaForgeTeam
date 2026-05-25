using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

public class AnimationEvents : LoadComponents
{
    [SerializeField] private Animator animator;
    private readonly List<UnityAction> registeredActions = new();

    protected override void LoadComponent()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    protected override void LoadComponentRuntime()
    {

    }
    public void AddEvent(string animationClipName, float eventTime, UnityAction function)
    {
        AnimationClip clip = GetAnimationClip(animationClipName);
        if (clip == null)
            return;

        int actionIndex = registeredActions.Count;
        registeredActions.Add(function);

        AnimationEvent animationEvent = new AnimationEvent
        {
            time = Mathf.Clamp01(eventTime) * clip.length,
            functionName = nameof(InvokeRegisteredAction),
            intParameter = actionIndex
        };

        clip.AddEvent(animationEvent);
    }

    public void InvokeRegisteredAction(int actionIndex)
    {
        if (actionIndex < 0 || actionIndex >= registeredActions.Count)
        {
            Debug.LogWarning("Animation event action index invalid: " + actionIndex, this);
            return;
        }

        registeredActions[actionIndex]?.Invoke();
    }
    private AnimationClip GetAnimationClip(string clipName)
    {
        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == clipName)
                return clip;
        }
        Debug.LogWarning("Animation clip not found: " + clipName);
        return null;
    }
}
