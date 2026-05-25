using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class AttackMeleeStep_1 : AttackStepBase
{
    public AttackMeleeStep_1(CharacterBase character) : base(character)
    {
        if (character is CharacterMelee meleeCharacter)
        {
            meleeCharacter.CharacterAnimation.AddEvent(AttackStateName, TimeTriggerAttack, () => meleeCharacter.meleeAttackEffect_1.SetActive(true));
            meleeCharacter.CharacterAnimation.AddEvent(AttackStateName, TimeEndAttack, () => meleeCharacter.meleeAttackEffect_1.SetActive(false));
        }
    }

    public override string AttackStateName => "Attack_1";
    public override float TimeTriggerAttack => 0.4f;
    public override void Attack(CharacterBase character)
    {
        base.Attack(character);
    }
}
