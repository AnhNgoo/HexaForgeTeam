using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class AttackMeleeStep_3 : AttackStepBase
{
    public override string AttackStateName => "Attack_3";
    public override float TimeTriggerAttack => 0.5f;

    public override void StartAttack(CharacterBase character)
    {
        character.CharacterAnimation.CrossFade(AttackStateName, 0.1f);
        if (character is CharacterMelee characterMelee)
            characterMelee.PlayTrailSlashEffect();
    }

    protected override void TriggerAttack(CharacterBase character)
    {
        if (character is CharacterMelee characterMelee)
            characterMelee.PlaySlashEffect(3);
    }
}
