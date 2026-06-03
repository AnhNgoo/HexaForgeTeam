using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterSkillData", menuName = "ScriptableObjects/CharacterSkillData", order = 1)]
public class CharacterSkillData : ScriptableObject
{
    public string skillName;
    [TextArea] public string skillDescription;
    public Sprite skillIcon;

    //NOTE - Thêm các trường chỉ số kỹ năng ở đây
    [Header("Skill Stats")]
    public int damage;
    public float cooldown;
    public float duration;
}
