using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public abstract class AttackStepBase : IAttackStep
{
    public abstract string AttackStateName { get; }
    public virtual float TimeTriggerAttack { get; } = 0.2f;
    public virtual float TimeEndAttack { get; } = 0.7f;

    public AttackStepBase(CharacterBase character)
    {
    }

    /// <summary>
    /// Gọi ngay lúc chạy animation
    /// Nhớ phải gọi base.Attack(character) để chạy phần chung của tất cả các bước tấn công, nếu không sẽ bị lỗi animation và trigger attack
    /// </summary>
    /// <param name="character"></param>
    public abstract void Attack(CharacterBase character);
}
