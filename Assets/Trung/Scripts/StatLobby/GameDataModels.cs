using System;
using System.Collections.Generic;
using UnityEngine;

public enum CharacterType
{
    Kael,
    Lyra,
    Ares,
    Elara
}

[Serializable]
public class AccountLevelData
{
    public int level = 1;
    public int currentExp = 0;
}

[Serializable]
public class CharacterRuneEquip
{
    public CharacterType characterType;
    public string[] equippedRuneIDs = new string[3] { "", "", "" };

    public CharacterRuneEquip(CharacterType type)
    {
        characterType = type;
        equippedRuneIDs = new string[3] { "", "", "" };
    }
}

[System.Serializable]
public class CharacterUnlockData
{
    public bool kaelUnlocked = true;
    public bool lyraUnlocked = false;
    public bool aresUnlocked = false;
    public bool elaraUnlocked = false;
    public CharacterType selectedCharacter = CharacterType.Kael;
    public List<CharacterRuneEquip> characterRuneBuilds = new List<CharacterRuneEquip>();
}

[Serializable]
public class SavedAccount
{
    public string username;
    public string password;
}

[Serializable]
public class SavedAccountList
{
    public List<SavedAccount> accounts = new List<SavedAccount>();
}

[Serializable]
public class InventoryItemData
{
    public string itemID;
    public string itemName;
    public int quantity;

    public InventoryItemData(string id, string name, int qty)
    {
        itemID = id;
        itemName = name;
        quantity = qty;
    }
}

[Serializable]
public class CostData
{
    public string itemID;
    public int amount;

    public CostData(string itemID, int amount)
    {
        this.itemID = itemID;
        this.amount = amount;
    }
}

[Serializable]
public class LobbyStatData
{
    public float HP;
    public float HPPercent;
    public float MP;
    public float MPPercent;
    public float Stamina;
    public float StaminaPercent;
    public float ATK;
    public float ATKPercent;
    public float DEF;
    public float DEFPercent;
    public float CritChance;
    public float CritDamage;
    public float ArmorPenetration;
    public float StaminaRegen;
}