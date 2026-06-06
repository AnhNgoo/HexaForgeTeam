using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "ScriptableObjects/CharacterData", order = 1)]
public class CharacterData : ScriptableObject
{
    public CharacterStats stats;
}

[System.Serializable]
public class CharacterStats
{
    public float health;
    public float speed;
    public DamageType damageType;
    public float damage;
    public float poisonDamage;
    public float stamina;
    [Range(100f, 150f)] public float attackSpeed;
}

[System.Serializable]
public enum DamageType
{
    Physical,
    Magical
}