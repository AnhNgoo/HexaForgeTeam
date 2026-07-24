using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class ChainBolt : CharacterSkillBase
{
    public ChainBolt(CharacterBase character, CharacterSkillData skillData) : base(character, skillData)
    {
    }

    protected override async void ExecuteSkill()
    {
        if (skillData == null)
        {
            Debug.LogError("Đang thiếu data, hãy thêm vào trong CharacterSkill");
            return;
        }
        if (character is not Lyra lyra)
        {
            Debug.LogError("MysticOrbs skill chỉ có thể được sử dụng bởi Lyra");
            return;
        }
        character.ConsumeSkillCost(character.CharacterData.characterTypes, skillData.skillCost);
        character.CharacterAnimation.CrossFade("Skill_1_1", 0.1f);

        GameObject arcaneChargeObj = ObjectPooling.Instance.SpawnFromPool(lyra.arcaneChargeEffect,
                                   lyra.fireEffectPoint.transform.position,
                                   lyra.fireEffectPoint.transform.rotation,
                                   lyra.fireEffectPoint.transform);

        await UniTask.WaitUntil(() => character.CharacterAnimation.GetAnimationTime("Skill_1_1") > 0.3f);

        CameraShake.Instance.Shake();
        lyra.projectileAudioSource.Play();

        ObjectPooling.Instance.ReturnToPool(lyra.arcaneChargeEffect, arcaneChargeObj);

        CreateProjectile(lyra.lyraSkill_1_Projectile, lyra.hitEffect);

        character.StateController.ChangeState(new IdleState(character));
    }

    public void CreateProjectile(PoolType characterProjectile, PoolType hitEffect = PoolType.None)
    {
        if (character is not Lyra lyra)
        {
            Debug.LogError("ChainBolt skill chỉ có thể được sử dụng bởi Lyra");
            return;
        }
        GameObject projectileObj = ObjectPooling.Instance.SpawnFromPool(characterProjectile,
                                            lyra.fireEffectPoint.transform.position,
                                             lyra.fireEffectPoint.transform.rotation);

        if (projectileObj.TryGetComponent(out LyraSkill_1_Projectile projectile))
        {
            Vector3 direction = lyra.GetDirectionToTarget();
            projectile.Init(direction, character, hitEffect);
        }
    }
}
