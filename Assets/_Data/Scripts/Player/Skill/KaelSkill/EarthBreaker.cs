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
        if (character is not Kael kael)
        {
            Debug.LogError("EarthBreaker skill chỉ có thể được sử dụng bởi Kael");
            return;
        }

        //Gồng
        character.CharacterAnimation.AddEvent("Skill_1_1", 0.15f, () => ObjectPooling.Instance?.SpawnFromPool(
                                                             kael.auraEffect_1,
                                                             kael.middleEffectPoint.transform.position,
                                                             kael.middleEffectPoint.transform.rotation,
                                                              kael.middleEffectPoint.transform));
        character.CharacterAnimation.AddEvent("Skill_1_1", 0.15f, () => ObjectPooling.Instance?.SpawnFromPool(
                                                  kael.auraEffect_2,
                                                  kael.bottomEffectPoint.transform.position,
                                                  kael.bottomEffectPoint.transform.rotation,
                                                   kael.bottomEffectPoint.transform));
        character.CharacterAnimation.AddEvent("Skill_1_1", 0.3f, () => character.CharacterAnimation.CrossFade("Skill_1_2", 0.1f));

        //Nhảy lên và đập xuống
        character.CharacterAnimation.AddEvent("Skill_1_2", 0.4f, () => ObjectPooling.Instance?.SpawnFromPool(
                                                                                    kael.earthBreakerEffect,
                                                                                    kael.earthBreakerEffectPoint.transform.position,
                                                                                    kael.earthBreakerEffectPoint.transform.rotation));

        character.CharacterAnimation.AddEvent("Skill_1_2", 0.4f, () => CameraShake.Instance?.Shake());
        character.CharacterAnimation.AddEvent("Skill_1_2", 0.8f, () => character.StateController.ChangeState(new IdleState(character)));
    }

    protected override UniTask ExecuteSkill()
    {
        character.CharacterAnimation.CrossFade("Skill_1_1", 0.1f);
        return UniTask.CompletedTask;
    }
}
