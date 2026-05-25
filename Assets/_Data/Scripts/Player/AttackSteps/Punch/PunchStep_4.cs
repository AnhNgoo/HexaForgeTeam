using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;

public class PunchStep_4 : AttackStepBase
{
    public PunchStep_4(CharacterBase character) : base(character)
    {
        character.CharacterAnimation.AddEvent(AttackStateName, TimeTriggerAttack, () => character.punchEffect_4.SetActive(true));
        character.CharacterAnimation.AddEvent(AttackStateName, TimeEndAttack, () => character.punchEffect_4.SetActive(false));
    }

    public override string AttackStateName => "Punch_4";
    public override float TimeTriggerAttack => 0.4f;
    public override void Attack(CharacterBase character)
    {
        base.Attack(character);
    }
}
