using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class KaelAttackMeleeStep_4 : AttackStepBase
{
    public KaelAttackMeleeStep_4(CharacterBase character) : base(character)
    {

    }

    public override string AttackStateName => "Attack_4";
    public override float TimeTriggerAttack => 0.7f;
    public override async void Attack(CharacterBase character)
    {
        base.Attack(character);

        await UniTask.WaitUntil(() => character.CharacterAnimation.GetAnimationTime(AttackStateName) > TimeTriggerAttack);

        if (character is Kael kael)
        {
            kael.CharacterCombat.AttackHitBox(kael.hitEffect_2);
            ObjectPooling.Instance.SpawnFromPool(
                kael.meleeAttackEffect_4,
                kael.meleeAttackEffectPoint_4.transform.position,
                kael.meleeAttackEffectPoint_4.transform.rotation);
        }

    }
}
