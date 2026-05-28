using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CharacterAnimation : MonoBehaviour
{
    [SerializeField] private Animator animator;
    public Animator Animator => animator;
    [SerializeField] private AnimatorRootMotion animatorRootMotion;
    [SerializeField] protected AnimationEvents animationEvents;
    private string currentState;

    public void Init(GameObject visual)
    {
        if (visual.TryGetComponent<Animator>(out var anim))
        {
            SetAnimator(anim);
        }

        if (visual.TryGetComponent<AnimatorRootMotion>(out var arm))
        {
            SetAnimatorRootMotion(arm);
        }

        if (visual.TryGetComponent<AnimationEvents>(out var animEvents))
        {
            SetAnimationEvents(animEvents);
        }
    }
    private void SetAnimator(Animator animator)
    {
        this.animator = animator;
    }

    private void SetAnimatorRootMotion(AnimatorRootMotion arm)
    {
        this.animatorRootMotion = arm;
    }

    private void SetAnimationEvents(AnimationEvents animationEvents)
    {
        this.animationEvents = animationEvents;
    }

    public void EnableRootMotion()
    {
        animatorRootMotion?.EnableRootMotion();
    }
    public void DisableRootMotion()
    {
        animatorRootMotion?.DisableRootMotion();
    }
    /// <summary>
    /// Hàm này sẽ chuyển sang animation state mới mà không cần kiểm tra nếu đã ở state đó hay chưa. Sử dụng khi bạn muốn chắc chắn rằng animation sẽ được reset lại từ đầu mỗi khi gọi, ví dụ như khi nhân vật bị choáng hoặc bị ngắt quãng bởi một hiệu ứng nào đó.
    /// </summary>
    /// <param name="stateName"></param>
    /// <param name="transitionDuration"></param>
    public void CrossFade(string stateName, float transitionDuration = 0.1f, int layer = 0)
    {
        if (animator == null)
        {
            Debug.LogWarning("Animator chưa được gán cho CharacterAnimation!");
            return;
        }
        animator.CrossFade(stateName, transitionDuration, layer);
    }

    /// <summary>
    /// Sau khi gọi hàm này nhớ gọi hàm ResetState
    /// </summary>
    /// <param name="stateName"></param>
    /// <param name="transitionDuration"></param>
    public void CrossFadeOneshot(string stateName, float transitionDuration = 0.1f, int layer = 0)
    {
        if (currentState == stateName)
            return;

        animator.CrossFade(stateName, transitionDuration, layer);
        currentState = stateName;
    }

    public void SetFloat(string parameterName, float value)
    {
        if (animator == null)
        {
            Debug.LogWarning("Animator chưa được gán cho CharacterAnimation!");
            return;
        }

        animator.SetFloat(parameterName, value);
    }
    public void ResetState()
    {
        currentState = null;
    }

    public float GetAnimationTime(string stateName, int layer = 0)
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(layer);
        if (stateInfo.IsName(stateName))
        {
            return stateInfo.normalizedTime;
        }
        return 0f;
    }

    public void SetAnimationLayerWeight(string layerName, float weight)
    {
        int layerIndex = animator.GetLayerIndex(layerName);
        if (layerIndex != -1)
        {
            animator.SetLayerWeight(layerIndex, weight);
        }
        else
        {
            Debug.LogWarning($"Layer '{layerName}' không tồn tại trong Animator!");
        }
    }

    public int GetAnimationLayerWeight(string layerName)
    {
        int layerIndex = animator.GetLayerIndex(layerName);
        if (layerIndex != -1)
        {
            return layerIndex;
        }
        else
        {
            Debug.LogWarning($"Layer '{layerName}' không tồn tại trong Animator!");
            return 0;
        }
    }

    public void AddEvent(string animationClipName, float eventTime, UnityAction function)
    {
        if (animationEvents == null)
        {
            Debug.LogWarning("AnimationEvents chưa được gán cho CharacterAnimation!");
            return;
        }

        animationEvents.AddEvent(animationClipName, eventTime, function);
    }
    public void SetAnimationSpeed(string stateName, float speed)
    {
        animator.SetFloat(stateName, speed);
    }
}
