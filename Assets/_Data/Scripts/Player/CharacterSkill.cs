using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSkill : MonoBehaviour
{
    [SerializeField] private CharacterSkillData skillData1;
    public CharacterSkillData SkillData1 => skillData1;
    [SerializeField] private CharacterSkillData skillData2;
    public CharacterSkillData SkillData2 => skillData2;
    private ICharacterSkill skill1;
    private ICharacterSkill skill2;
    private CharacterBase characterBase;

    public void Init(CharacterBase character, ICharacterSkill skill1, ICharacterSkill skill2)
    {
        this.characterBase = character;
        this.skill1 = skill1;
        this.skill2 = skill2;
    }

    public void UseSkill1()
    {
        skill1?.UseSkill();
    }

    public void UseSkill2()
    {
        skill2?.UseSkill();
    }
}
