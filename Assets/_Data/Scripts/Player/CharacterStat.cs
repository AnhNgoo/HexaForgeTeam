using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStat : MonoBehaviour
{
    [SerializeField] private CharacterStats stats;
    public CharacterStats Stats => stats;

    private CharacterBase characterBase;

    public void Init(CharacterBase CharacterBase)
    {
        this.characterBase = CharacterBase;
        SetStats(characterBase.CharacterData.stats);
    }

    public void SetStats(CharacterStats characterStats)
    {
        this.stats = characterStats;
        characterBase.CharacterHealth.SetMaxHealth(characterStats.maxHealth);
        characterBase.CharacterStamina.SetMaxStamina(characterStats.stamina);
        characterBase.CharacterMP.SetMaxMP(characterStats.mp);
    }
}
