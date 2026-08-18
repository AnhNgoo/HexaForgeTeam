using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "CharacterSkillData", menuName = "ScriptableObjects/CharacterSkillData", order = 1)]
public class CharacterSkillData : ScriptableObject
{
    public string skillName;
    [TextArea(3, 10)] public string skillDescription;
    public Sprite skillIcon;
    public SkillStats skillStats;
}

[System.Serializable]
public class SkillStats
{
    public bool hasStatsBonus; // Skill có cộng thêm chỉ số cho nhân vật khi sử dụng hay không
    [ShowIf("hasStatsBonus")] public CharacterStats characterStatsBonus; // stats được cộng thêm của nhân vật khi sử dụng skill
    public float skillCost; // năng lượng tiêu hao (có thể là mana hoặc stamina)
    public float damage; // damage cơ bản của skill
    public float damageBonus; //Damage cộng thêm
    [Range(0f, 2f)] public float damagePercentage; // damage theo phần trăm (ví dụ: 0.2f = 20% damage cộng thêm)
    public float poisonDamage;
    public float cooldown;
    public float duration;
}
