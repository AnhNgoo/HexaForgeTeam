using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.EventSystems;

public class Kael : CharacterMelee
{
    [Header("Kael")]
    [SerializeField] protected GameObject kaelGiantVisual;
    [SerializeField] protected float kaelGiantforwardAttackOffset = 2.5f;
    [SerializeField] protected float kaelGiantYAttackOffset = 1f;
    [SerializeField] protected float kaelGiantAttackHitBoxRadius = 2f;

    [Header("Effect Points")]
    public GameObject earthBreakerEffectPoint;
    public GameObject kaelGiantPunchEffectPoint_1;
    public GameObject kaelGiantPunchEffectPoint_2;
    public GameObject kaelGiantPunchEffectPoint_3;
    public GameObject kaelGiantPunchEffectPoint_4;

    [Header("Effects")]
    public PoolType earthBreakerEffect = PoolType.EarthBreaker_2;
    public PoolType auraEffect_1 = PoolType.AuraEffect_1;
    public PoolType auraEffect_2 = PoolType.AuraEffect_2;
    public PoolType auraEffect_3 = PoolType.AuraEffect_3;
    public PoolType auraEffect_4 = PoolType.AuraEffect_4;
    public PoolType auraEffect_5 = PoolType.AuraEffect_5;
    public PoolType kaelGiantAuraEffect_1 = PoolType.KaelGiantAuraEffect_1;
    public PoolType kaelGiantPunchEffect_1 = PoolType.KaelGiantPunchEffect_1;
    public PoolType kaelGiantPunchEffect_2 = PoolType.KaelGiantPunchEffect_2;


    public bool IsGiantForm { get; private set; } = false;
    protected override void LoadComponent()
    {
        base.LoadComponent();
        if (kaelGiantVisual == null)
            kaelGiantVisual = visuals.transform.Find("KaelGiant").gameObject;
    }

    protected override void LoadEffectPoints()
    {
        base.LoadEffectPoints();
        if (earthBreakerEffectPoint == null)
            earthBreakerEffectPoint = effectPoints.transform.Find("EarthBreakerEffectPoint").gameObject;
        if (kaelGiantPunchEffectPoint_1 == null)
            kaelGiantPunchEffectPoint_1 = effectPoints.transform.Find("KaelGiantPunchEffectPoint_1").gameObject;
        if (kaelGiantPunchEffectPoint_2 == null)
            kaelGiantPunchEffectPoint_2 = effectPoints.transform.Find("KaelGiantPunchEffectPoint_2").gameObject;
        if (kaelGiantPunchEffectPoint_3 == null)
            kaelGiantPunchEffectPoint_3 = effectPoints.transform.Find("KaelGiantPunchEffectPoint_3").gameObject;
        if (kaelGiantPunchEffectPoint_4 == null)
            kaelGiantPunchEffectPoint_4 = effectPoints.transform.Find("KaelGiantPunchEffectPoint_4").gameObject;
    }
    protected override void Init(CharacterData data)
    {
        base.Init(data);
    }

    #region Override đòn tấn công
    protected override IAttackStep[] InitPunchCombos()
    {
        return new IAttackStep[4]
        {
            new KaelPunchStep_1(this),
            new KaelPunchStep_2(this),
            new KaelPunchStep_3(this),
            new KaelPunchStep_4(this)
        };
    }

    // Override để khởi tạo các đòn tấn công riêng cho Kael
    protected override IAttackStep[] InitAttackCombos()
    {
        return new IAttackStep[4]
        {
            new KaelAttacStep_1(this),
            new KaelAttackStep_2(this),
            new KaelAttackStep_3(this),
            new KaelAttackStep_4(this)
        };
    }

    #endregion

    #region Override kỹ năng
    protected override ICharacterSkill GetSkill_1()
    {
        if (characterSkill.SkillData1 == null)
            Debug.LogError("Đang thiếu data, hãy thêm vào trong CharacterSkill");

        return new EarthBreaker(this, characterSkill.SkillData1);
    }

    protected override ICharacterSkill GetSkill_2()
    {
        if (characterSkill.SkillData2 == null)
            Debug.LogError("Đang thiếu data, hãy thêm vào trong CharacterSkill");

        return new EarthRages(this, characterSkill.SkillData2);
    }

    #endregion

    public virtual void GiantForm()
    {
        IsGiantForm = true;
        characterCombat.ResetCombo(); // Reset combo khi biến hình để tránh lỗi combo giữa 2 hình dạng
        characterMeleeHitbox.SetHitBox(kaelGiantforwardAttackOffset, kaelGiantYAttackOffset, kaelGiantAttackHitBoxRadius); // Cập nhật hitbox cho hình dạng khổng lồ
        characterWeapon.StoreWeapon(); // Cất vũ khí khi biến hình
        GetDashShadowEffect(kaelGiantVisual);
        // Chuyển sang hình dạng khổng lồ  
        characterAnimation.Init(kaelGiantVisual);
        kaelGiantVisual.SetActive(true);
        characterVisual.SetActive(false);
    }

    public virtual void NormalForm()
    {
        IsGiantForm = false;
        characterCombat.ResetCombo(); // Reset combo khi biến hình để tránh lỗi combo giữa 2 hình dạng
        characterMeleeHitbox.ResetHitBox(); // Reset hitbox về giá trị mặc định
        characterWeapon.RetrieveWeapon(); // Lấy lại vũ khí khi trở về hình dạng bình thường
        GetDashShadowEffect(characterVisual);
        // Chuyển về hình dạng bình thường  
        characterAnimation.Init(characterVisual);
        kaelGiantVisual.SetActive(false);
        characterVisual.SetActive(true);
    }
}
