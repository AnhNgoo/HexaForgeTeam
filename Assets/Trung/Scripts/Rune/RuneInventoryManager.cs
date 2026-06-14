using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RuneInventoryManager :
    MonoBehaviour
{
    public static RuneInventoryManager Instance;

    public List<RuneData> runes =
        new List<RuneData>();
        [Header("Rune Hard Cap")]

[SerializeField] private float maxHP = 2800f;
[SerializeField] private float maxMP = 2800f;
[SerializeField] private float maxStamina = 2800f;

[SerializeField] private float maxATK = 110f;
[SerializeField] private float maxDEF = 75f;

[SerializeField] private float maxHPPercent = 28f;
[SerializeField] private float maxMPPercent = 28f;
[SerializeField] private float maxStaminaPercent = 28f;

[SerializeField] private float maxATKPercent = 24f;
[SerializeField] private float maxDEFPercent = 24f;


[SerializeField] private float maxCritChance = 38f;
[SerializeField] private float maxCritDamage = 90f;

[SerializeField] private float maxArmorPenetration = 28f;

[SerializeField] private float maxStaminaRegen = 45f;

    private void Start()
{
    LoadRunes();
}
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddRune(
        RuneData runeData)
    {
        runes.Add(runeData);

SaveRunes();
    }

    public void RemoveRune(
        string runeID)
    {
        for (int i = runes.Count - 1;
            i >= 0;
            i--)
        {
            if (runes[i].runeID == runeID)
            {
                runes.RemoveAt(i);

                break;
            }
        }

        SaveRunes();
    }
    #region Equip

public bool EquipRune(
    RuneData runeData)
{
    if (runeData == null)
    {
        return false;
    }

    if (runeData.isEquipped)
    {
        return false;
    }

    int emptySlot =
        GetEmptySlot();

    if (emptySlot == -1)
    {
        return false;
    }

    runeData.isEquipped = true;

    runeData.equippedSlotIndex =
        emptySlot;

    SaveRunes();

    if (LobbyStatManager.Instance != null)
{
    LobbyStatManager.Instance
        .RecalculateStats();
}
    return true;
}

public void UnequipRune(
    RuneData runeData)
{
    if (runeData == null)
    {
        return;
    }

    runeData.isEquipped = false;

    runeData.equippedSlotIndex = -1;

    SaveRunes();
    if (LobbyStatManager.Instance != null)
{
    LobbyStatManager.Instance
        .RecalculateStats();
}    
}

private int GetEmptySlot()
{
    bool slot0Used = false;

    bool slot1Used = false;

    bool slot2Used = false;

    for (int i = 0;
        i < runes.Count;
        i++)
    {
        RuneData rune =
            runes[i];

        if (!rune.isEquipped)
        {
            continue;
        }

        switch (rune.equippedSlotIndex)
        {
            case 0:

                slot0Used = true;

                break;

            case 1:

                slot1Used = true;

                break;

            case 2:

                slot2Used = true;

                break;
        }
    }

    if (!slot0Used)
    {
        return 0;
    }

    if (!slot1Used)
    {
        return 1;
    }

    if (!slot2Used)
    {
        return 2;
    }

    return -1;
}

#endregion
#region Total Stats

public Dictionary<RuneStatType, float>
    GetStats()
{
    Dictionary<RuneStatType, float>
        totalStats =
        new Dictionary<RuneStatType, float>();

    for (int i = 0;
        i < runes.Count;
        i++)
    {
        RuneData rune =
            runes[i];

        if (!rune.isEquipped)
        {
            continue;
        }

        for (int j = 0;
            j < rune.affixes.Count;
            j++)
        {
            RuneAffixData affix =
                rune.affixes[j];

            if (!totalStats.ContainsKey(
                affix.statType))
            {
                totalStats.Add(
                    affix.statType,
                    0f);
            }

            totalStats[affix.statType] +=
                affix.value;
        }
    }
    ApplyRuneHardCaps(
    totalStats);

    return totalStats;
}
[ContextMenu("Debug Total Stats")]
private void DebugTotalStats()
{
    Dictionary<RuneStatType, float>
        stats =
        GetStats();

    foreach (var stat in stats)
    {
        Debug.Log(
            $"{stat.Key} : {stat.Value}");
    }
}

#endregion
private void ApplyRuneHardCaps(
    Dictionary<RuneStatType, float> stats)
{
    ClampStat(stats, RuneStatType.HP, maxHP);
    ClampStat(stats, RuneStatType.MP, maxMP);
    ClampStat(stats, RuneStatType.Stamina, maxStamina);

    ClampStat(stats, RuneStatType.ATK, maxATK);
    ClampStat(stats, RuneStatType.DEF, maxDEF);

    ClampStat(stats, RuneStatType.HPPercent, maxHPPercent);
    ClampStat(stats, RuneStatType.MPPercent, maxMPPercent);
    ClampStat(stats, RuneStatType.StaminaPercent, maxStaminaPercent);

    ClampStat(stats, RuneStatType.ATKPercent, maxATKPercent);
    ClampStat(stats, RuneStatType.DEFPercent, maxDEFPercent);


    ClampStat(stats, RuneStatType.CritChance, maxCritChance);
    ClampStat(stats, RuneStatType.CritDamage, maxCritDamage);

    ClampStat(stats, RuneStatType.ArmorPenetration, maxArmorPenetration);

    ClampStat(stats, RuneStatType.StaminaRegen, maxStaminaRegen);
}

private void ClampStat(
    Dictionary<RuneStatType, float> stats,
    RuneStatType statType,
    float maxValue)
{
    if (!stats.ContainsKey(statType))
    {
        return;
    }

    stats[statType] =
        Mathf.Min(
            stats[statType],
            maxValue);
}

public float GetHardCap(
    RuneStatType statType)
{
    switch (statType)
    {
        case RuneStatType.HP:
            return maxHP;

        case RuneStatType.MP:
            return maxMP;

        case RuneStatType.Stamina:
            return maxStamina;

        case RuneStatType.ATK:
            return maxATK;


        case RuneStatType.DEF:
            return maxDEF;

        case RuneStatType.HPPercent:
            return maxHPPercent;

        case RuneStatType.MPPercent:
            return maxMPPercent;

        case RuneStatType.StaminaPercent:
            return maxStaminaPercent;

        case RuneStatType.ATKPercent:
            return maxATKPercent;


        case RuneStatType.DEFPercent:
            return maxDEFPercent;


        case RuneStatType.CritChance:
            return maxCritChance;

        case RuneStatType.CritDamage:
            return maxCritDamage;

        case RuneStatType.ArmorPenetration:
            return maxArmorPenetration;

        case RuneStatType.StaminaRegen:
            return maxStaminaRegen;
    }

    return 0f;
}
private void LoadRunes()
{
    runes =
        SaveLoadManager.Instance
        .SaveData.runes;

    if (runes == null)
    {
        runes =
            new List<RuneData>();
    }
}
private void SaveRunes()
{
    SaveLoadManager.Instance
        .SaveData.runes =
        runes;

    SaveLoadManager.Instance
        .SaveGame();
}
}