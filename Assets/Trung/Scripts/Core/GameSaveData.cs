using System.Collections.Generic;

[System.Serializable]
public class GameSaveData
{
    // Gem
    public int gem = 3000;

    // Account
    public int accountLevel = 1;
    public int accountExp = 0;

    // Leaderboard
    public int lifetimeGemEarned = 0;

    // TIỀN TỆ MỚI: Mảnh Cổ Tự (Rune Shards)
    public int runeShards = 0;

    // Character
    public CharacterUnlockData characterData = new CharacterUnlockData();

    // Achievement
    public List<AchievementData> achievements = new List<AchievementData>();

    // Rune Inventory
    public List<RuneData> runes = new List<RuneData>();

    public bool isTutorialCompleted = false;

    public List<InventoryItemData> inventoryItems = new List<InventoryItemData>();
    public int totalKills = 0; 
    public int totalRuns = 0;

    // Constructor đảm bảo không bao giờ bị Null khi tài khoản khởi tạo mới
    public GameSaveData()
    {
        gem = 3000;
        accountLevel = 1;
        accountExp = 0;
        lifetimeGemEarned = 0;
        runeShards = 0;
        isTutorialCompleted = false;

        if (characterData == null) characterData = new CharacterUnlockData();
        if (achievements == null) achievements = new List<AchievementData>();
        if (runes == null) runes = new List<RuneData>();
        if (inventoryItems == null) inventoryItems = new List<InventoryItemData>();
    }
}