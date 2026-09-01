using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    public const string TALK_ALL_QUEST_ID = "QUEST_TUTORIAL_TALK_ALL";
    public const string GAMBLE_QUEST_ID = "QUEST_GAMBLE_BET";

    [Header("Quest Database Reference")]
    [SerializeField] private QuestDatabaseSO questDatabase;

    [Header("Gamble Wager Config")]
    [Range(0, 100)] [SerializeField] private int gambleDoubleChance = 40; // 40% an x2
    [Range(0, 100)] [SerializeField] private int gambleTripleChance = 10; // 10% an x3

    private List<QuestData> quests = new List<QuestData>();
    private List<string> talkedNPCsInQuest = new List<string>();

    public event Action OnQuestUpdated;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        LoadQuests();
    }

    private void OnEnable()
    {
        GameEventManager.OnGachaRolled += HandleGachaRolled;
        GameEventManager.OnEnemyKilled += HandleEnemyKilled;
        GameEventManager.OnTutorialCompleted += HandleTutorialCompleted;
        GameEventManager.OnRunCompleted += HandleRunCompleted;
    }

    private void OnDisable()
    {
        GameEventManager.OnGachaRolled -= HandleGachaRolled;
        GameEventManager.OnEnemyKilled -= HandleEnemyKilled;
        GameEventManager.OnTutorialCompleted -= HandleTutorialCompleted;
        GameEventManager.OnRunCompleted -= HandleRunCompleted;
    }

    public bool IsTalkQuestActive()
    {
        QuestData talkQuest = GetQuest(TALK_ALL_QUEST_ID);
        return talkQuest != null && talkQuest.state == QuestState.InProgress;
    }

    public bool HasTalkedToNPCInQuest(string npcName)
    {
        if (string.IsNullOrEmpty(npcName)) return false;
        return talkedNPCsInQuest.Contains(npcName);
    }

    public void RegisterNPCTalk(string npcName)
    {
        if (string.IsNullOrEmpty(npcName)) return;

        QuestData talkQuest = GetQuest(TALK_ALL_QUEST_ID);
        if (talkQuest != null && talkQuest.state == QuestState.InProgress)
        {
            if (!talkedNPCsInQuest.Contains(npcName))
            {
                talkedNPCsInQuest.Add(npcName);
                AddQuestProgress(TALK_ALL_QUEST_ID, 1);
            }
        }
    }

    private void HandleGachaRolled(int count)
    {
        AddQuestProgress("QUEST_ROLL_1", count);
    }

    private void HandleEnemyKilled(int count, bool isBoss)
    {
        AddQuestProgress("QUEST_KILL_10", count);
    }

    private void HandleTutorialCompleted()
    {
        AddQuestProgress("QUEST_TUTORIAL_01", 1);
    }

    public List<QuestData> GetAllQuests() => quests;

    public QuestData GetQuest(string questID)
    {
        return quests.Find(q => q.questID == questID);
    }

    public QuestSO GetQuestSO(string questID)
    {
        if (questDatabase != null)
        {
            return questDatabase.GetQuestSO(questID);
        }
        return null;
    }

    public bool IsQuestUnlocked(QuestSO questSO)
    {
        if (questSO == null) return false;

        int currentAccountLevel = AccountLevelManager.Instance != null ? AccountLevelManager.Instance.GetLevel() : 1;
        if (currentAccountLevel < questSO.requiredAccountLevel)
        {
            return false;
        }

        if (!string.IsNullOrEmpty(questSO.requiredPrerequisiteQuestID))
        {
            QuestData prereq = GetQuest(questSO.requiredPrerequisiteQuestID);
            if (prereq == null || prereq.state != QuestState.Completed)
            {
                return false;
            }
        }

        return true;
    }

    public void StartQuest(string questID)
    {
        QuestData quest = GetQuest(questID);
        QuestSO questSO = GetQuestSO(questID);

        if (quest != null && quest.state == QuestState.NotStarted)
        {
            quest.state = QuestState.InProgress;
            quest.currentProgress = 0;
            if (questSO != null)
            {
                quest.targetProgress = questSO.targetProgress;
            }

            if (questID == TALK_ALL_QUEST_ID)
            {
                talkedNPCsInQuest.Clear();
            }

            if (LobbyNotifyManager.Instance != null)
            {
                string title = questSO != null ? questSO.questTitle : quest.title;
                LobbyNotifyManager.Instance.ShowNotify($"Quest Accepted: {title}", new Color(1f, 0.85f, 0.2f));
            }

            SaveQuests();
            OnQuestUpdated?.Invoke();
        }
    }

    public void AddQuestProgress(string questID, int amount)
    {
        QuestData quest = GetQuest(questID);
        QuestSO questSO = GetQuestSO(questID);

        if (quest != null && quest.state == QuestState.InProgress)
        {
            if (questSO != null)
            {
                quest.targetProgress = questSO.targetProgress;
            }

            quest.AddProgress(amount);

            if (quest.state == QuestState.CanClaim)
            {
                if (LobbyNotifyManager.Instance != null)
                {
                    string title = questSO != null ? questSO.questTitle : quest.title;
                    LobbyNotifyManager.Instance.ShowNotify($"Quest Completed: {title}! Return to NPC for reward.", Color.cyan);
                }
            }

            SaveQuests();
            OnQuestUpdated?.Invoke();
        }
    }

    public void ClaimQuest(string questID)
    {
        QuestData quest = GetQuest(questID);
        if (quest == null || quest.state != QuestState.CanClaim) return;

        quest.state = QuestState.Completed;

        QuestSO questSO = GetQuestSO(questID);

        int gemToGive = questSO != null ? questSO.rewardGem : quest.rewardGem;
        int shardToGive = questSO != null ? questSO.rewardShard : quest.rewardShard;
        int expToGive = questSO != null ? questSO.rewardExp : 0;
        List<CostData> itemsToGive = questSO != null ? questSO.rewardItems : quest.rewardItems;

        if (gemToGive > 0 && GemManager.Instance != null)
        {
            GemManager.Instance.AddGem(gemToGive);
        }

        if (shardToGive > 0 && RuneShardManager.Instance != null)
        {
            RuneShardManager.Instance.AddShards(shardToGive);
        }

        if (expToGive > 0 && AccountLevelManager.Instance != null)
        {
            AccountLevelManager.Instance.AddExp(expToGive);
        }

        if (itemsToGive != null && InventoryItemManager.Instance != null)
        {
            for (int i = 0; i < itemsToGive.Count; i++)
            {
                CostData item = itemsToGive[i];
                if (item != null && !string.IsNullOrEmpty(item.itemID) && item.amount > 0)
                {
                    InventoryItemManager.Instance.AddItem(item.itemID, item.itemID, item.amount);
                }
            }
        }

        if (LobbyNotifyManager.Instance != null)
        {
            string title = questSO != null ? questSO.questTitle : quest.title;
            LobbyNotifyManager.Instance.ShowNotify($"Reward Claimed for [{title}]!", Color.green);
        }

        SaveQuests();
        OnQuestUpdated?.Invoke();
    }

    private void SyncFromDatabase()
    {
        if (questDatabase == null || questDatabase.allQuests == null) return;

        List<QuestData> syncedList = new List<QuestData>();

        foreach (var questSO in questDatabase.allQuests)
        {
            if (questSO == null || string.IsNullOrEmpty(questSO.questID)) continue;

            QuestData existingQuest = quests.Find(q => q.questID == questSO.questID);
            if (existingQuest == null)
            {
                QuestData newQuest = new QuestData(
                    questSO.questID,
                    questSO.questTitle,
                    questSO.questDescription,
                    questSO.questType,
                    questSO.targetProgress,
                    questSO.rewardGem,
                    questSO.rewardShard,
                    questSO.rewardItems
                );
                if (questSO.questID == GAMBLE_QUEST_ID)
                {
                    newQuest.state = QuestState.Completed;
                }
                syncedList.Add(newQuest);
            }
            else
            {
                existingQuest.title = questSO.questTitle;
                existingQuest.description = questSO.questDescription;
                existingQuest.targetProgress = questSO.targetProgress;
                existingQuest.rewardGem = questSO.rewardGem;
                existingQuest.rewardShard = questSO.rewardShard;
                existingQuest.rewardItems = questSO.rewardItems;
                syncedList.Add(existingQuest);
            }
        }

        quests = syncedList;
    }

    public void SaveQuests()
    {
        if (SaveLoadManager.Instance != null && SaveLoadManager.Instance.SaveData != null)
        {
            SaveLoadManager.Instance.SaveData.quests = quests;
            SaveLoadManager.Instance.SaveGame();

            if (PlayFabDataManager.Instance != null)
            {
                PlayFabDataManager.Instance.MarkDirty();
            }
        }
    }

    public void LoadQuests()
    {
        if (SaveLoadManager.Instance != null && SaveLoadManager.Instance.SaveData != null && SaveLoadManager.Instance.SaveData.quests != null)
        {
            quests = SaveLoadManager.Instance.SaveData.quests;
        }

        SyncFromDatabase();
        SaveQuests();
    }

    public bool IsMenuUnlocked(MenuType menuType)
    {
        switch (menuType)
        {
            case MenuType.LobbyGachaMenu:
                QuestData rollQuest = GetQuest("QUEST_ROLL_1");
                return rollQuest != null && rollQuest.state != QuestState.NotStarted;

            case MenuType.LobbyShopMenu:
                QuestData shopQuest = GetQuest("QUEST_VISIT_SHOP");
                return shopQuest != null && shopQuest.state != QuestState.NotStarted;

            case MenuType.LobbyRuneInventoryMenu:
                QuestData runeQuest = GetQuest("QUEST_OPEN_RUNE_INVENTORY");
                return runeQuest != null && runeQuest.state != QuestState.NotStarted;

            case MenuType.LobbyCharacterMenu:
                QuestData charQuest = GetQuest("QUEST_SELECT_CHARACTER");
                return charQuest != null && charQuest.state != QuestState.NotStarted;

            case MenuType.LobbyBossSelectMenu:
                QuestData bossQuest = GetQuest("QUEST_EXPEDITION_TRIAL");
                return bossQuest != null && bossQuest.state != QuestState.NotStarted;

            case MenuType.LobbyAchievementMenu:
                QuestData achQuest = GetQuest("QUEST_CHECK_ACHIEVEMENTS");
                return achQuest != null && achQuest.state != QuestState.NotStarted;

            case MenuType.LobbyLeaderboardMenu:
                QuestData leadQuest = GetQuest("QUEST_CHECK_LEADERBOARDS");
                return leadQuest != null && leadQuest.state != QuestState.NotStarted;

            default:
                return true;
        }
    }

    private void HandleRunCompleted(bool isVictory)
    {
        AddQuestProgress("QUEST_EXPEDITION_TRIAL", 1);

        QuestData gambleQuest = GetQuest(GAMBLE_QUEST_ID);
        if (gambleQuest != null)
        {
            gambleQuest.state = QuestState.InProgress;
            gambleQuest.currentProgress = 0;
            SaveQuests();
            OnQuestUpdated?.Invoke();
        }
    }

    public void ExecuteGambleBet(int betAmount)
    {
        if (GemManager.Instance == null) return;

        int currentGems = (SaveLoadManager.Instance != null && SaveLoadManager.Instance.SaveData != null)
            ? SaveLoadManager.Instance.SaveData.gem
            : 0;

        if (currentGems < betAmount)
        {
            if (LobbyNotifyManager.Instance != null)
            {
                LobbyNotifyManager.Instance.ShowNotify("Not enough Gems to place this wager!", Color.red);
            }
            return;
        }

        GemManager.Instance.SpendGem(betAmount);

        int roll = UnityEngine.Random.Range(1, 101);

        if (roll <= gambleTripleChance)
        {
            int wonAmount = betAmount * 3;
            GemManager.Instance.AddGem(wonAmount);
            if (LobbyNotifyManager.Instance != null)
            {
                LobbyNotifyManager.Instance.ShowNotify($"JACKPOT! You won {wonAmount} Gems! (x3 Multiplier)", Color.yellow);
            }
        }
        else if (roll <= gambleTripleChance + gambleDoubleChance)
        {
            int wonAmount = betAmount * 2;
            GemManager.Instance.AddGem(wonAmount);
            if (LobbyNotifyManager.Instance != null)
            {
                LobbyNotifyManager.Instance.ShowNotify($"LUCKY! You won {wonAmount} Gems! (x2 Multiplier)", Color.green);
            }
        }
        else
        {
            if (LobbyNotifyManager.Instance != null)
            {
                LobbyNotifyManager.Instance.ShowNotify($"LOST! The Smuggler took all {betAmount} Gems. Try again next run!", Color.red);
            }
        }

        QuestData gambleQuest = GetQuest(GAMBLE_QUEST_ID);
        if (gambleQuest != null)
        {
            gambleQuest.state = QuestState.Completed;
            SaveQuests();
            OnQuestUpdated?.Invoke();
        }
    }

    public void SkipAllQuests()
    {
        if (quests == null || quests.Count == 0)
        {
            SyncFromDatabase();
        }

        for (int i = 0; i < quests.Count; i++)
        {
            QuestData qData = quests[i];
            if (qData == null) continue;

            QuestSO qSO = GetQuestSO(qData.questID);
            int target = qSO != null ? qSO.targetProgress : qData.targetProgress;

            qData.state = QuestState.Completed;
            qData.currentProgress = target;
        }

        SaveQuests();
        RefreshInteractsInScene();
        OnQuestUpdated?.Invoke();

        if (LobbyNotifyManager.Instance != null)
        {
            LobbyNotifyManager.Instance.ShowNotify("All Quests & Features Fully Unlocked!", Color.green);
        }
    }

    public bool AreSpecificQuestsCompleted(List<QuestSO> questList)
    {
        if (questList == null || questList.Count == 0) return true;

        for (int i = 0; i < questList.Count; i++)
        {
            QuestSO so = questList[i];
            if (so == null || string.IsNullOrEmpty(so.questID)) continue;

            QuestData data = GetQuest(so.questID);
            if (data == null || data.state != QuestState.Completed)
            {
                return false;
            }
        }
        return true;
    }

    public void SkipAndClaimSpecificQuests(List<QuestSO> questList)
    {
        if (questList == null || questList.Count == 0) return;

        if (quests == null || quests.Count == 0)
        {
            SyncFromDatabase();
        }

        int totalGems = 0;
        int totalShards = 0;
        int totalExp = 0;

        for (int i = 0; i < questList.Count; i++)
        {
            QuestSO qSO = questList[i];
            if (qSO == null || string.IsNullOrEmpty(qSO.questID)) continue;

            QuestData qData = GetQuest(qSO.questID);
            if (qData == null)
            {
                qData = new QuestData(
                    qSO.questID,
                    qSO.questTitle,
                    qSO.questDescription,
                    qSO.questType,
                    qSO.targetProgress,
                    qSO.rewardGem,
                    qSO.rewardShard,
                    qSO.rewardItems
                );
                quests.Add(qData);
            }

            if (qData.state != QuestState.Completed)
            {
                totalGems += qSO.rewardGem;
                totalShards += qSO.rewardShard;
                totalExp += qSO.rewardExp;

                if (qSO.rewardItems != null && InventoryItemManager.Instance != null)
                {
                    for (int j = 0; j < qSO.rewardItems.Count; j++)
                    {
                        var item = qSO.rewardItems[j];
                        if (item != null && !string.IsNullOrEmpty(item.itemID) && item.amount > 0)
                        {
                            InventoryItemManager.Instance.AddItem(item.itemID, item.itemID, item.amount);
                        }
                    }
                }
            }

            qData.state = QuestState.Completed;
            qData.currentProgress = qSO.targetProgress;
        }

        if (totalGems > 0 && GemManager.Instance != null)
        {
            GemManager.Instance.AddGem(totalGems);
        }
        if (totalShards > 0 && RuneShardManager.Instance != null)
        {
            RuneShardManager.Instance.AddShards(totalShards);
        }
        if (totalExp > 0 && AccountLevelManager.Instance != null)
        {
            AccountLevelManager.Instance.AddExp(totalExp);
        }

        SaveQuests();
        RefreshInteractsInScene();
        OnQuestUpdated?.Invoke();

        if (LobbyNotifyManager.Instance != null)
        {
            LobbyNotifyManager.Instance.ShowNotify($"Tutorial Skipped! +{totalGems} Gems, +{totalShards} Shards", Color.green);
        }
    }

    private void RefreshInteractsInScene()
    {
        InteractV2[] allInteracts = FindObjectsByType<InteractV2>(FindObjectsSortMode.None);
        for (int i = 0; i < allInteracts.Length; i++)
        {
            if (allInteracts[i] != null)
            {
                allInteracts[i].CheckFeatureUnlockStatus();
            }
        }

        if (InteractManagerV2.Instance != null)
        {
            InteractManagerV2.Instance.RescanNearbyInteracts();
        }
    }
}