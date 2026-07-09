using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;
using Cysharp.Threading.Tasks;

public class PunchStep_4 : AttackStepBase
{
    public PunchStep_4(CharacterBase character) : base(character)
    {

    }

    public override string AttackStateName => "Punch_4";
    public override float TimeTriggerAttack => 0.4f;
    public override async void Attack(CharacterBase character)
    {
        character.CharacterAnimation.CrossFade(AttackStateName, 0.1f);
        await UniTask.WaitUntil(() => character.CharacterAnimation.GetAnimationTime(AttackStateName) > TimeTriggerAttack);

        character.CharacterMeleeHitbox.AttackHitBox(character.hitEffect_1);
        ObjectPooling.Instance.SpawnFromPool(character.punchEffect_4,
                                    character.punchEffectPoint_4.transform.position,
                                    character.punchEffectPoint_4.transform.rotation);
    }
}
