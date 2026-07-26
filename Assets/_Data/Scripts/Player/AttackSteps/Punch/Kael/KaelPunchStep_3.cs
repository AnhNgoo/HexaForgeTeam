using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class KaelPunchStep_3 : AttackStepBase
{
    public KaelPunchStep_3(CharacterBase character) : base(character)
    {
    }

    public override string AttackStateName => "Punch_3";
    public float KaelGiantTimeTriggerAttack => 0.4f;

    public override async void Attack()
    {
        if (character is not Kael kael) return;

        if (kael.IsGiantForm)
        {
            character.CharacterAnimation.CrossFade(AttackStateName, 0.1f);
            await UniTask.WaitUntil(() => character.CharacterAnimation.GetAnimationTime(AttackStateName) > KaelGiantTimeTriggerAttack);


            character.CharacterMeleeHitbox.AttackHitBox(kael.hitEffect_1);
            ObjectPooling.Instance.SpawnFromPool(kael.kaelGiantPunchEffect_1,
                                        kael.kaelGiantPunchEffectPoint_3.transform.position,
                                        kael.kaelGiantPunchEffectPoint_3.transform.rotation);
        }
        else
        {
            character.CharacterAnimation.CrossFade(AttackStateName, 0.1f);
            await UniTask.WaitUntil(() => character.CharacterAnimation.GetAnimationTime(AttackStateName) > TimeTriggerAttack);


            character.CharacterMeleeHitbox.AttackHitBox(kael.hitEffect_1);
            ObjectPooling.Instance.SpawnFromPool(kael.punchEffect_3,
                                        kael.punchEffectPoint_3.transform.position,
                                        kael.punchEffectPoint_3.transform.rotation);
        }
    }
}
