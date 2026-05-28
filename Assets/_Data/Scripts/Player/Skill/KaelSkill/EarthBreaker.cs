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
        if (character is Kael kael)
        {
            character.CharacterAnimation.AddEvent("JumpAttack", 0.4f, () => ObjectPooling.Instance.SpawnFromPool(
                                                                                        PoolType.EarthBreaker_2,
                                                                                        kael.earthBreakerEffectPoint.transform.position,
                                                                                        kael.earthBreakerEffectPoint.transform.rotation));

            character.CharacterAnimation.AddEvent("Battlecry", 0.15f, () => ObjectPooling.Instance.SpawnFromPool(
                                                                    PoolType.AuraEffect_1,
                                                                    kael.auraEffect.transform.position,
                                                                    kael.auraEffect.transform.rotation,
                                                                     kael.auraEffect.transform));
            character.CharacterAnimation.AddEvent("Battlecry", 0.15f, () => ObjectPooling.Instance.SpawnFromPool(
                                                      PoolType.AuraEffect_2,
                                                      kael.auraEffect.transform.position,
                                                      kael.auraEffect.transform.rotation,
                                                       kael.auraEffect.transform));
        }
        character.CharacterAnimation.AddEvent("Battlecry", 0.3f, () => character.CharacterAnimation.CrossFade("JumpAttack", 0.1f));

        character.CharacterAnimation.AddEvent("JumpAttack", 0.8f, () => character.StateController.ChangeState(new IdleState(character)));
        character.CharacterAnimation.AddEvent("JumpAttack", 0.4f, () => CameraShake.Instance.Shake());
    }

    protected override UniTask ExecuteSkill()
    {
        character.CharacterAnimation.CrossFade("Battlecry", 0.1f);
        return UniTask.CompletedTask;
    }
}
