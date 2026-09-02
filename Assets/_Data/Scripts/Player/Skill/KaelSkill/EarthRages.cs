using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class EarthRages : CharacterSkillBase
{
    private GameObject kaelGiantAuraEffect_1;
    public EarthRages(CharacterBase character, CharacterSkillData skillData) : base(character, skillData)
    {

    }

    protected override async void ExecuteSkill()
    {
        if (character is not Kael kael)
        {
            Debug.LogError("EarthRages skill chỉ có thể được sử dụng bởi Kael");
            return;
        }
        character.CanBeAttacked = false;
        character.CharacterSkill.CanUseSkill1 = false;
        character.CharacterSkill.CanUseSkill2 = false;
        EventManager.Notify(GameEvent.OnUpdateCooldownSkill2, skillData.skillStats.cooldown);

        character.CharacterAnimation.CrossFade("Skill_2_1", 0.1f);
        ObjectPooling.Instance?.SpawnFromPool(kael.auraEffect_3,
                                            kael.bottomEffectPoint.transform.position,
                                            kael.bottomEffectPoint.transform.rotation,
                                            kael.bottomEffectPoint.transform);

        // Đợi 0.2s để biến thành dạng khổng lồ
        await UniTask.WaitUntil(() => character.CharacterAnimation.GetAnimationTime("Skill_2_1") > 0.15f &&
                  !character.CharacterAnimation.Animator.IsInTransition(0));

        kael.GiantForm();
        character.CharacterAnimation.CrossFade("Skill_2_1", 0.1f, 0, 0.15f);
        kaelGiantAuraEffect_1 = ObjectPooling.Instance?.SpawnFromPool(kael.kaelGiantAuraEffect_1,
                                            kael.bottomEffectPoint.transform.position,
                                            kael.bottomEffectPoint.transform.rotation,
                                            kael.bottomEffectPoint.transform);
        ObjectPooling.Instance?.SpawnFromPool(kael.auraEffect_4,
                                       kael.middleEffectPoint.transform.position,
                                       kael.middleEffectPoint.transform.rotation,
                                       kael.middleEffectPoint.transform);
        ObjectPooling.Instance?.SpawnFromPool(kael.auraEffect_5,
       kael.bottomEffectPoint.transform.position,
       kael.bottomEffectPoint.transform.rotation,
       kael.bottomEffectPoint.transform);

        //Đợi hoàn thành animation rồi chuyển về IdleState
        await UniTask.WaitUntil(() => character.CharacterAnimation.GetAnimationTime("Skill_2_1") > 0.3f &&
                 !character.CharacterAnimation.Animator.IsInTransition(0));

        character.StateController.ChangeState(new IdleState(character));

        CountdownToNormalForm();
        character.CanBeAttacked = true;
        character.CharacterCinematic.StopCinematic();
    }

    //Hàm đếm ngược thời gian trở lại hình dạng bình thường sau khi dùng skill
    private async void CountdownToNormalForm()
    {
        float duration = skillData.skillStats.duration;
        while (duration > 0)
        {
            duration -= Time.deltaTime;
            if (character.CharacterHealth.CurrentHealth <= 0)
                break;
            await UniTask.Yield();
        }
        if (character is not Kael kael) return;


        character.CanBeAttacked = false;
        character.StateController.ChangeState(new CombatState(character));
        character.CharacterAnimation.CrossFade("Skill_2_1", 0.1f);
        ObjectPooling.Instance?.SpawnFromPool(kael.auraEffect_3,
                                            kael.bottomEffectPoint.transform.position,
                                            kael.bottomEffectPoint.transform.rotation,
                                            kael.bottomEffectPoint.transform);

        // Đợi 0.2s để biến thành dạng thường
        await UniTask.WaitUntil(() => character.CharacterAnimation.GetAnimationTime("Skill_2_1") > 0.15f &&
                  !character.CharacterAnimation.Animator.IsInTransition(0));

        Debug.Log("Hết thời gian biến hình, trở về hình dạng bình thường");
        kael.NormalForm();
        character.CharacterAnimation.CrossFade("Skill_2_1", 0.1f, 0, 0.15f);
        ObjectPooling.Instance?.ReturnToPool(kael.kaelGiantAuraEffect_1, kaelGiantAuraEffect_1);
        ObjectPooling.Instance?.SpawnFromPool(kael.auraEffect_4,
                                       kael.middleEffectPoint.transform.position,
                                       kael.middleEffectPoint.transform.rotation,
                                       kael.middleEffectPoint.transform);
        ObjectPooling.Instance?.SpawnFromPool(kael.auraEffect_5,
       kael.bottomEffectPoint.transform.position,
       kael.bottomEffectPoint.transform.rotation,
       kael.bottomEffectPoint.transform);

        //Đợi hoàn thành animation rồi chuyển về IdleState
        await UniTask.WaitUntil(() => character.CharacterAnimation.GetAnimationTime("Skill_2_1") > 0.3f &&
                 !character.CharacterAnimation.Animator.IsInTransition(0));

        character.CharacterSkill.CanUseSkill1 = true;
        character.CharacterSkill.CanUseSkill2 = true;
        character.CharacterSkill.IsUsingSkill2 = false;
        character.StateController.ChangeState(new IdleState(character));
        character.CanBeAttacked = true;
    }
}
