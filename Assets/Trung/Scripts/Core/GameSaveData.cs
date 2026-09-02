using System.Collections.Generic;

[System.Serializable]
public class GameSaveData
{
    public int gem = 300; // Cân bằng: Đổi từ 3000 về 300 cho khởi đầu chuẩn
    public int accountLevel = 1;
    public int accountExp = 0;
    public int lifetimeGemEarned = 0;
    public int runeShards = 0;
    public CharacterUnlockData characterData = new CharacterUnlockData();
    public List<AchievementData> achievements = new List<AchievementData>();
    public List<QuestData> quests = new List<QuestData>();
    public List<RuneData> runes = new List<RuneData>();
    public bool isTutorialCompleted = false;
    public List<InventoryItemData> inventoryItems = new List<InventoryItemData>();
    public int totalKills = 0; 
    public int totalRuns = 0;

    public GameSaveData()
    {
        gem = 300;
        accountLevel = 1;
        accountExp = 0;
        lifetimeGemEarned = 0;
        runeShards = 0;
        isTutorialCompleted = false;

        if (characterData == null) characterData = new CharacterUnlockData();
        if (achievements == null) achievements = new List<AchievementData>();
        if (quests == null) quests = new List<QuestData>();
        if (runes == null) runes = new List<RuneData>();
        if (inventoryItems == null) inventoryItems = new List<InventoryItemData>();
    }
}