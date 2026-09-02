using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "ScriptableObjects/CharacterData", order = 1)]
public class CharacterData : ScriptableObject
{
    public string characterName;
    public CharacterTypes characterTypes;
    public PoolType characterPoolType;
    public WeaponData weaponData;
    public CharacterStats stats;
    public StaminaCost staminaCost;
    public MPCost mpCost;
    public CharacterSkillData skill1Data;
    public CharacterSkillData skill2Data;
}

[System.Serializable]
public class CharacterStats
{
    public float maxHealth;
    public float speed;
    public float damage;
    public float defense;
    public float poisonDamage;
    public float stamina;
    public float staminaRegen;
    public float mp;
    public float mpRegen;
}

[System.Serializable]
public enum CharacterTypes
{
    None = 0,
    PhysicalMelee = 1,
    Magical = 2
}

[System.Serializable]
public class StaminaCost
{
    public float sprintCost;
    public float dodgeCost;
    public float jumpCost;
    public float attackCost;
}

[System.Serializable]
public class MPCost
{
    public float attackCost;
}
