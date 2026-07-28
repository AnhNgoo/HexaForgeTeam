using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;
using Cysharp.Threading.Tasks;

public class PunchStep_3 : AttackStepBase
{
    public PunchStep_3(CharacterBase character) : base(character)
    {

    }

    public override string AttackStateName => "Punch_3";
    public override async void Attack()
    {

        character.CharacterAnimation.CrossFade(AttackStateName, 0.1f);
        await UniTask.WaitUntil(() => character.CharacterAnimation.GetAnimationTime(AttackStateName) > TimeTriggerAttack);

        character.CharacterMeleeHitbox.AttackHitBox(character.hitEffect_1);
        ObjectPooling.Instance.SpawnFromPool(character.punchEffect_3,
                                    character.punchEffectPoint_3.transform.position,
                                    character.punchEffectPoint_3.transform.rotation);
    }
}
