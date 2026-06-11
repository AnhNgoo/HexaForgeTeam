using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class AttackMeleeStep_3 : AttackStepBase
{
    public AttackMeleeStep_3(CharacterBase character) : base(character)
    {
        if (character is CharacterMelee meleeCharacter)
        {
            meleeCharacter.CharacterAnimation.AddEvent(AttackStateName, TimeTriggerAttack, () => ObjectPooling.Instance?.SpawnFromPool(
                                                                                        meleeCharacter.meleeAttackEffect_3,
                                                                                        meleeCharacter.meleeAttackEffectPoint_3.transform.position,
                                                                                        meleeCharacter.meleeAttackEffectPoint_3.transform.rotation));
        }
    }

    public override string AttackStateName => "Attack_3";
    public override float TimeTriggerAttack => 0.4f;
    public override void Attack(CharacterBase character)
    {
        base.Attack(character);
    }
}
