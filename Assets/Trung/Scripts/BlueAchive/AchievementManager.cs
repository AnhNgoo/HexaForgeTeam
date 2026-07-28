using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance;

    [Header("UI")]
    [SerializeField] private GameObject achievementPanel;
    [SerializeField] private Transform contentParent;
    [SerializeField] private AchievementCardUI cardPrefab;
    [SerializeField] private AchievementToastUI toastUI;
    [SerializeField] private Button claimAllButton;

    private GameObject defaultAchievementPanel;
    private Transform defaultContentParent;
    private AchievementCardUI defaultCardPrefab;
    private AchievementToastUI defaultToastUI;

    private bool defaultUICached;

    private List<AchievementData> achievements = new List<AchievementData>();
    private float lastSaveTime = -99f;

    private const string Lvl2ID = "LVL_2";
    private const string Lvl5ID = "LVL_5";
    private const string Lvl10ID = "LVL_10";
    private const string Lvl15ID = "LVL_15";
    private const string Lvl20ID = "LVL_20";
    private const string Lvl25ID = "LVL_25";
    private const string Lvl30ID = "LVL_30";

    private const string Roll1ID = "ROLL_1";
    private const string Roll10ID = "ROLL_10";
    private const string Roll30ID = "ROLL_30";
    private const string Roll50ID = "ROLL_50";
    private const string Roll100ID = "ROLL_100";
    private const string Legendary1ID = "LEGENDARY_1";
    private const string Legendary5ID = "LEGENDARY_5";
    private const string Legendary10ID = "LEGENDARY_10";

    private const string Kill20ID = "KILL_20";
    private const string Kill100ID = "KILL_100";
    private const string Kill300ID = "KILL_300";
    private const string Kill500ID = "KILL_500";
    private const string Kill1000ID = "KILL_1000";
    private const string Boss1ID = "BOSS_1";
    private const string Boss5ID = "BOSS_5";
    private const string Boss10ID = "BOSS_10";

    private const string Fuse1ID = "FUSE_1";
    private const string Fuse5ID = "FUSE_5";
    private const string Fuse10ID = "FUSE_10";
    private const string EquipFull3ID = "EQUIP_FULL_3";
    private const string Reroll1ID = "REROLL_1";
    private const string Dismantle10ID = "DISMANTLE_10";

    private const string MasterAchievementID = "MASTER_ACHIEVEMENT";
    private const string UltimateRuneName = "Origin of Creation";

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

        CreateDefaultAchievements();
        LoadAchievement();
        CacheDefaultUI();
    }

    private void Start()
    {
        if (claimAllButton != null)
        {
            claimAllButton.onClick.RemoveAllListeners();
            claimAllButton.onClick.AddListener(ClaimAllRewards);
        }

        RefreshUI();
    }

    #region Create Default Achievements

    private void CreateDefaultAchievements()
    {
        if (achievements.Count > 0) return;

        achievements.Add(new AchievementData(Lvl2ID, "New Traveler", "Reach Account Level 2", 2, 100, 0, new List<CostData> { new CostData("GACHA_TICKET_01", 1) }));
        achievements.Add(new AchievementData(Lvl5ID, "First Steps", "Reach Account Level 5", 5, 250, 0, new List<CostData> { new CostData("GACHA_TICKET_01", 1) }));
        achievements.Add(new AchievementData(Lvl10ID, "Rising Hero", "Reach Account Level 10", 10, 500, 0, new List<CostData> { new CostData("REROLL_SCROLL_01", 1) }));
        achievements.Add(new AchievementData(Lvl15ID, "Seasoned Warrior", "Reach Account Level 15", 15, 750, 0, new List<CostData> { new CostData("FUSION_CHARM_01", 1) }));
        achievements.Add(new AchievementData(Lvl20ID, "Veteran Adventurer", "Reach Account Level 20", 20, 1000, 0, new List<CostData> { new CostData("GACHA_TICKET_01", 2) }));
        achievements.Add(new AchievementData(Lvl25ID, "Grand Commander", "Reach Account Level 25", 25, 1500, 0, new List<CostData> { new CostData("FUSION_CHARM_01", 2) }));
        achievements.Add(new AchievementData(Lvl30ID, "Legend of the Realm", "Reach Account Level 30", 30, 2000, 0, new List<CostData> { new CostData("GACHA_TICKET_01", 3), new CostData("REROLL_SCROLL_01", 2) }));

        achievements.Add(new AchievementData(Roll1ID, "First Fortune", "Roll 1 Time", 1, 60, 50));
        achievements.Add(new AchievementData(Roll10ID, "First Gambler", "Roll 10 Times", 10, 200, 0, new List<CostData> { new CostData("GACHA_TICKET_01", 1) }));
        achievements.Add(new AchievementData(Roll30ID, "Lucky Seeker", "Roll 30 Times", 30, 500, 0, new List<CostData> { new CostData("GACHA_TICKET_01", 2) }));
        achievements.Add(new AchievementData(Roll50ID, "Rune Enthusiast", "Roll 50 Times", 50, 800, 0, new List<CostData> { new CostData("FUSION_CHARM_01", 1) }));
        achievements.Add(new AchievementData(Roll100ID, "Master Summoner", "Roll 100 Times", 100, 1500, 0, new List<CostData> { new CostData("GACHA_TICKET_01", 3) }));
        achievements.Add(new AchievementData(Legendary1ID, "Golden Touch", "Obtain 1 Legendary Rune", 1, 300, 0, new List<CostData> { new CostData("REROLL_SCROLL_01", 1) }));
        achievements.Add(new AchievementData(Legendary5ID, "Rune Hunter", "Obtain 5 Legendary Runes", 5, 1000, 0, new List<CostData> { new CostData("FUSION_CHARM_01", 1) }));
        achievements.Add(new AchievementData(Legendary10ID, "Mythic Collector", "Obtain 10 Legendary Runes", 10, 2000, 0, new List<CostData> { new CostData("REROLL_SCROLL_01", 2) }));

        achievements.Add(new AchievementData(Kill20ID, "First Blood", "Defeat 20 Monsters", 20, 100, 100));
        achievements.Add(new AchievementData(Kill100ID, "Monster Slayer", "Defeat 100 Monsters", 100, 300, 0, new List<CostData> { new CostData("GACHA_TICKET_01", 1) }));
        achievements.Add(new AchievementData(Kill300ID, "Dungeon Cleaner", "Defeat 300 Monsters", 300, 600, 0, new List<CostData> { new CostData("REROLL_SCROLL_01", 1) }));
        achievements.Add(new AchievementData(Kill500ID, "Executioner", "Defeat 500 Monsters", 500, 1000, 0, new List<CostData> { new CostData("FUSION_CHARM_01", 1) }));
        achievements.Add(new AchievementData(Kill1000ID, "Fiend Nemesis", "Defeat 1,000 Monsters", 1000, 2000, 0, new List<CostData> { new CostData("GACHA_TICKET_01", 2) }));
        achievements.Add(new AchievementData(Boss1ID, "Boss Crusher", "Defeat 1 Boss", 1, 200, 0, new List<CostData> { new CostData("GACHA_TICKET_01", 1) }));
        achievements.Add(new AchievementData(Boss5ID, "Boss Hunter", "Defeat 5 Bosses", 5, 800, 0, new List<CostData> { new CostData("FUSION_CHARM_01", 1) }));
        achievements.Add(new AchievementData(Boss10ID, "Dungeon Dominator", "Defeat 10 Bosses", 10, 1800, 0, new List<CostData> { new CostData("FUSION_CHARM_01", 2) }));

        achievements.Add(new AchievementData(Fuse1ID, "Alchemist Apprentice", "Fuse Runes 1 Time", 1, 200, 0, new List<CostData> { new CostData("FUSION_CHARM_01", 1) }));
        achievements.Add(new AchievementData(Fuse5ID, "Forge Enthusiast", "Fuse Runes 5 Times", 5, 600, 0, new List<CostData> { new CostData("FUSION_CHARM_01", 1) }));
        achievements.Add(new AchievementData(Fuse10ID, "Master Transmuter", "Fuse Runes 10 Times", 10, 1200, 0, new List<CostData> { new CostData("FUSION_CHARM_01", 2) }));
        achievements.Add(new AchievementData(EquipFull3ID, "Power Unleashed", "Equip 3 Runes on a Hero", 3, 300, 0, new List<CostData> { new CostData("GACHA_TICKET_01", 1) }));
        achievements.Add(new AchievementData(Reroll1ID, "Affinities Tinkerer", "Reroll Rune Affix 1 Time", 1, 150, 0, new List<CostData> { new CostData("REROLL_SCROLL_01", 1) }));
        achievements.Add(new AchievementData(Dismantle10ID, "Scrap Recycler", "Dismantle 10 Runes", 10, 250, 300));

        achievements.Add(new AchievementData(MasterAchievementID, "Master of Achievements", "Complete all other achievements", 1, 0, 0));
    }

    #endregion

    #region Progress

    public void AddRollProgress(int amount)
    {
        AddGenericProgress(Roll1ID, amount);
        AddGenericProgress(Roll10ID, amount);
        AddGenericProgress(Roll30ID, amount);
        AddGenericProgress(Roll50ID, amount);
        AddGenericProgress(Roll100ID, amount);
    }

    public void AddLegendaryProgress(int amount)
    {
        AddGenericProgress(Legendary1ID, amount);
        AddGenericProgress(Legendary5ID, amount);
        AddGenericProgress(Legendary10ID, amount);
    }

    public void SetLevelProgress(int level)
    {
        UpdateProgressDirect(Lvl2ID, level);
        UpdateProgressDirect(Lvl5ID, level);
        UpdateProgressDirect(Lvl10ID, level);
        UpdateProgressDirect(Lvl15ID, level);
        UpdateProgressDirect(Lvl20ID, level);
        UpdateProgressDirect(Lvl25ID, level);
        UpdateProgressDirect(Lvl30ID, level);
    }

    public void AddKillProgress(int kills, int bosses)
    {
        if (kills > 0)
        {
            AddGenericProgress(Kill20ID, kills);
            AddGenericProgress(Kill100ID, kills);
            AddGenericProgress(Kill300ID, kills);
            AddGenericProgress(Kill500ID, kills);
            AddGenericProgress(Kill1000ID, kills);
        }

        if (bosses > 0)
        {
            AddGenericProgress(Boss1ID, bosses);
            AddGenericProgress(Boss5ID, bosses);
            AddGenericProgress(Boss10ID, bosses);
        }
    }

    public void AddFusionProgress(int amount = 1)
    {
        AddGenericProgress(Fuse1ID, amount);
        AddGenericProgress(Fuse5ID, amount);
        AddGenericProgress(Fuse10ID, amount);
    }

    public void AddRerollProgress(int amount = 1)
    {
        AddGenericProgress(Reroll1ID, amount);
    }

    public void AddDismantleProgress(int amount = 1)
    {
        AddGenericProgress(Dismantle10ID, amount);
    }

    public void CheckEquipFullProgress(int equippedCount)
    {
        if (equippedCount >= 3)
        {
            UpdateProgressDirect(EquipFull3ID, 3);
        }
    }

    private void AddGenericProgress(string id, int amount)
    {
        AchievementData achievement = GetAchievement(id);
        if (achievement == null) return;
        bool wasCompleted = achievement.isCompleted;
        achievement.AddProgress(amount);
        CheckComplete(achievement, wasCompleted);
        SaveAchievement();
    }

    private void UpdateProgressDirect(string id, int value)
    {
        AchievementData achievement = GetAchievement(id);
        if (achievement == null || achievement.isCompleted) return;
        bool wasCompleted = achievement.isCompleted;
        if (value >= achievement.targetProgress)
        {
            achievement.currentProgress = achievement.targetProgress;
            achievement.isCompleted = true;
        }
        else
        {
            achievement.currentProgress = value;
        }
        CheckComplete(achievement, wasCompleted);
        SaveAchievement();
    }

    #endregion

    #region Complete

    private void CheckComplete(AchievementData achievement, bool wasCompleted)
    {
        if (wasCompleted || !achievement.isCompleted) return;

        if (toastUI != null)
        {
            toastUI.ShowToast("Achievement Unlocked", achievement.title);
        }

        RefreshUI();
        CheckMasterAchievement();

        if (LeaderboardManager.Instance != null)
        {
            LeaderboardManager.Instance.UpdatePowerScore();
        }
    }

    #endregion

    #region UI

    public void OpenPanel()
    {
        if (achievementPanel != null)
        {
            achievementPanel.SetActive(true);
        }

        RefreshUI();
    }

    public void ClosePanel()
    {
        if (achievementPanel != null)
        {
            achievementPanel.SetActive(false);
        }
    }

    public bool HasAnyClaimableAchievement()
    {
        for (int i = 0; i < achievements.Count; i++)
        {
            if (achievements[i] != null && achievements[i].isCompleted && !achievements[i].isClaimed)
            {
                return true;
            }
        }
        return false;
    }

    public void RefreshUI()
    {
        if (claimAllButton != null)
        {
            claimAllButton.interactable = HasAnyClaimableAchievement();
        }

        if (contentParent == null) return;

        for (int i = contentParent.childCount - 1; i >= 0; i--)
        {
            Destroy(contentParent.GetChild(i).gameObject);
        }

        List<AchievementData> sortedList = new List<AchievementData>(achievements);
        sortedList.Sort((a, b) =>
        {
            int GetPriority(AchievementData data)
            {
                if (data.isCompleted && !data.isClaimed) return 0;
                if (!data.isCompleted) return 1;
                return 2;
            }

            int priorityA = GetPriority(a);
            int priorityB = GetPriority(b);

            if (priorityA != priorityB)
                return priorityA.CompareTo(priorityB);

            return 0;
        });

        for (int i = 0; i < sortedList.Count; i++)
        {
            AchievementCardUI card = Instantiate(cardPrefab, contentParent);
            card.Setup(sortedList[i]);
            
            card.transform.localScale = Vector3.one * 0.9f;
            card.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack);
        }
    }

    #endregion

    #region Get

    private AchievementData GetAchievement(string achievementID)
    {
        for (int i = 0; i < achievements.Count; i++)
        {
            if (achievements[i].achievementID == achievementID)
            {
                return achievements[i];
            }
        }

        return null;
    }

    #endregion

    #region Save

    public void SaveAchievement()
    {
        if (SaveLoadManager.Instance != null && SaveLoadManager.Instance.SaveData != null)
        {
            SaveLoadManager.Instance.SaveData.achievements = achievements;
            
            // ===== CHẶN SPAM SAVE PLAYFAB THỜI GIAN NGẮN =====
            if (Time.time - lastSaveTime > 2.0f)
            {
                lastSaveTime = Time.time;
                SaveLoadManager.Instance.SaveGame();
            }
        }
    }

    public void LoadAchievement()
    {
        if (SaveLoadManager.Instance == null) return;

        if (SaveLoadManager.Instance.SaveData.achievements == null || SaveLoadManager.Instance.SaveData.achievements.Count == 0)
        {
            SaveAchievement();
            return;
        }

        achievements = SaveLoadManager.Instance.SaveData.achievements;
    }

    #endregion

    private bool HasUltimateRune()
    {
        if (RuneInventoryManager.Instance == null) return false;

        for (int i = 0; i < RuneInventoryManager.Instance.runes.Count; i++)
        {
            RuneData rune = RuneInventoryManager.Instance.runes[i];
            if (rune.runeName == UltimateRuneName) return true;
        }

        return false;
    }

    public void CheckUltimateRuneReward()
    {
        AchievementData master = GetAchievement(MasterAchievementID);

        if (master == null || !master.isClaimed || HasUltimateRune()) return;

        GiveUltimateRune();
    }

    private void GiveUltimateRune()
    {
        RuneData rune = new RuneData(RuneColor.Red, RuneRarity.Legendary);

        rune.ignoreHardCap = true;
        rune.runeName = UltimateRuneName;
        rune.runeLore = "The final proof that nothing remains unconquered.";
        rune.affixes.Add(new RuneAffixData(RuneStatType.AllStats, 99999));

        RuneInventoryManager.Instance.AddRune(rune);

        Debug.Log("ULTIMATE ORIGIN RUNE (+99999 ALL STATS) UNLOCKED!");
    }

    private void CheckMasterAchievement()
    {
        AchievementData master = GetAchievement(MasterAchievementID);

        if (master == null || master.isCompleted) return;

        for (int i = 0; i < achievements.Count; i++)
        {
            AchievementData achievement = achievements[i];

            if (achievement.achievementID == MasterAchievementID) continue;

            if (!achievement.isCompleted) return;
        }

        master.currentProgress = 1;
        master.isCompleted = true;

        SaveAchievement();
        RefreshUI();

        if (toastUI != null)
        {
            toastUI.ShowToast("Achievement Unlocked", master.title);
        }
    }

    private void CacheDefaultUI()
    {
        if (defaultUICached) return;

        defaultAchievementPanel = achievementPanel;
        defaultContentParent = contentParent;
        defaultCardPrefab = cardPrefab;
        defaultToastUI = toastUI;

        defaultUICached = true;
    }

    public void BindUI(GameObject newPanel, Transform newContentParent, AchievementCardUI newCardPrefab, AchievementToastUI newToastUI = null)
    {
        CacheDefaultUI();

        if (achievementPanel != null && achievementPanel != newPanel)
        {
            achievementPanel.SetActive(false);
        }

        achievementPanel = newPanel;
        contentParent = newContentParent;
        cardPrefab = newCardPrefab;

        if (newToastUI != null) toastUI = newToastUI;
    }

    public void RestoreDefaultUI()
    {
        if (!defaultUICached) return;

        if (achievementPanel != null && achievementPanel != defaultAchievementPanel)
        {
            achievementPanel.SetActive(false);
        }

        achievementPanel = defaultAchievementPanel;
        contentParent = defaultContentParent;
        cardPrefab = defaultCardPrefab;
        toastUI = defaultToastUI;
    }

    public void ClaimAllRewards()
    {
        if (!HasAnyClaimableAchievement()) return;

        if (claimAllButton != null)
        {
            claimAllButton.transform.DOPunchScale(new Vector3(0.15f, 0.15f, 0f), 0.35f, 8, 0.5f);
        }

        bool anyClaimed = false;
        int totalGem = 0;
        int totalShard = 0;

        for (int i = 0; i < achievements.Count; i++)
        {
            AchievementData data = achievements[i];
            if (data != null && data.isCompleted && !data.isClaimed)
            {
                data.isClaimed = true;
                anyClaimed = true;

                if (data.rewardGem > 0) totalGem += data.rewardGem;
                if (data.rewardShard > 0) totalShard += data.rewardShard;

                if (data.rewardItems != null && InventoryItemManager.Instance != null)
                {
                    for (int j = 0; j < data.rewardItems.Count; j++)
                    {
                        CostData item = data.rewardItems[j];
                        if (item != null && !string.IsNullOrEmpty(item.itemID) && item.amount > 0)
                        {
                            string itemName = item.itemID == "GACHA_TICKET_01" ? "Gacha Ticket" :
                                               item.itemID == "FUSION_CHARM_01" ? "Protection Charm" :
                                               item.itemID == "REROLL_SCROLL_01" ? "Reroll Scroll" : "Special Item";
                            InventoryItemManager.Instance.AddItem(item.itemID, itemName, item.amount);
                        }
                    }
                }
            }
        }

        if (anyClaimed)
        {
            if (totalGem > 0 && GemManager.Instance != null)
            {
                GemManager.Instance.AddGem(totalGem);
            }

            if (totalShard > 0 && RuneShardManager.Instance != null)
            {
                RuneShardManager.Instance.AddShards(totalShard);
            }

            SaveAchievement();
            RefreshUI();
            CheckUltimateRuneReward();

            if (LobbyNotifyManager.Instance != null)
            {
                LobbyNotifyManager.Instance.ShowNotify($"All available achievement rewards claimed!", Color.green);
            }
        }
    }

    public void HackUnlockAllAchievements()
    {
        for (int i = 0; i < achievements.Count; i++)
        {
            achievements[i].currentProgress = achievements[i].targetProgress;
            achievements[i].isCompleted = true;
        }

        SaveAchievement();
        RefreshUI();
        CheckMasterAchievement();

        if (LeaderboardManager.Instance != null)
        {
            LeaderboardManager.Instance.UpdatePowerScore();
        }

        Debug.Log("<color=#00FFCC><b>[CHEAT]</b> Kích hoạt Skip toàn bộ Thành Tựu thành công!</color>");
    }
}