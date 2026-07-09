using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lyra : CharacterRanged
{
    public AudioSource projectileAudioSource;
    [Header("Effects")]
    public PoolType hitEffect = PoolType.HitEffect_3;
    public PoolType arcaneChargeEffect = PoolType.ArcaneChargeEffect;
    public PoolType lyraProjectile = PoolType.LyraProjectile;
    protected override IAttackStep[] InitAttackCombos()
    {
        return new IAttackStep[]
        {
            new LyraAttackStep_1(this),
        };
    }
}
