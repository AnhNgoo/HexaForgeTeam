using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;
using Cysharp.Threading.Tasks;

public class PunchStep_1 : AttackStepBase
{
    public PunchStep_1(CharacterBase character) : base(character)
    {
    }

    public override string AttackStateName => "Punch_1";
    public override async void Attack(CharacterBase character)
    {
        character.CharacterAnimation.CrossFade(AttackStateName, 0.1f);
        await UniTask.WaitUntil(() => character.CharacterAnimation.GetAnimationTime(AttackStateName) > TimeTriggerAttack);

        character.CharacterMeleeHitbox.AttackHitBox(character.hitEffect_1);
        ObjectPooling.Instance.SpawnFromPool(character.punchEffect_1,
                                    character.punchEffectPoint_1.transform.position,
                                    character.punchEffectPoint_1.transform.rotation);
    }
}
