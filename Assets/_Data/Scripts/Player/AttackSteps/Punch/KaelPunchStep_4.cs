using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class KaelPunchStep_4 : AttackStepBase
{
    public KaelPunchStep_4(CharacterBase character) : base(character)
    {
    }

    public override string AttackStateName => "Punch_4";
    public float KaelGiantTimeTriggerAttack => 0.6f;

    public override async void Attack(CharacterBase character)
    {
        if (character is not Kael kael) return;

        if (kael.IsGiantForm)
        {
            character.SetAttackSpeed(character.CharacterData.stats.attackSpeed);
            character.CharacterAnimation.CrossFade(AttackStateName, 0.1f);
            await UniTask.WaitUntil(() => character.CharacterAnimation.GetAnimationTime(AttackStateName) > KaelGiantTimeTriggerAttack);

            character.CharacterCombat.AttackHitBox();
            ObjectPooling.Instance.SpawnFromPool(kael.kaelGiantPunchEffect_2,
                                        kael.kaelGiantPunchEffectPoint_4.transform.position,
                                        kael.kaelGiantPunchEffectPoint_4.transform.rotation);
        }
        else
        {
            character.SetAttackSpeed(character.CharacterData.stats.attackSpeed);
            character.CharacterAnimation.CrossFade(AttackStateName, 0.1f);
            await UniTask.WaitUntil(() => character.CharacterAnimation.GetAnimationTime(AttackStateName) > TimeTriggerAttack);

            character.CharacterCombat.AttackHitBox();
            ObjectPooling.Instance.SpawnFromPool(kael.punchEffect_4,
                                        kael.punchEffectPoint_4.transform.position,
                                        kael.punchEffectPoint_4.transform.rotation,
                                        kael.punchEffectPoint_4.transform);
        }
    }
}
