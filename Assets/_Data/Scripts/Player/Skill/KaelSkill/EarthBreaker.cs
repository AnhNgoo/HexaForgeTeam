using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class EarthBreaker : CharacterSkillBase
{
    private HitPauseEffect hitPauseEffect = new HitPauseEffect();
    //Battlecry -> JumpAttack -> Idle
    //Battlecry: là animation gồng
    //JumpAttack: là animation nhảy lên và đập xuống

    public EarthBreaker(CharacterBase character, CharacterSkillData skillData) : base(character, skillData)
    {

    }

    protected override async void ExecuteSkill()
    {
        if (character is not Kael kael)
        {
            Debug.LogError("EarthBreaker skill chỉ có thể được sử dụng bởi Kael");
            return;
        }
        character.CharacterSkill.CanUseSkill1 = false;
        character.CharacterSkill.CanUseSkill2 = false;
        character.CanBeAttacked = false;

        EventManager.Notify(GameEvent.OnUpdateCooldownSkill1, skillData.skillStats.cooldown);

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
            float damageSkill = character.CharacterStat.GetSkillDamage(skillData);
            float poisonDamage = skillData.skillStats.poisonDamage;
            earthBreakerSkill.Init(damageSkill, poisonDamage);
        }

        CameraShake.Instance.Shake();

        await UniTask.WaitUntil(() => character.CharacterAnimation.GetAnimationTime("Skill_1_2") > 0.6f); // Đợi animation đập đất hoàn thành

        // hitPauseEffect.PlayHitPause(1.3f, 0.1f); // Tạm dừng thời gian khi đòn tấn công trúng mục tiêu

        // await UniTask.WaitUntil(() => character.CharacterAnimation.GetAnimationTime("Skill_1_2") > 0.9f);
        character.StateController.ChangeState(new IdleState(character));

        character.CanBeAttacked = true;
        character.CharacterSkill.CanUseSkill1 = true;
        character.CharacterSkill.CanUseSkill2 = true;
        character.CharacterSkill.IsUsingSkill1 = false;
    }
}
