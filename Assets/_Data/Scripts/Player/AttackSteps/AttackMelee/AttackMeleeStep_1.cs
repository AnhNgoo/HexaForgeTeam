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
            meleeCharacter.CharacterAnimation.AddEvent(AttackStateName, TimeTriggerAttack, () => ObjectPooling.Instance.SpawnFromPool(
                                                                                        meleeCharacter.meleeAttackEffect_1,
                                                                                        meleeCharacter.meleeAttackEffectPoint_1.transform.position,
                                                                                        meleeCharacter.meleeAttackEffectPoint_1.transform.rotation,
                                                                                         meleeCharacter.meleeAttackEffectPoint_1.transform));

        }
    }

    public override string AttackStateName => "Attack_1";
    public override float TimeTriggerAttack => 0.4f;
    public override void Attack(CharacterBase character)
    {
        base.Attack(character);
    }
}
