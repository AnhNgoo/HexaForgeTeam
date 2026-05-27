using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public abstract class CharacterSkillBase : ICharacterSkill
{
    protected CharacterBase character;
    protected CharacterSkillData skillData;
    protected Cooldown cooldown = new Cooldown();
    public CharacterSkillData SkillData => skillData;

    public CharacterSkillBase(CharacterBase character, CharacterSkillData skillData)
    {
        this.character = character;
        this.skillData = skillData;
    }
    public bool CanUseSkill()
    {
        if (cooldown.IsOnCooldown)
            return false;

        if (!character.CheckConditionAttack())
            return false;

        return true;
    }

    public void UseSkill()
    {
        if (!CanUseSkill())
            return;

        if (skillData == null)
        {
            Debug.LogError("Skill data is null!");
            return;
        }
        cooldown.StartCooldown(skillData.cooldown);
        character.StateController.ChangeState(new CombatState(character));
        ExecuteSkill().Forget();
    }

    protected abstract UniTask ExecuteSkill();
}
