using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class KaelAttacStep_1 : AttackStepBase
{
    public KaelAttacStep_1(CharacterBase character) : base(character)
    {
    }

    public override string AttackStateName => "MeleeAttack_1";
    public override float TimeTriggerAttack => 0.4f;
    public override async void Attack(CharacterBase character)
    {
        character.CharacterAnimation.CrossFade(AttackStateName, 0.1f);

        await UniTask.WaitUntil(() => character.CharacterAnimation.GetAnimationTime(AttackStateName) > TimeTriggerAttack);

        if (character is Kael kael)
        {
            kael.CharacterMeleeHitbox.AttackHitBox(kael.hitEffect_2);
            ObjectPooling.Instance.SpawnFromPool(
                kael.meleeAttackEffect_1,
                kael.meleeAttackEffectPoint_1.transform.position,
                kael.meleeAttackEffectPoint_1.transform.rotation);
        }

    }
}
