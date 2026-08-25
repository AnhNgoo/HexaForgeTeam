using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSkill : MonoBehaviour
{
    [SerializeField] private CharacterSkillData skill1Data;
    public CharacterSkillData Skill1Data => skill1Data;
    [SerializeField] private CharacterSkillData skill2Data;
    public CharacterSkillData Skill2Data => skill2Data;
    private ICharacterSkill skill1;
    private ICharacterSkill skill2;
    public bool CanUseSkill1 { get; set; } = true;
    public bool CanUseSkill2 { get; set; } = true;
    public bool IsUsingSkill { get; set; } = false;
    public bool IsUsingSkill1 { get; set; } = false;
    public bool IsUsingSkill2 { get; set; } = false;

    private CharacterBase characterBase;

    public void Init(CharacterBase character, CharacterSkillData skill1Data, CharacterSkillData skill2Data, ICharacterSkill skill1, ICharacterSkill skill2)
    {
        if (skill1Data == null || skill2Data == null)
        {
            Debug.LogError("CharacterSkillData is null, please assign CharacterSkillData in the inspector.");
            return;
        }
        else if (skill1Data != null && skill2Data != null)
        {
            Debug.Log("CharacterSkillData is assigned successfully.");
        }

        this.characterBase = character;
        this.skill1 = skill1;
        this.skill2 = skill2;
        this.skill1Data = skill1Data;
        this.skill2Data = skill2Data;
        EventManager.Notify(GameEvent.OnSetImageSkill1, skill1Data.skillIcon);
        EventManager.Notify(GameEvent.OnSetImageSkill2, skill2Data.skillIcon);
    }

    public void UseSkill1()
    {
        if (!CanUseSkill1)
            return;
        IsUsingSkill1 = true;
        skill1?.UseSkill();
    }

    public void UseSkill2()
    {
        if (!CanUseSkill2)
            return;
        IsUsingSkill2 = true;
        skill2?.UseSkill();
    }

    public void LockUseSkill(bool lockSkill1 = false, bool lockSkill2 = false)
    {
        CanUseSkill1 = !lockSkill1;
        CanUseSkill2 = !lockSkill2;
    }
}
