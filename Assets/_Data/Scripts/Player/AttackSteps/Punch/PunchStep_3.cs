using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;

public class PunchStep_3 : AttackStepBase
{
    public override string AttackStateName => "Punch_3";
    public override float TimeTriggerAttack => 0.3f;

    public override void StartAttack(CharacterBase character)
    {
        character.CharacterAnimation.CrossFade(AttackStateName, 0.1f);
    }

    protected override void TriggerAttack(CharacterBase character)
    {
    }
}
