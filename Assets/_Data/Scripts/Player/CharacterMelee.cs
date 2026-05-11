using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class CharacterMelee : CharacterBase
{
    [SerializeField] protected float timeCanAttack = 0.7f;
    [SerializeField] protected float comboResetDelay = 1f;
    [ReadOnly] protected int currentComboIndex = 0;
    protected IAttackStep[] attackCombos;

    protected bool isComboWindowOpen = false;
    protected Coroutine comboCoroutine;

    protected override void Awake()
    {
        base.Awake();
        attackCombos = InitAttackCombos();
        if (attackCombos == null)
            Debug.LogWarning("Bạn chưa khởi tạo đòn tấn công nào cho character này! Hãy override InitAttackCombos() để khởi tạo.");
    }

    #region Init Attack Combos
    /// <summary>
    /// Khởi tạo các đòn tấn công, bắt buộc phải override
    /// </summary>
    /// <returns></returns>
    protected virtual IAttackStep[] InitAttackCombos()
    {
        return null;
    }

    #endregion

    #region Attack
    protected override void OnAttack()
    {
        if (!CheckConditionAttack())
            return;

        if (!CanAttack)
            return;

        if (!IsAttacking)
            stateController.ChangeState(new AttackState(this));

        CanAttack = false;
        if (isComboWindowOpen)
        {
            currentComboIndex++;
            if (currentComboIndex >= attackCombos.Length)
            {
                currentComboIndex = 0;
            }
        }
        else
        {
            currentComboIndex = 0;
        }

        if (comboCoroutine != null)
        {
            StopCoroutine(comboCoroutine);
            comboCoroutine = null;
        }

        comboCoroutine = StartCoroutine(AttackRoutine(currentComboIndex));
    }

    protected virtual IEnumerator AttackRoutine(int comboIndex)
    {
        if (attackCombos.Length == 0)
            yield break;

        // Đảm bảo comboIndex nằm trong phạm vi của attackCombos
        comboIndex = Mathf.Clamp(comboIndex, 0, attackCombos.Length - 1);
        IAttackStep attackStep = attackCombos[comboIndex];

        CanAttack = false;
        IsAttacking = true;
        isComboWindowOpen = false;

        //Thực hiện đòn tấn công
        attackStep.Attack(this);

        // Đợi cho đến khi animation bắt đầu
        yield return new WaitUntil(() =>
            !characterAnimation.Animator.IsInTransition(0) &&
            characterAnimation.Animator.GetCurrentAnimatorStateInfo(0).IsName(attackStep.AttackStateName));

        // Đợi cho đến khi animation hoàn thành
        yield return new WaitUntil(() =>
        {
            AnimatorStateInfo stateInfo = characterAnimation.Animator.GetCurrentAnimatorStateInfo(0);
            return stateInfo.IsName(attackStep.AttackStateName) &&
                   stateInfo.normalizedTime >= timeCanAttack &&
                   !characterAnimation.Animator.IsInTransition(0);
        });

        //Cho phép tấn công ở kiểu đánh tiếp theo
        CanAttack = true;
        isComboWindowOpen = true;

        //Nếu qua thời gian này thì phải đánh lại từ đầu
        yield return new WaitForSeconds(comboResetDelay);

        if (isComboWindowOpen)
            ResetCombo();
    }

    protected virtual void ResetCombo()
    {
        isComboWindowOpen = false;
        currentComboIndex = 0;
        IsAttacking = false;
        CanAttack = true;
        stateController.ChangeState(new IdleState(this));
    }

    #endregion
}
