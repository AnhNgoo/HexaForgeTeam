using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class EarthBreaker : CharacterSkillBase
{
    //Battlecry -> JumpAttack -> Idle
    //Battlecry: là animation gồng
    //JumpAttack: là animation nhảy lên và đập xuống
    public EarthBreaker(CharacterBase character, CharacterSkillData skillData) : base(character, skillData)
    {

    }

    protected override async UniTask ExecuteSkill()
    {
        if (character is not Kael kael)
        {
            Debug.LogError("EarthBreaker skill chỉ có thể được sử dụng bởi Kael");
            return;
        }
        character.CharacterAnimation.CrossFade("Skill_1_1", 0.1f);

        await UniTask.WaitUntil(() => character.CharacterAnimation.GetAnimationTime("Skill_1_1") > 0.15f);
        ObjectPooling.Instance.SpawnFromPool(kael.auraEffect_1,
                                            kael.middleEffectPoint.transform.position,
                                            kael.middleEffectPoint.transform.rotation,
                                            kael.middleEffectPoint.transform);

        ObjectPooling.Instance.SpawnFromPool(
                          kael.auraEffect_2,
                          kael.bottomEffectPoint.transform.position,
                          kael.bottomEffectPoint.transform.rotation,
                           kael.bottomEffectPoint.transform);

        await UniTask.WaitUntil(() => character.CharacterAnimation.GetAnimationTime("Skill_1_1") > 0.15f);

        character.CharacterAnimation.CrossFade("Skill_1_2", 0.1f);

        await UniTask.WaitUntil(() => character.CharacterAnimation.GetAnimationTime("Skill_1_2") > 0.4f);

        // Tạo hiệu ứng đập đất tại vị trí mục tiêu
        GameObject earthBreakerEffect = ObjectPooling.Instance.SpawnFromPool(
                                     kael.earthBreakerEffect,
                                     kael.earthBreakerEffectPoint.transform.position,
                                     kael.earthBreakerEffectPoint.transform.rotation);

        EarthBreakerSkill earthBreakerSkill = earthBreakerEffect?.GetComponent<EarthBreakerSkill>();
        if (earthBreakerSkill != null)
        {
            earthBreakerSkill.Init(character.CharacterData.stats.damage, character.CharacterData.stats.poisonDamage);
        }

        CameraShake.Instance.Shake();

        await UniTask.WaitUntil(() => character.CharacterAnimation.GetAnimationTime("Skill_1_2") > 0.6f);

        character.StateController.ChangeState(new IdleState(character));
    }
}
