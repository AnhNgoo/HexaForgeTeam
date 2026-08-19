using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStat : MonoBehaviour
{
    [SerializeField] private CharacterStats originStats;
    public CharacterStats OriginStats => originStats;

    private CharacterStats cachedCurrentStats;

    public CharacterStats runeStats { get; set; } = new CharacterStats();
    public CharacterStats levelStats { get; set; } = new CharacterStats();
    public CharacterStats finalStats { get; set; } = new CharacterStats();
    private CharacterBase characterBase;

    public void Init(CharacterBase CharacterBase, CharacterStats characterStats)
    {
        this.characterBase = CharacterBase;
        SetOriginStats(characterStats);
    }

    public void SetOriginStats(CharacterStats characterStats)
    {
        this.originStats = characterStats;
        SetFinalStats();
    }

    public void SetRuneStats(CharacterStats runeStats)
    {
        this.runeStats = runeStats;
        SetFinalStats();
    }

    public void SetLevelStats(CharacterStats levelStats)
    {
        this.levelStats = levelStats;
        SetFinalStats();
    }

    /// <summary>
    /// Tính toán các chỉ số cuối cùng của nhân vật dựa trên các chỉ số gốc, chỉ số từ rune, chỉ số từ level và các hiệu ứng khác.
    /// </summary>
    public void SetFinalStats(bool isSetMaxHealth = true, bool isSetMaxStamina = true, bool isSetMaxMP = true)
    {
        finalStats = new CharacterStats();
        finalStats.maxHealth = originStats.maxHealth + runeStats.maxHealth + levelStats.maxHealth;
        finalStats.speed = originStats.speed + runeStats.speed + levelStats.speed;
        finalStats.damage = originStats.damage + runeStats.damage + levelStats.damage;
        finalStats.defense = originStats.defense + runeStats.defense + levelStats.defense;
        finalStats.poisonDamage = originStats.poisonDamage + runeStats.poisonDamage + levelStats.poisonDamage;
        finalStats.stamina = originStats.stamina + runeStats.stamina + levelStats.stamina;
        finalStats.staminaRegen = originStats.staminaRegen + runeStats.staminaRegen + levelStats.staminaRegen;
        finalStats.mp = originStats.mp + runeStats.mp + levelStats.mp;
        finalStats.mpRegen = originStats.mpRegen + runeStats.mpRegen + levelStats.mpRegen;

        characterBase.CharacterHealth.SetMaxHealth(finalStats.maxHealth, isSetMaxHealth);
        characterBase.CharacterStamina.SetMaxStamina(finalStats.stamina, isSetMaxStamina);
        characterBase.CharacterMP.SetMaxMP(finalStats.mp, isSetMaxMP);
    }

    #region Skill Stats
    /// <summary>
    /// Tính chỉ số khi dùng skill, lấy chỉ số final cộng thêm chỉ số của skill (skillData.skillStats.characterStats)
    /// </summary>
    /// <param name="skillData"></param>
    public void SetStatsForSkill(CharacterSkillData skillData)
    {
        cachedCurrentStats = finalStats; // Lưu lại chỉ số hiện tại trước khi dùng skill
        finalStats.maxHealth += skillData.skillStats.characterStatsBonus.maxHealth;
        finalStats.speed += skillData.skillStats.characterStatsBonus.speed;
        finalStats.damage += skillData.skillStats.characterStatsBonus.damage;
        finalStats.defense += skillData.skillStats.characterStatsBonus.defense;
        finalStats.poisonDamage += skillData.skillStats.characterStatsBonus.poisonDamage;
        finalStats.stamina += skillData.skillStats.characterStatsBonus.stamina;
        finalStats.staminaRegen += skillData.skillStats.characterStatsBonus.staminaRegen;
        finalStats.mp += skillData.skillStats.characterStatsBonus.mp;
        finalStats.mpRegen += skillData.skillStats.characterStatsBonus.mpRegen;

        characterBase.CharacterHealth.SetMaxHealth(finalStats.maxHealth);
        characterBase.CharacterStamina.SetMaxStamina(finalStats.stamina);
        characterBase.CharacterMP.SetMaxMP(finalStats.mp);
    }

    public void ResetStatsAfterSkill()
    {
        finalStats = cachedCurrentStats; // Khôi phục lại chỉ số trước khi dùng skill
        characterBase.CharacterHealth.SetMaxHealth(finalStats.maxHealth, false);
        characterBase.CharacterStamina.SetMaxStamina(finalStats.stamina);
        characterBase.CharacterMP.SetMaxMP(finalStats.mp);
    }

    /// <summary>
    /// Tính damage của kỹ năng
    /// Nếu có damage riêng của kỹ năng (skillData.skillStats.damage > 0), thì damage = damage riêng + damage cộng thêm + damage theo phần trăm
    /// Nếu không có damage riêng của kỹ năng (skillData.skillStats.damage <= 0), thì damage = damage cơ bản của nhân vật + damage cộng thêm + damage theo phần trăm
    /// </summary>
    /// <returns></returns>
    public float GetSkillDamage(CharacterSkillData skillData)
    {
        float totalDamage;
        if (skillData.skillStats.damage <= 0) // Nếu không có damage riêng
        {
            totalDamage = finalStats.damage + skillData.skillStats.damageBonus + (finalStats.damage * skillData.skillStats.damagePercentage);
        }
        else // Nếu có damage riêng
        {
            totalDamage = skillData.skillStats.damage + skillData.skillStats.damageBonus + (finalStats.damage * skillData.skillStats.damagePercentage);
        }

        return totalDamage;
    }

    #endregion

    #region  Weapon Stats

    public float GetWeaponDamage()
    {

        float totalDamage = 0;
        if (characterBase.CharacterWeapon != null && characterBase.CharacterWeapon.HasWeapon)
        {
            WeaponStats weaponStats = characterBase.CharacterWeapon.CurrentWeapon.weaponStats;
            totalDamage = weaponStats.damageBonus + (finalStats.damage * weaponStats.damagePercentage);
        }
        return totalDamage;
    }

    public float GetWeaponPoisonDamage()
    {
        float totalPoisonDamage = 0;
        if (characterBase.CharacterWeapon != null && characterBase.CharacterWeapon.HasWeapon)
        {
            WeaponStats weaponStats = characterBase.CharacterWeapon.CurrentWeapon.weaponStats;
            totalPoisonDamage = weaponStats.poisonDamage;
        }
        return totalPoisonDamage;
    }
    #endregion
}
