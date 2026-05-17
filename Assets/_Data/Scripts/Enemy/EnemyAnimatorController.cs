using UnityEngine;

public class EnemyAnimatorController : MonoBehaviour
{
    private EnemyBase _enemyBase;
    [SerializeField] private Animator _animator;
    public Animator Animator => _animator;

    public void Initialize(EnemyBase enemyBase)
    {
        _enemyBase = enemyBase;
        _animator = GetComponent<Animator>();
        Debug.Log($"{gameObject.name} - EnemyAnimatorController đã được khởi tạo!");
    }

    public void OnValidate()
    {
        if (_animator == null)
        {
            _animator = GetComponentInChildren<Animator>();
            if (_animator == null)
            {
                Debug.LogError("Animator component is missing on " + gameObject.name);
            }
        }
    }

    public void PlayAttackAnimation(AttackDataSO attackData)
    {
        if (_animator != null && attackData != null)
        {
            _animator.CrossFadeInFixedTime(attackData.animationStateName, attackData.transitionDuration);
        }
    }

    public void OpenHitBox()
    {
        Debug.Log($"{gameObject.name} đã mở hitbox!");
    }

    public void CloseHitBox()
    {
        Debug.Log($"{gameObject.name} đã đóng hitbox!");
    }
}
