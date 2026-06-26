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

    // Character
    public CharacterUnlockData characterData =
        new CharacterUnlockData();

    // Achievement
    public List<AchievementData> achievements =
        new List<AchievementData>();

    // Rune Inventory
    public List<RuneData> runes =
        new List<RuneData>();
}