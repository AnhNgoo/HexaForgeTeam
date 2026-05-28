using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;

public class PunchStep_2 : AttackStepBase
{
    public PunchStep_2(CharacterBase character) : base(character)
    {
        character.CharacterAnimation.AddEvent(AttackStateName, TimeTriggerAttack, () => ObjectPooling.Instance.SpawnFromPool(
                                                                                        character.punchEffect_2,
                                                                                        character.punchEffectPoint_2.transform.position,
                                                                                        character.punchEffectPoint_2.transform.rotation,
                                                                                         character.punchEffectPoint_2.transform));
    }

    public override string AttackStateName => "Punch_2";
    public override void Attack(CharacterBase character)
    {
        base.Attack(character);
    }
}
