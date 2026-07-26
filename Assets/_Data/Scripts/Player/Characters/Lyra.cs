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
    public PoolType lyraSkill_1_Projectile = PoolType.LyraSkill_1_Projectile;
    public PoolType lyraAuraSkill_2_1 = PoolType.LyraAuraSkill_2_1;
    public PoolType lyraAuraSkill_2_2 = PoolType.LyraAuraSkill_2_2;
    public PoolType lyraAuraSkill_2_3 = PoolType.LyraAuraSkill_2_3;
    public PoolType lyraSkill_2_DetectionAreaEffect = PoolType.LyraSkill_2_DetectionAreaEffect;
    public PoolType lyraSkill_2_Projectile = PoolType.LyraSkill_2_Projectile;
    public PoolType lyraSkill_2_HitEffect = PoolType.LyraSkill_2_HitEffect;
    protected override IAttackStep[] InitAttackCombos()
    {
        return new IAttackStep[]
        {
            new LyraAttackStep_1(this),
        };
    }

    #region Override kỹ năng
    protected override ICharacterSkill GetSkill_1(CharacterSkillData skill1Data)
    {
        if (skill1Data == null)
            Debug.LogError("Đang thiếu data, hãy thêm vào trong CharacterSkill");

        return new ChainBolt(this, skill1Data);
    }

    protected override ICharacterSkill GetSkill_2(CharacterSkillData skill2Data)
    {
        if (skill2Data == null)
            Debug.LogError("Đang thiếu data, hãy thêm vào trong CharacterSkill");

        return new MysticOrbs(this, skill2Data);
    }

    #endregion
}
