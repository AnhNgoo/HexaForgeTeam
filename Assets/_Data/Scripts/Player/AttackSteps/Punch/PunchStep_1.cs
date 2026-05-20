using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;

public class PunchStep_1 : AttackStepBase
{
    public override string AttackStateName => "Punch_1";
    public override float TimeTriggerAttack => 0.3f;

    public override void StartAttack(CharacterBase character)
    {
        Debug.Log("PunchStep_1");
        character.CharacterAnimation.CrossFade(AttackStateName, 0.1f);
    }

    protected override void TriggerAttack(CharacterBase character)
    {
    }
}
