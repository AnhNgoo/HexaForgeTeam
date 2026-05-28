using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICharacterSkill
{
    CharacterSkillData SkillData { get; }
    bool CanUseSkill();
    void UseSkill();
}
