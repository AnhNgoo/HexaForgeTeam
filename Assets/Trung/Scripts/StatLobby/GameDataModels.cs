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

public enum QuestType
{
    Tutorial,
    Main,
    Daily,
    Achievement
}

public enum QuestState
{
    NotStarted,
    InProgress,
    CanClaim,
    Completed
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

    public void Reset()
    {
        HP = 0; HPPercent = 0;
        MP = 0; MPPercent = 0;
        Stamina = 0; StaminaPercent = 0;
        ATK = 0; ATKPercent = 0;
        DEF = 0; DEFPercent = 0;
        CritChance = 0; CritDamage = 0;
        ArmorPenetration = 0; StaminaRegen = 0;
    }
}

[Serializable]
public class CombinedLobbyStats
{
    public CharacterType targetCharacter;
    public LobbyStatData levelBonusStats = new LobbyStatData();
    public LobbyStatData runeBonusStats = new LobbyStatData();
}

[System.Serializable]
public class SaveData
{
    public int accountLevel = 1;
    public int lifetimeGemEarned = 0;
    public int totalKills = 0;
    public int totalRuns = 0;
    public List<AchievementData> achievements = new List<AchievementData>();
}

[Serializable]
public class QuestData
{
    public string questID;
    public string title;
    [TextArea(2, 4)] public string description;
    public QuestType questType = QuestType.Main;
    public int currentProgress;
    public int targetProgress;
    public int rewardGem;
    public int rewardShard;
    public List<CostData> rewardItems = new List<CostData>();
    public QuestState state = QuestState.NotStarted;

    public QuestData(string id, string title, string description, QuestType type, int target, int gem = 0, int shard = 0, List<CostData> items = null)
    {
        this.questID = id;
        this.title = title;
        this.description = description;
        this.questType = type;
        this.targetProgress = target;
        this.rewardGem = gem;
        this.rewardShard = shard;
        if (items != null) this.rewardItems = items;
        this.currentProgress = 0;
        this.state = QuestState.NotStarted;
    }

    public void AddProgress(int amount)
    {
        if (state == QuestState.Completed || state == QuestState.CanClaim) return;

        currentProgress += amount;
        if (currentProgress >= targetProgress)
        {
            currentProgress = targetProgress;
            state = QuestState.CanClaim;
        }
    }
}