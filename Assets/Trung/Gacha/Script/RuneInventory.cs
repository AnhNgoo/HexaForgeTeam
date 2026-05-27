using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RuneInventory : MonoBehaviour
{
    public static RuneInventory Instance;

    public List<RuneData> runes =
        new List<RuneData>();

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

        SaveLoadManager.Instance
            .SaveInventory();
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

        SaveLoadManager.Instance
            .SaveInventory();
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

    SaveLoadManager.Instance
        .SaveInventory();

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

    SaveLoadManager.Instance
        .SaveInventory();
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
    GetTotalStats()
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

    return totalStats;
}
[ContextMenu("Debug Total Stats")]
private void DebugTotalStats()
{
    Dictionary<RuneStatType, float>
        stats =
        GetTotalStats();

    foreach (var stat in stats)
    {
        Debug.Log(
            $"{stat.Key} : {stat.Value}");
    }
}

#endregion
}