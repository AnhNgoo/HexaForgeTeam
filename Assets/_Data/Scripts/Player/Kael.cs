using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.EventSystems;
public class Kael : CharacterMelee
{
    [Header("Kael")]
    [SerializeField] protected GameObject kaelGiantVisual;
    public GameObject earthBreakerEffectPoint;
    public GameObject auraEffect;
    public GameObject skill2Effect;
    protected override void LoadComponent()
    {
        base.LoadComponent();
        if (characterVisual == null)
            characterVisual = visuals.transform.Find("Kael").gameObject;
        if (kaelGiantVisual == null)
            kaelGiantVisual = visuals.transform.Find("KaelGiant").gameObject;
    }

    protected override void LoadEffectPoints()
    {
        base.LoadEffectPoints();
        if (earthBreakerEffectPoint == null)
            earthBreakerEffectPoint = effectPoints.transform.Find("EarthBreakerEffectPoint").gameObject;
        if (auraEffect == null)
            auraEffect = effectPoints.transform.Find("AuraEffect").gameObject;
    }
    protected override void Init(CharacterData data)
    {
        base.Init(data);
    }

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

        return new EarthBreaker(this, characterSkill.SkillData2);
    }
}
