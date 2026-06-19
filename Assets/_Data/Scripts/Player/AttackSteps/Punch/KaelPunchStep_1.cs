using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class KaelPunchStep_1 : AttackStepBase
{
    public KaelPunchStep_1(CharacterBase character) : base(character)
    {
    }

    public override string AttackStateName => "Punch_1";
    public float KaelGiantTimeTriggerAttack => 0.3f;

    public override async void Attack(CharacterBase character)
    {
        if (character is not Kael kael) return;

        if (kael.IsGiantForm)
        {
            character.CharacterAnimation.CrossFade(AttackStateName, 0.1f);
            await UniTask.WaitUntil(() => character.CharacterAnimation.GetAnimationTime(AttackStateName) > KaelGiantTimeTriggerAttack);

            character.CharacterCombat.AttackHitBox(kael.hitEffect_1);
            ObjectPooling.Instance.SpawnFromPool(kael.kaelGiantPunchEffect_1,
                                        kael.kaelGiantPunchEffectPoint_1.transform.position,
                                        kael.kaelGiantPunchEffectPoint_1.transform.rotation);
        }
        else
        {
            character.CharacterAnimation.CrossFade(AttackStateName, 0.1f);
            await UniTask.WaitUntil(() => character.CharacterAnimation.GetAnimationTime(AttackStateName) > TimeTriggerAttack);

            character.CharacterCombat.AttackHitBox(kael.hitEffect_1);
            ObjectPooling.Instance.SpawnFromPool(kael.punchEffect_1,
                                        kael.punchEffectPoint_1.transform.position,
                                        kael.punchEffectPoint_1.transform.rotation);
        }
    }
}
