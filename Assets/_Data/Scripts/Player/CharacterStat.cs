using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStat : MonoBehaviour
{
    [SerializeField] private CharacterStats originStats;
    public CharacterStats OriginStats => originStats;

    [SerializeField] private CharacterStats runStats = new();
    public CharacterStats RunStats => runStats;

    private CharacterStats cachedCurrentStats;

    public CharacterStats runeStats { get; set; } = new CharacterStats();
    public CharacterStats levelStats { get; set; } = new CharacterStats();
    public CharacterStats finalStats { get; set; } = new CharacterStats();
    private CharacterBase characterBase;
    private float cachedCurrentHealth;

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
        finalStats.maxHealth = originStats.maxHealth + runeStats.maxHealth + levelStats.maxHealth + runStats.maxHealth;
        finalStats.speed = originStats.speed + runeStats.speed + levelStats.speed + runStats.speed;
        finalStats.damage = originStats.damage + runeStats.damage + levelStats.damage + runStats.damage;
        finalStats.defense = originStats.defense + runeStats.defense + levelStats.defense + runStats.defense;
        finalStats.poisonDamage = originStats.poisonDamage + runeStats.poisonDamage + levelStats.poisonDamage + runStats.poisonDamage;
        finalStats.stamina = originStats.stamina + runeStats.stamina + levelStats.stamina + runStats.stamina;
        finalStats.staminaRegen = originStats.staminaRegen + runeStats.staminaRegen + levelStats.staminaRegen + runStats.staminaRegen;
        finalStats.mp = originStats.mp + runeStats.mp + levelStats.mp + runStats.mp;
        finalStats.mpRegen = originStats.mpRegen + runeStats.mpRegen + levelStats.mpRegen + runStats.mpRegen;


        characterBase.CharacterHealth.SetMaxHealth(finalStats.maxHealth, isSetMaxHealth);
        characterBase.CharacterStamina.SetMaxStamina(finalStats.stamina, isSetMaxStamina);
        characterBase.CharacterMP.SetMaxMP(finalStats.mp, isSetMaxMP);
    }

    public bool ApplyRunReward(BossRewardType rewardType, float percentage)
    {
        if (percentage <= 0f) return false;

        float ratio = percentage / 100f;

        switch (rewardType)
        {
            case BossRewardType.MaxHealth:
                {
                    float oldMax = finalStats.maxHealth;
                    runStats.maxHealth += oldMax * ratio;
                    SetFinalStats(false, false, false);
                    characterBase.CharacterHealth.AddHealth(finalStats.maxHealth - oldMax);
                    return true;
                }

            case BossRewardType.Damage:
                runStats.damage += finalStats.damage * ratio;
                SetFinalStats(false, false, false);
                return true;

            case BossRewardType.Defense:
                runStats.defense += finalStats.defense * ratio;
                SetFinalStats(false, false, false);
                return true;

            case BossRewardType.Stamina:
                {
                    float oldMax = finalStats.stamina;
                    runStats.stamina += oldMax * ratio;
                    SetFinalStats(false, false, false);
                    characterBase.CharacterStamina.AddStamina(finalStats.stamina - oldMax);
                    return true;
                }
            case BossRewardType.MoveSpeed:
                runStats.speed += finalStats.speed * ratio;
                break;

            case BossRewardType.PoisonDamage:
                runStats.poisonDamage += finalStats.poisonDamage * ratio;
                break;

            case BossRewardType.StaminaRegen:
                runStats.staminaRegen += finalStats.staminaRegen * ratio;
                break;

            case BossRewardType.MPRegen:
                runStats.mpRegen += finalStats.mpRegen * ratio;
                break;

            case BossRewardType.MaxMP:
                {
                    float oldMax = finalStats.mp;
                    runStats.mp += oldMax * ratio;
                    SetFinalStats(false, false, false);
                    characterBase.CharacterMP.AddMP(finalStats.mp - oldMax);
                    return true;
                }

            default: return false;
        }
        SetFinalStats(false, false, false);
        return true;
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

        cachedCurrentHealth = characterBase.CharacterHealth.CurrentHealth > 0 ? characterBase.CharacterHealth.CurrentHealth : 1; // Lưu lại máu hiện tại trước khi dùng skill, nếu máu <= 0 thì set thành 1 để tránh lỗi
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
        characterBase.CharacterHealth.SetCurrentHealth(cachedCurrentHealth);
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
    private static CharacterStats CloneStats(CharacterStats source)
    {
        if (source == null) return new CharacterStats();

        return new CharacterStats
        {
            maxHealth = source.maxHealth,
            speed = source.speed,
            damage = source.damage,
            defense = source.defense,
            poisonDamage = source.poisonDamage,
            stamina = source.stamina,
            staminaRegen = source.staminaRegen,
            mp = source.mp,
            mpRegen = source.mpRegen
        };
    }

    public void ResetRunStats()
    {
        runStats = new CharacterStats();
        SetFinalStats();
    }

}
