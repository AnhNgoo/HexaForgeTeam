using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class KaelAttackStep_2 : AttackStepBase
{
    public KaelAttackStep_2(CharacterBase character) : base(character)
    {

    }

    public override string AttackStateName => "MeleeAttack_2";
    public override float TimeTriggerAttack => 0.2f;
    public override async void Attack()
    {
        character.CharacterAnimation.CrossFade(AttackStateName, 0.1f);

        await UniTask.WaitUntil(() => character.CharacterAnimation.GetAnimationTime(AttackStateName) > TimeTriggerAttack);

        if (character is Kael kael)
        {
            kael.CharacterMeleeHitbox.AttackHitBox(kael.hitEffect_2);
            ObjectPooling.Instance.SpawnFromPool(
                kael.meleeAttackEffect_2,
                kael.meleeAttackEffectPoint_2.transform.position,
                kael.meleeAttackEffectPoint_2.transform.rotation);
        }

    }
}
