using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class KaelAttackMeleeStep_2 : AttackStepBase
{
    public KaelAttackMeleeStep_2(CharacterBase character) : base(character)
    {

    }

    public override string AttackStateName => "Attack_2";
    public override float TimeTriggerAttack => 0.2f;
    public override async void Attack(CharacterBase character)
    {
        base.Attack(character);

        await UniTask.WaitUntil(() => character.CharacterAnimation.GetAnimationTime(AttackStateName) > TimeTriggerAttack);

        if (character is Kael kael)
        {
            kael.CharacterCombat.AttackHitBox(kael.hitEffect_2);
            ObjectPooling.Instance.SpawnFromPool(
                kael.meleeAttackEffect_2,
                kael.meleeAttackEffectPoint_2.transform.position,
                kael.meleeAttackEffectPoint_2.transform.rotation);
        }

    }
}
