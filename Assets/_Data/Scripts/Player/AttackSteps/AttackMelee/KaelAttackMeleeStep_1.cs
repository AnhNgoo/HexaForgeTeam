using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class KaelAttackMeleeStep_1 : AttackStepBase
{
    public KaelAttackMeleeStep_1(CharacterBase character) : base(character)
    {
    }

    public override string AttackStateName => "Attack_1";
    public override float TimeTriggerAttack => 0.4f;
    public override async void Attack(CharacterBase character)
    {
        base.Attack(character);

        await UniTask.WaitUntil(() => character.CharacterAnimation.GetAnimationTime(AttackStateName) > TimeTriggerAttack);

        if (character is Kael kael)
        {
            kael.CharacterCombat.AttackHitBox(kael.hitEffect_2);
            ObjectPooling.Instance.SpawnFromPool(
                kael.meleeAttackEffect_1,
                kael.meleeAttackEffectPoint_1.transform.position,
                kael.meleeAttackEffectPoint_1.transform.rotation);
        }

    }
}
