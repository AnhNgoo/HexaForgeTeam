using System.Collections.Generic;
using UnityEngine;

public class AccountLevelManager : MonoBehaviour
{
    public static AccountLevelManager Instance;

    private AccountLevelData accountData = new AccountLevelData();
    private const int MAX_LEVEL = 30;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        LoadData();
    }

    private void Start()
    {
        UpdateUI();
    }

    public void AddExp(int amount)
    {
        if (accountData.level >= MAX_LEVEL) return;

        accountData.currentExp += amount;

        while (accountData.level < MAX_LEVEL && accountData.currentExp >= GetRequiredExp(accountData.level))
        {
            accountData.currentExp -= GetRequiredExp(accountData.level);
            LevelUp();
        }

        if (AchievementManager.Instance != null)
        {
            AchievementManager.Instance.SetLevelProgress(accountData.level);
        }

        SaveData();
        UpdateUI();
    }

    private void LevelUp()
    {
        int oldLevel = accountData.level;
        accountData.level++;

        List<CostData> rewards = new List<CostData>();
        string unlockText = "";

        // ===== 1. CÂN BẰNG PHẦN THƯỞNG GEM & ITEM THEO CỘT MỐC =====
        int gemReward = 50 + (accountData.level * 20); // Phân bổ Gems tăng dần nhẹ nhàng
        rewards.Add(new CostData("GEM", gemReward));

        if (GemManager.Instance != null)
        {
            GemManager.Instance.AddGem(gemReward);
        }

        // Tặng Vé Gacha mỗi 5 Level
        if (accountData.level % 5 == 0)
        {
            rewards.Add(new CostData("GACHA_TICKET_01", 1));
            if (InventoryItemManager.Instance != null)
            {
                InventoryItemManager.Instance.AddItem("GACHA_TICKET_01", "Gacha Ticket", 1);
            }
        }

        // Tặng Bùa Fusion / Cuộn Reroll ở các mốc Level đặc biệt
        if (accountData.level == 10 || accountData.level == 20)
        {
            rewards.Add(new CostData("FUSION_CHARM_01", 1));
            if (InventoryItemManager.Instance != null)
            {
                InventoryItemManager.Instance.AddItem("FUSION_CHARM_01", "Protection Charm", 1);
            }
        }
        else if (accountData.level == 30)
        {
            rewards.Add(new CostData("REROLL_SCROLL_01", 2));
            if (InventoryItemManager.Instance != null)
            {
                InventoryItemManager.Instance.AddItem("REROLL_SCROLL_01", "Reroll Scroll", 2);
            }
        }

        // ===== 2. MỞ KHÓA TƯỚNG =====
        if (accountData.level == 5)
        {
            rewards.Add(new CostData("CHAR_LYRA", 1));
            unlockText = "\n<color=#00FFCC>Unlocked New Hero: Lyra!</color>";
        }
        else if (accountData.level == 15)
        {
            rewards.Add(new CostData("CHAR_ARES", 1));
            unlockText = "\n<color=#00FFCC>Unlocked New Hero: Ares!</color>";
        }
        else if (accountData.level == 25)
        {
            rewards.Add(new CostData("CHAR_ELARA", 1));
            unlockText = "\n<color=#00FFCC>Unlocked New Hero: Elara!</color>";
        }

        if (CharacterManager.Instance != null)
        {
            CharacterManager.Instance.CheckUnlockCharacter();
        }

        if (LobbyStatManager.Instance != null)
        {
            LobbyStatManager.Instance.RecalculateStats();
        }

        ShowLevelUpPopup(oldLevel, accountData.level, rewards, unlockText);
    }

    private int GetRequiredExp(int level)
    {
        return 100 + ((level - 1) * 50);
    }

    private void UpdateUI()
    {
        if (LobbyHUDTopBar.Instance != null)
        {
            LobbyHUDTopBar.Instance.RefreshLevelUI(
                accountData.level,
                accountData.currentExp,
                GetRequiredExp(accountData.level));
        }
    }

    private void ShowLevelUpPopup(int oldLevel, int newLevel, List<CostData> rewards, string unlockText)
    {
        if (LevelUpPopupUI.Instance == null) return;

        string bonusText = $"<color=#FFD700>Stat Bonus:</color> Max HP +10 | ATK +1" + unlockText;
        LevelUpPopupUI.Instance.Show("LEVEL UP!", oldLevel, newLevel, rewards, bonusText);
    }

    public int GetLevel() => accountData.level;
    public int GetHPBonus() => (accountData.level - 1) * 10;
    public int GetATKBonus() => accountData.level - 1;

    private void SaveData()
    {
        if (SaveLoadManager.Instance != null && SaveLoadManager.Instance.SaveData != null)
        {
            SaveLoadManager.Instance.SaveData.accountLevel = accountData.level;
            SaveLoadManager.Instance.SaveData.accountExp = accountData.currentExp;
            SaveLoadManager.Instance.SaveGame();

            if (PlayFabDataManager.Instance != null)
            {
                PlayFabDataManager.Instance.MarkDirty();

                if (LeaderboardManager.Instance != null)
                {
                    LeaderboardManager.Instance.UpdatePowerScore();
                }
            }
        }
    }

    private void LoadData()
    {
        if (SaveLoadManager.Instance != null && SaveLoadManager.Instance.SaveData != null)
        {
            accountData.level = SaveLoadManager.Instance.SaveData.accountLevel;
            accountData.currentExp = SaveLoadManager.Instance.SaveData.accountExp;
        }
    }

    public void ResetLevelData()
    {
        accountData = new AccountLevelData();
        UpdateUI();
        SaveData();
    }
}