using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerStatTest : MonoBehaviour
{
    public static PlayerStatTest Instance;

    [Header("Base Stats")]

    [SerializeField] private float baseHP = 1000f;

    [SerializeField] private float baseMP = 100f;

    [SerializeField] private float baseStamina = 100f;

    [SerializeField] private float baseATK = 25f;

    [SerializeField] private float baseMATK = 20f;

    [SerializeField] private float baseDEF = 10f;

    [Header("Final Stats")]

    public float finalHP;

    public float finalMP;

    public float finalStamina;

    public float finalATK;

    public float finalMATK;

    public float finalDEF;

    public float attackSpeed;

    public float critChance;

    public float critDamage;

    public float armorPenetration;

    public float staminaRegen;

    [Header("Debug UI")]
    [SerializeField] private TMP_Text debugText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    private void Update()
{
    RecalculateStats();
}

    public void RecalculateStats()
    {
        ResetStats();
        if (RuneInventory.Instance == null)
{
    return;
}

Dictionary<RuneStatType, float>
    stats =
    RuneInventory.Instance
    .GetStats();

        foreach (var stat in stats)
        {
            switch (stat.Key)
            {
                #region Flat

                case RuneStatType.HP:

                    finalHP += stat.Value;

                    break;

                case RuneStatType.MP:

                    finalMP += stat.Value;

                    break;

                case RuneStatType.Stamina:

                    finalStamina += stat.Value;

                    break;

                case RuneStatType.ATK:

                    finalATK += stat.Value;

                    break;

                case RuneStatType.MATK:

                    finalMATK += stat.Value;

                    break;

                case RuneStatType.DEF:

                    finalDEF += stat.Value;

                    break;

                #endregion

                #region Percent

                case RuneStatType.HPPercent:

                    finalHP +=
                        baseHP *
                        (stat.Value / 100f);

                    break;

                case RuneStatType.MPPercent:

                    finalMP +=
                        baseMP *
                        (stat.Value / 100f);

                    break;

                case RuneStatType.StaminaPercent:

                    finalStamina +=
                        baseStamina *
                        (stat.Value / 100f);

                    break;

                case RuneStatType.ATKPercent:

                    finalATK +=
                        baseATK *
                        (stat.Value / 100f);

                    break;

                case RuneStatType.MATKPercent:

                    finalMATK +=
                        baseMATK *
                        (stat.Value / 100f);

                    break;

                case RuneStatType.DEFPercent:

                    finalDEF +=
                        baseDEF *
                        (stat.Value / 100f);

                    break;

                #endregion

                #region Combat

                case RuneStatType.AttackSpeed:

                    attackSpeed += stat.Value;

                    break;

                case RuneStatType.CritChance:

                    critChance += stat.Value;

                    break;

                case RuneStatType.CritDamage:

                    critDamage += stat.Value;

                    break;

                case RuneStatType.ArmorPenetration:

                    armorPenetration += stat.Value;

                    break;

                case RuneStatType.StaminaRegen:

                    staminaRegen += stat.Value;

                    break;

                #endregion
            }
        }

        UpdateDebugUI();
    }

    private void ResetStats()
    {
        finalHP = baseHP;

        finalMP = baseMP;

        finalStamina = baseStamina;

        finalATK = baseATK;

        finalMATK = baseMATK;

        finalDEF = baseDEF;

        attackSpeed = 0f;

        critChance = 0f;

        critDamage = 0f;

        armorPenetration = 0f;

        staminaRegen = 0f;
    }

    private void UpdateDebugUI()
    {
        if (debugText == null)
        {
            return;
        }

        debugText.text =
            $"HP : {finalHP:F0}\n" +
            $"MP : {finalMP:F0}\n" +
            $"STA : {finalStamina:F0}\n\n" +

            $"ATK : {finalATK:F0}\n" +
            $"MATK : {finalMATK:F0}\n" +
            $"DEF : {finalDEF:F0}\n\n" +

            $"ASPD : {attackSpeed:F1}%\n" +
            $"CRIT : {critChance:F1}%\n" +
            $"CRIT DMG : {critDamage:F1}%\n" +
            $"ARM PEN : {armorPenetration:F1}%\n" +
            $"STA REG : {staminaRegen:F1}%";
    }
}