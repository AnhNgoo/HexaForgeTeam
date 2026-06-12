using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class AttackMeleeStep_2 : AttackStepBase
{
    public AttackMeleeStep_2(CharacterBase character) : base(character)
    {
        if (character is CharacterMelee meleeCharacter)
        {
            meleeCharacter.CharacterAnimation.AddEvent(AttackStateName, TimeTriggerAttack, () => ObjectPooling.Instance?.SpawnFromPool(
                                                                                        meleeCharacter.meleeAttackEffect_2,
                                                                                        meleeCharacter.meleeAttackEffectPoint_2.transform.position,
                                                                                        meleeCharacter.meleeAttackEffectPoint_2.transform.rotation));
        }
    }

    public override string AttackStateName => "Attack_2";
    public override float TimeTriggerAttack => 0.2f;
    public override void Attack(CharacterBase character)
    {
        base.Attack(character);
    }
}
