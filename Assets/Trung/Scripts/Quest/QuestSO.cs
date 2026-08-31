using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewQuestData", menuName = "Quest System/Create Quest Data")]
public class QuestSO : ScriptableObject
{
    [Header("1. Basic Information")]
    public string questID;
    public string questTitle;
    [TextArea(2, 4)] public string questDescription;
    public QuestType questType = QuestType.Tutorial;

    [Header("2. Unlock Requirements")]
    public int requiredAccountLevel = 1;
    public string requiredPrerequisiteQuestID = "";

    [Header("3. Target Progress")]
    public int targetProgress = 1;

    [Header("4. Quest Target Station Point Name (Optional)")]
    [Tooltip("Tên GameObject Point trên Hierarchy mà NPC sẽ di chuyển đến khi làm quest này (ví dụ: 'Point Khoa' hoặc 'Point Rune')")]
    public string targetStationPointName = "";

    [Header("5. NPC Dialogues Setup (Multi-Speaker)")]
    public string npcName = "Guide";
    public List<DialogueLine> notStartedDialogues = new List<DialogueLine>();
    public List<DialogueLine> inProgressDialogues = new List<DialogueLine>();
    public List<DialogueLine> canClaimDialogues = new List<DialogueLine>();
    public List<DialogueLine> completedDialogues = new List<DialogueLine>();

    [Header("6. Quest Rewards")]
    public int rewardGem = 0;
    public int rewardShard = 0;
    public int rewardExp = 0;
    public List<CostData> rewardItems = new List<CostData>();
}