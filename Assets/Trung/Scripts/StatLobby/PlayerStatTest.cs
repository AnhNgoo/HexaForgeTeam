using UnityEngine;

public class PlayerStatTest : MonoBehaviour
{
    [Header("Base Stats")]

    [SerializeField]
    private float baseHP = 1000f;

    [SerializeField]
    private float baseMP = 100f;

    [SerializeField]
    private float baseStamina = 100f;

    [SerializeField]
    private float baseATK = 20f;

    [SerializeField]
    private float baseDEF = 10f;

    [Header("Final Stats (Read Only)")]

    public float FinalHP;

    public float FinalMP;

    public float FinalStamina;

    public float FinalATK;

    public float FinalDEF;

    public float FinalCritChance;

    public float FinalCritDamage;

    public float FinalArmorPenetration;

    public float FinalStaminaRegen;

    private void Start()
    {
        RecalculateStats();
    }

    [ContextMenu("Recalculate Stats")]
    public void RecalculateStats()
    {
        if (LobbyStatManager.Instance == null)
        {
            Debug.LogWarning(
                "LobbyStatManager not found");

            return;
        }

        LobbyStatData bonus =
            LobbyStatManager.Instance
            .GetBonusStats();

        #region HP

        FinalHP =
            baseHP +
            bonus.HP +
            (baseHP *
             bonus.HPPercent / 100f);

        #endregion

        #region MP

        FinalMP =
            baseMP +
            bonus.MP +
            (baseMP *
             bonus.MPPercent / 100f);

        #endregion

        #region Stamina

        FinalStamina =
            baseStamina +
            bonus.Stamina +
            (baseStamina *
             bonus.StaminaPercent / 100f);

        #endregion

        #region ATK

        FinalATK =
            baseATK +
            bonus.ATK +
            (baseATK *
             bonus.ATKPercent / 100f);

        #endregion

        #region DEF

        FinalDEF =
            baseDEF +
            bonus.DEF +
            (baseDEF *
             bonus.DEFPercent / 100f);

        #endregion

        #region Combat

        FinalCritChance =
            bonus.CritChance;

        FinalCritDamage =
            bonus.CritDamage;

        FinalArmorPenetration =
            bonus.ArmorPenetration;

        FinalStaminaRegen =
            bonus.StaminaRegen;

        #endregion

        DebugStats();
    }

    private void DebugStats()
    {
        Debug.Log(
            "===== CHARACTER FINAL STATS =====\n" +

            $"HP: {FinalHP}\n" +
            $"MP: {FinalMP}\n" +
            $"Stamina: {FinalStamina}\n\n" +

            $"ATK: {FinalATK}\n" +
            $"DEF: {FinalDEF}\n\n" +

            $"Crit Chance: {FinalCritChance}%\n" +
            $"Crit Damage: {FinalCritDamage}%\n" +

            $"Armor Pen: {FinalArmorPenetration}%\n" +
            $"Stamina Regen: {FinalStaminaRegen}%"
        );
    }
}