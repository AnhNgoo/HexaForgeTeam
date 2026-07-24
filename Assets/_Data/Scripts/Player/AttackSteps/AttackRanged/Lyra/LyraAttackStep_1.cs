using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class LyraAttackStep_1 : AttackStepBase
{
    public LyraAttackStep_1(CharacterBase character) : base(character)
    {
    }

    public override string AttackStateName => "RangedAttack_1";
    public override float TimeTriggerAttack => 0.6f;
    private float timeTriggerArcaneChargeEffect = 0.1f;

    public override async void Attack()
    {
        character.CharacterAnimation.CrossFade(AttackStateName, 0.1f, 1);
        if (character is not Lyra lyra)
            return;

        await UniTask.WaitUntil(() => character.CharacterAnimation.GetAnimationTime(AttackStateName, 1) > timeTriggerArcaneChargeEffect);

        GameObject arcaneChargeObj = ObjectPooling.Instance.SpawnFromPool(lyra.arcaneChargeEffect,
                                   lyra.fireEffectPoint.transform.position,
                                   lyra.fireEffectPoint.transform.rotation,
                                   lyra.fireEffectPoint.transform);

        await UniTask.WaitUntil(() => character.CharacterAnimation.GetAnimationTime(AttackStateName, 1) > TimeTriggerAttack);

        CameraShake.Instance.Shake();
        lyra.projectileAudioSource.Play();

        ObjectPooling.Instance.ReturnToPool(lyra.arcaneChargeEffect, arcaneChargeObj);
        lyra.CreateProjectile(lyra.lyraProjectile, lyra.hitEffect);
    }
}
