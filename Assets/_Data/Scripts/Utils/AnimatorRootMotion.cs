using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorRootMotion : LoadComponents
{
    [SerializeField] private bool ApplyRootMotion = true;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Animator animator;

    protected override void LoadComponent()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
        if (characterController == null)
            characterController = GetComponentInParent<CharacterController>();
    }

    protected override void LoadComponentRuntime()
    {

    }

    void OnAnimatorMove()
    {
        if (!ApplyRootMotion || animator == null || characterController == null) return;

        // Lấy delta và áp dụng trực tiếp vào cha (transform.parent)
        characterController.Move(animator.deltaPosition);
        transform.parent.rotation *= animator.deltaRotation;

        // QUAN TRỌNG: Reset local position của model về 0 để tránh bị lệch xa dần theo thời gian
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    public void EnableRootMotion()
    {
        ApplyRootMotion = true;
    }

    public void DisableRootMotion()
    {
        ApplyRootMotion = false;
    }
}