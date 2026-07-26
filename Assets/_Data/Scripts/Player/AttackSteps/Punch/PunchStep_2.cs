using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;
using Cysharp.Threading.Tasks;

public class PunchStep_2 : AttackStepBase
{
    public PunchStep_2(CharacterBase character) : base(character)
    {

    }

    public override string AttackStateName => "Punch_2";
    public override async void Attack()
    {
        character.CharacterAnimation.CrossFade(AttackStateName, 0.1f);
        await UniTask.WaitUntil(() => character.CharacterAnimation.GetAnimationTime(AttackStateName) > TimeTriggerAttack);

        character.CharacterMeleeHitbox.AttackHitBox(character.hitEffect_1);
        ObjectPooling.Instance.SpawnFromPool(character.punchEffect_2,
                                    character.punchEffectPoint_2.transform.position,
                                    character.punchEffectPoint_2.transform.rotation);
    }
}
