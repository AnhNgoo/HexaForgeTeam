using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NPCDialogue))]
public class NPCQuestHandler : MonoBehaviour
{
    [Header("Quest Station Point (Vị trí gốc đứng giao quest)")]
    [SerializeField] private Transform questStationPoint;

    [Header("World Name Tag Reference")]
    [SerializeField] private WorldNameTag worldNameTag;

    [Header("First Intro Dialogue")]
    [SerializeField] private List<DialogueLine> firstIntroQuestDialogues = new List<DialogueLine>();

    [Header("Assigned Quests (In Priority Order)")]
    [SerializeField] private List<QuestSO> assignedQuests = new List<QuestSO>();

    private NPCDialogue defaultNPCDialogue;
    private float visualCheckTimer = 0f;

    private void Awake()
    {
        defaultNPCDialogue = GetComponent<NPCDialogue>();
        if (worldNameTag == null)
        {
            worldNameTag = GetComponentInChildren<WorldNameTag>();
        }
    }

    private void Start()
    {
        StartCoroutine(SyncVisualsRoutine());
    }

    private void Update()
    {
        visualCheckTimer += Time.deltaTime;
        if (visualCheckTimer >= 0.5f)
        {
            visualCheckTimer = 0f;
            RefreshQuestVisuals();
        }
    }

    private IEnumerator SyncVisualsRoutine()
    {
        yield return null;
        RefreshQuestVisuals();
        yield return new WaitForSeconds(0.2f);
        RefreshQuestVisuals();
        yield return new WaitForSeconds(0.5f);
        RefreshQuestVisuals();
    }

