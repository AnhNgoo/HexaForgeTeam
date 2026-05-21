using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
public abstract class AttackStepBase : IAttackStep
{
    public abstract string AttackStateName { get; }
    public virtual float TimeTriggerAttack { get; }

    /// <summary>
    /// Gọi ngay lúc chạy animation
    /// Nhớ phải gọi base.Attack(character) để chạy phần chung của tất cả các bước tấn công, nếu không sẽ bị lỗi animation và trigger attack
    /// </summary>
    /// <param name="character"></param>
    public virtual void Attack(CharacterBase character)
    {
        character.SetAttackSpeed(character.CharacterData.stats.attackSpeed);
        StartAttack(character);
        AwaitTriggerAttack(character);
    }

    public virtual async void AwaitTriggerAttack(CharacterBase character)
    {
        await UniTask.WaitUntil(() =>
            {
                return character.CharacterAnimation.GetAnimationTime(AttackStateName) >= TimeTriggerAttack &&
                       !character.CharacterAnimation.Animator.IsInTransition(0);
            });
        TriggerAttack(character);
    }

    /// <summary>
    /// Chay ngay lúc chạy animation, dùng để gọi các hiệu ứng chung của tất cả các bước tấn công, như hiệu ứng vệt kiếm, hiệu ứng âm thanh, v.v... Các bước tấn công cụ thể sẽ override để gọi hiệu ứng riêng của chúng
    /// </summary>
    /// <param name="character"></param>
    public abstract void StartAttack(CharacterBase character);

    /// <summary>
    /// Chạy khi animation đến đúng thời điểm trigger va chạm
    /// </summary>
    /// <param name="character"></param>
    protected abstract void TriggerAttack(CharacterBase character);
}
