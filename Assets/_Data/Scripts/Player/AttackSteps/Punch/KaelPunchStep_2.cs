using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
public class KaelPunchStep_2 : AttackStepBase
{
    public KaelPunchStep_2(CharacterBase character) : base(character)
    {
    }

    public override string AttackStateName => "Punch_2";
    public float KaelGiantTimeTriggerAttack => 0.3f;

    public override async void Attack(CharacterBase character)
    {
        if (character is not Kael kael) return;

        Debug.Log("KaelPunchStep_2 Attack");
        if (kael.IsGiantForm)
        {
            character.SetAttackSpeed(character.CharacterData.stats.attackSpeed);
            character.CharacterAnimation.CrossFade(AttackStateName, 0.1f);
            await UniTask.WaitUntil(() => character.CharacterAnimation.GetAnimationTime(AttackStateName) > KaelGiantTimeTriggerAttack);


            character.CharacterCombat.AttackHitBox(kael.hitEffect_1);
            ObjectPooling.Instance.SpawnFromPool(kael.kaelGiantPunchEffect_1,
                                        kael.kaelGiantPunchEffectPoint_2.transform.position,
                                        kael.kaelGiantPunchEffectPoint_2.transform.rotation);
        }
        else
        {
            character.SetAttackSpeed(character.CharacterData.stats.attackSpeed);
            character.CharacterAnimation.CrossFade(AttackStateName, 0.1f);
            await UniTask.WaitUntil(() => character.CharacterAnimation.GetAnimationTime(AttackStateName) > TimeTriggerAttack);

            character.CharacterCombat.AttackHitBox(kael.hitEffect_1);
            ObjectPooling.Instance.SpawnFromPool(kael.punchEffect_2,
                                        kael.punchEffectPoint_2.transform.position,
                                        kael.punchEffectPoint_2.transform.rotation);
        }
    }
}
