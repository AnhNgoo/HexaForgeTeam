using System;
using System.Collections.Generic;
using UnityEngine;

public enum RuneColor
{
    Red,
    Green,
    Blue
}

public enum RuneRarity
{
    Common,
    Rare,
    Epic,
    Legendary
}

public enum RuneStatType
{
    HP,
    HPPercent,

    MP,
    MPPercent,

    Stamina,
    StaminaPercent,

    ATK,
    ATKPercent,


    DEF,
    DEFPercent,

    CritChance,
    CritDamage,
    ArmorPenetration,
    StaminaRegen,

}

[Serializable]
public class RuneAffixData
{
    public RuneStatType statType;

    public float value;

    public RuneAffixData()
    {

    }

    public RuneAffixData(
        RuneStatType statType,
        float value)
    {
        this.statType = statType;

        this.value = value;
    }
}

[Serializable]
public class RuneData
{
    public string runeID;

    public RuneColor runeColor;

    public RuneRarity runeRarity;
    public string runeName;

public string runeLore;

    public bool isEquipped;

    public int equippedSlotIndex = -1;

    public List<RuneAffixData> affixes =
        new List<RuneAffixData>();

    public RuneData()
    {

    }

    public RuneData(
        RuneColor runeColor,
        RuneRarity runeRarity)
    {
        runeID =
            Guid.NewGuid()
            .ToString();

        this.runeColor =
            runeColor;

        this.runeRarity =
            runeRarity;
    }
}