using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;

public class PunchStep_2 : AttackStepBase
{
    public PunchStep_2(CharacterBase character) : base(character)
    {
        character.CharacterAnimation.AddEvent(AttackStateName, TimeTriggerAttack, () => character.punchEffect_2.SetActive(true));
        character.CharacterAnimation.AddEvent(AttackStateName, TimeEndAttack, () => character.punchEffect_2.SetActive(false));
    }

    public override string AttackStateName => "Punch_2";
    public override void Attack(CharacterBase character)
    {
        base.Attack(character);
    }
}
