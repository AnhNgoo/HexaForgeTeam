using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;

public class PunchStep_3 : AttackStepBase
{
    public PunchStep_3(CharacterBase character) : base(character)
    {
        character.CharacterAnimation.AddEvent(AttackStateName, TimeTriggerAttack, () => character.punchEffect_3.SetActive(true));
        character.CharacterAnimation.AddEvent(AttackStateName, TimeEndAttack, () => character.punchEffect_3.SetActive(false));
    }

    public override string AttackStateName => "Punch_3";
    public override void Attack(CharacterBase character)
    {
        base.Attack(character);
    }
}