    private void OnEnable()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestUpdated += RefreshQuestVisuals;
        }
        RefreshQuestVisuals();
    }

    private void OnDisable()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestUpdated -= RefreshQuestVisuals;
        }
    }

    public Transform GetQuestStationPoint()
    {
        QuestState state;
        QuestSO activeSO = GetCurrentActiveQuestSO(out state);

        // Chỉ khi ĐÃ NHẬN QUEST (InProgress hoặc CanClaim) thì mới chạy đến điểm đích targetStationPointName
        if (activeSO != null && (state == QuestState.InProgress || state == QuestState.CanClaim))
        {
            if (!string.IsNullOrEmpty(activeSO.targetStationPointName))
            {
                Transform foundPoint = FindPointRecursive(activeSO.targetStationPointName);
                if (foundPoint != null)
                {
                    return foundPoint;
                }
            }
        }

        // Khi CHƯA NHẬN QUEST (NotStarted) hoặc không có điểm đích -> Đứng tại điểm gốc của NPC
        return questStationPoint;
    }

    private Transform FindPointRecursive(string pointName)
    {
        GameObject directObj = GameObject.Find(pointName);
        if (directObj != null) return directObj.transform;

        Transform[] allTransforms = Resources.FindObjectsOfTypeAll<Transform>();
        for (int i = 0; i < allTransforms.Length; i++)
        {
            Transform t = allTransforms[i];
            if (t != null && t.gameObject.scene.isLoaded && t.name.Equals(pointName, System.StringComparison.OrdinalIgnoreCase))
            {
                return t;
            }
        }

        return null;
    }

    public void RefreshQuestVisuals()
    {
        if (worldNameTag == null)
        {
            worldNameTag = GetComponentInChildren<WorldNameTag>();
        }

        if (worldNameTag == null) return;

        QuestState state;
        QuestSO activeSO = GetCurrentActiveQuestSO(out state);

        if (activeSO != null)
        {
            if (state == QuestState.NotStarted || state == QuestState.CanClaim)
            {
                worldNameTag.UpdateQuestIcon(state);
            }
            else
            {
                worldNameTag.HideQuestIcon();
            }
        }
        else
        {
            worldNameTag.HideQuestIcon();
        }
    }

    public bool HasAvailableOrActiveQuest()
    {
        QuestState state;
        QuestSO activeSO = GetCurrentActiveQuestSO(out state);
        if (activeSO == null) return false;

        return state == QuestState.NotStarted || state == QuestState.InProgress || state == QuestState.CanClaim;
    }

    public bool ShouldStandAtStation()
    {
        QuestState state;
        QuestSO activeSO = GetCurrentActiveQuestSO(out state);

        if (activeSO == null) return false;

        return state == QuestState.NotStarted || state == QuestState.InProgress || state == QuestState.CanClaim;
    }

    public QuestSO GetCurrentActiveQuestSO(out QuestState currentState)
    {
        currentState = QuestState.Completed;

        if (QuestManager.Instance == null || assignedQuests == null || assignedQuests.Count == 0) return null;

        for (int i = 0; i < assignedQuests.Count; i++)
        {
            QuestSO qSO = assignedQuests[i];
            if (qSO == null) continue;

            if (!QuestManager.Instance.IsQuestUnlocked(qSO)) continue;

            QuestData qData = QuestManager.Instance.GetQuest(qSO.questID);
            if (qData == null)
            {
                currentState = QuestState.NotStarted;
                return qSO;
            }

            if (qData.state != QuestState.Completed)
            {
                currentState = qData.state;
                return qSO;
            }
        }

        return null;
    }

    public void ShowInitialDialogue()
    {
        if (defaultNPCDialogue == null) return;

        DialogueData baseData = defaultNPCDialogue.GetDialogue();
        string npcIdentifier = !string.IsNullOrEmpty(baseData.npcName) ? baseData.npcName : gameObject.name;

        bool isQuestGiver = assignedQuests.Exists(q => q != null && q.questID == QuestManager.TALK_ALL_QUEST_ID);

        // Đang làm Quest chào hỏi và người này KHÔNG PHẢI người giao quest
        if (QuestManager.Instance != null && QuestManager.Instance.IsTalkQuestActive() && !isQuestGiver)
        {
            if (!QuestManager.Instance.HasTalkedToNPCInQuest(npcIdentifier))
            {
                QuestManager.Instance.RegisterNPCTalk(npcIdentifier);

                List<DialogueLine> introLines = firstIntroQuestDialogues;
                if (introLines == null || introLines.Count == 0)
                {
                    introLines = new List<DialogueLine>
                    {
                        new DialogueLine(SpeakerType.NPC, $"Hello newcomer! I am {npcIdentifier}. Welcome to our base."),
                        new DialogueLine(SpeakerType.Player, "Nice to meet you! I'm just looking around getting to know everyone.")
                    };
                }

                List<DialogueChoice> introChoices = new List<DialogueChoice>
                {
                    new DialogueChoice { choiceText = "Nice to meet you!", action = DialogueAction.CloseDialogue, menuType = MenuType.None }
                };

                if (DialogueUI.Instance != null)
                {
                    DialogueUI.Instance.ShowCustom(npcIdentifier, introLines, introChoices);
                }
                return;
            }
        }

        List<DialogueLine> lines = new List<DialogueLine>();
        if (baseData.dialogues != null)
        {
            foreach (var d in baseData.dialogues)
            {
                lines.Add(new DialogueLine(SpeakerType.NPC, d));
            }
        }

        List<DialogueChoice> choices = new List<DialogueChoice>();
        bool hasQuest = HasAvailableOrActiveQuest();

        if (baseData.choices != null)
        {
            for (int i = 0; i < baseData.choices.Count; i++)
            {
                DialogueChoice choice = baseData.choices[i];
                if (choice == null) continue;

                bool isQuestButton = (choice.menuType == MenuType.LobbyQuestMenu) || 
                                     choice.choiceText.IndexOf("Quest", System.StringComparison.OrdinalIgnoreCase) >= 0;

                if (isQuestButton)
                {
                    if (!hasQuest) continue;

                    DialogueChoice questBranchChoice = new DialogueChoice();
                    questBranchChoice.choiceText = choice.choiceText;
                    questBranchChoice.action = DialogueAction.None;
                    questBranchChoice.menuType = MenuType.None;
                    choices.Add(questBranchChoice);
                    continue;
                }

                choices.Add(choice);
            }
        }

        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.ShowCustom(npcIdentifier, lines, choices);
        }
    }

    public void ShowQuestDialogue()
    {
        QuestState state;
        QuestSO currentQuestSO = GetCurrentActiveQuestSO(out state);

        if (currentQuestSO == null)
        {
            ShowInitialDialogue();
            return;
        }

        string npcName = !string.IsNullOrEmpty(currentQuestSO.npcName) ? currentQuestSO.npcName : gameObject.name;
        List<DialogueLine> sourceLines = null;

        switch (state)
        {
            case QuestState.NotStarted:
                sourceLines = currentQuestSO.notStartedDialogues;
                break;
            case QuestState.InProgress:
                sourceLines = currentQuestSO.inProgressDialogues;
                break;
            case QuestState.CanClaim:
                sourceLines = currentQuestSO.canClaimDialogues;
                break;
            case QuestState.Completed:
                sourceLines = currentQuestSO.completedDialogues;
                break;
        }

        if (sourceLines == null || sourceLines.Count == 0)
        {
            sourceLines = new List<DialogueLine>() { new DialogueLine(SpeakerType.NPC, "...") };
        }

        DialogueChoice actionChoice = new DialogueChoice();
        if (state == QuestState.NotStarted)
        {
            actionChoice.choiceText = "Accept Quest";
            actionChoice.action = DialogueAction.CloseDialogue;
            actionChoice.menuType = MenuType.None;
        }
        else if (state == QuestState.CanClaim)
        {
            actionChoice.choiceText = "Claim Reward";
            actionChoice.action = DialogueAction.CloseDialogue;
            actionChoice.menuType = MenuType.None;
        }
        else
        {
            actionChoice.choiceText = "Continue";
            actionChoice.action = DialogueAction.CloseDialogue;
            actionChoice.menuType = MenuType.None;
        }

        DialogueChoice backChoice = new DialogueChoice();
        backChoice.choiceText = "Back";
        backChoice.action = DialogueAction.None;
        backChoice.menuType = MenuType.None;

        List<DialogueChoice> choices = new List<DialogueChoice> { actionChoice, backChoice };

        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.ShowCustom(npcName, sourceLines, choices);
        }
    }

    public void OnQuestDialogueActionTriggered()
    {
        QuestState state;
        QuestSO currentQuestSO = GetCurrentActiveQuestSO(out state);
        if (currentQuestSO == null || QuestManager.Instance == null) return;

        if (state == QuestState.NotStarted)
        {
            QuestManager.Instance.StartQuest(currentQuestSO.questID);
        }
        else if (state == QuestState.CanClaim)
        {
            QuestManager.Instance.ClaimQuest(currentQuestSO.questID);
        }

        RefreshQuestVisuals();
    }
}