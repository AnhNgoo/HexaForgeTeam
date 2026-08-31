using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    public const string TALK_ALL_QUEST_ID = "QUEST_TUTORIAL_TALK_ALL";

    [Header("Quest Database Reference")]
    [SerializeField] private QuestDatabaseSO questDatabase;

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

            // Bắn Notify nhận nhiệm vụ mới
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

            // Nếu vừa hoàn thành đủ chỉ tiêu tiến độ -> Báo Notify cho người chơi quay lại NPC nhận thưởng
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

        // Bắn Notify nhận thưởng thành công
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
                // Mở khóa cổng vào Run khi đã nhận hoặc hoàn thành Quest gộp này
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
    }
}