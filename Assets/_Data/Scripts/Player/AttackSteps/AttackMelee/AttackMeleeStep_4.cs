using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class AttackMeleeStep_4 : AttackStepBase
{
    public AttackMeleeStep_4(CharacterBase character) : base(character)
    {
        if (character is CharacterMelee meleeCharacter)
        {
            meleeCharacter.CharacterAnimation.AddEvent(AttackStateName, TimeTriggerAttack, () => meleeCharacter.meleeAttackEffect_4.SetActive(true));
            meleeCharacter.CharacterAnimation.AddEvent(AttackStateName, TimeEndAttack, () => meleeCharacter.meleeAttackEffect_4.SetActive(false));
        }
    }

    public override string AttackStateName => "Attack_4";
    public override float TimeTriggerAttack => 0.4f;
    public override void Attack(CharacterBase character)
    {
        base.Attack(character);
    }
}
