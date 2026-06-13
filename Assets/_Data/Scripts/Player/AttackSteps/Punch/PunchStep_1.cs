using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;

public class PunchStep_1 : AttackStepBase
{
    public PunchStep_1(CharacterBase character) : base(character)
    {
        character.CharacterAnimation.AddEvent(AttackStateName, TimeTriggerAttack, () => ObjectPooling.Instance?.SpawnFromPool(
                                                                                        character.punchEffect_1,
                                                                                         character.punchEffectPoint_1.transform.position,
                                                                                         character.punchEffectPoint_1.transform.rotation,
                                                                                          character.punchEffectPoint_1.transform));
    }

    public override string AttackStateName => "Punch_1";
    public override void Attack(CharacterBase character)
    {
        base.Attack(character);
    }
}
