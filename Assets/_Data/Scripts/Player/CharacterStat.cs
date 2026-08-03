using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStat : MonoBehaviour
{
    [SerializeField] private CharacterStats stats;
    public CharacterStats Stats => stats;

    private CharacterBase characterBase;

    public void Init(CharacterBase CharacterBase, CharacterStats characterStats)
    {
        this.characterBase = CharacterBase;
        SetStats(characterStats);
    }

    public void SetStats(CharacterStats characterStats)
    {
        this.stats = characterStats;
        characterBase.CharacterHealth.SetMaxHealth(characterStats.maxHealth);
        Debug.Log($"CharacterStat: SetMaxHealth called with maxHealth = {characterStats.maxHealth}");
        characterBase.CharacterStamina.SetMaxStamina(characterStats.stamina);
        Debug.Log($"CharacterStat: SetMaxStamina called with stamina = {characterStats.stamina}");
        characterBase.CharacterMP.SetMaxMP(characterStats.mp);
    }
}
