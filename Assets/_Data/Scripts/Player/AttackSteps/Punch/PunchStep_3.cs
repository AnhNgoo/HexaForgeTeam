using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;

public class PunchStep_3 : AttackStepBase
{
    public PunchStep_3(CharacterBase character) : base(character)
    {
        character.CharacterAnimation.AddEvent(AttackStateName, TimeTriggerAttack, () => ObjectPooling.Instance.SpawnFromPool(
                                                                                         character.punchEffect_3,
                                                                                         character.punchEffectPoint_3.transform.position,
                                                                                         character.punchEffectPoint_3.transform.rotation,
                                                                                          character.punchEffectPoint_3.transform));
    }

    public override string AttackStateName => "Punch_3";
    public override void Attack(CharacterBase character)
    {
        base.Attack(character);
    }
}
