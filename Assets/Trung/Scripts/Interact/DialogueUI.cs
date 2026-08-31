using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class DialogueUI : MonoBehaviour
{
    public static DialogueUI Instance;

    [System.Serializable]
    public class ChoiceTabItem
    {
        public Button button;
        public GameObject selectedLine;
        public TMP_Text text;
        [HideInInspector] public EventTrigger trigger;
    }

    [Header("Root")]
    [SerializeField] private GameObject root;

    [Header("UI")]
    [SerializeField] private TMP_Text npcNameText;
    [SerializeField] private TMP_Text dialogueText;

    [Header("Choices with Hover Line Indicator")]
    [SerializeField] private ChoiceTabItem choice1Tab;
    [SerializeField] private ChoiceTabItem choice2Tab;
    [SerializeField] private ChoiceTabItem choice3Tab;
    [SerializeField] private ChoiceTabItem choice4Tab;

    [Header("Typewriter Settings")]
    [SerializeField] private float textSpeedPerChar = 0.03f;

    private float allowInputTime = 0f;
    private List<DialogueLine> currentLines = new List<DialogueLine>();
    private string currentNPCName = "NPC";
    private List<DialogueChoice> currentChoices = new List<DialogueChoice>();
    private int currentIndex;

    private Coroutine typewriterRoutine;
    private bool isTyping = false;
    private string targetFullText = "";

    private List<ChoiceTabItem> allChoices = new List<ChoiceTabItem>();
    private MenuType previousMenuType = MenuType.DefaultLobbyInputMenu;

    public bool IsDialogueOpen() => root != null && root.activeInHierarchy;

    private void Awake()
    {
        Instance = this;

        allChoices = new List<ChoiceTabItem>() { choice1Tab, choice2Tab, choice3Tab, choice4Tab };
        InitTabHoverTriggers();

        if (root != null) root.SetActive(false);
    }

    private void InitTabHoverTriggers()
    {
        for (int i = 0; i < allChoices.Count; i++)
        {
            int index = i;
            var tab = allChoices[index];
            if (tab == null || tab.button == null) continue;

            EventTrigger trigger = tab.button.gameObject.GetComponent<EventTrigger>();
            if (trigger == null) trigger = tab.button.gameObject.AddComponent<EventTrigger>();
            tab.trigger = trigger;

            trigger.triggers.Clear();

            EventTrigger.Entry enterEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            enterEntry.callback.AddListener((eventData) => { SetTabHoverVisual(index, true); });
            trigger.triggers.Add(enterEntry);

            EventTrigger.Entry exitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            exitEntry.callback.AddListener((eventData) => { SetTabHoverVisual(index, false); });
            trigger.triggers.Add(exitEntry);

            if (tab.selectedLine != null)
            {
                tab.selectedLine.SetActive(false);
            }
        }
    }

    private void SetTabHoverVisual(int index, bool isHovered)
    {
        if (index < 0 || index >= allChoices.Count) return;
        var tab = allChoices[index];
        if (tab == null) return;

        if (tab.selectedLine != null)
        {
            tab.selectedLine.transform.DOKill();
            if (isHovered)
            {
                tab.selectedLine.SetActive(true);
                tab.selectedLine.transform.localScale = new Vector3(0f, 1f, 1f);
                tab.selectedLine.transform.DOScaleX(1f, 0.15f).SetUpdate(true);
            }
            else
            {
                tab.selectedLine.transform.DOScaleX(0f, 0.12f).SetUpdate(true).OnComplete(() =>
                {
                    tab.selectedLine.SetActive(false);
                });
            }
        }

        if (tab.text != null)
        {
            tab.text.color = isHovered ? new Color(1f, 0.85f, 0.2f) : Color.white;
        }
    }

    private void ResetAllHoverLines()
    {
        for (int i = 0; i < allChoices.Count; i++)
        {
            var tab = allChoices[i];
            if (tab == null) continue;

            if (tab.selectedLine != null)
            {
                tab.selectedLine.transform.DOKill();
                tab.selectedLine.SetActive(false);
            }
            if (tab.text != null)
            {
                tab.text.color = Color.white;
            }
        }
    }

    public void Show(DialogueData data)
    {
        if (data == null) return;

        List<DialogueLine> lines = new List<DialogueLine>();
        if (data.dialogues != null)
        {
            foreach (var d in data.dialogues)
            {
                lines.Add(new DialogueLine(SpeakerType.NPC, d));
            }
        }

        ShowCustom(data.npcName, lines, data.choices);
    }

    public void ShowCustom(string npcName, List<DialogueLine> lines, List<DialogueChoice> choices)
    {
        currentNPCName = npcName;
        currentLines = lines != null ? new List<DialogueLine>(lines) : new List<DialogueLine>();
        currentChoices = choices != null ? new List<DialogueChoice>(choices) : new List<DialogueChoice>();
        currentIndex = 0;

        if (InteractManagerV2.Instance != null)
        {
            InteractManagerV2.Instance.IsBusy = true;
        }

        if (InteractUIV2.Instance != null)
        {
            InteractUIV2.Instance.Hide();
        }

        if (UIManager.Instance != null)
        {
            if (UIManager.Instance.CurrentMenuType != MenuType.LobbyDialogueMenu)
            {
                previousMenuType = UIManager.Instance.CurrentMenuType;
            }
            UIManager.Instance.ChangeMenu(MenuType.LobbyDialogueMenu);
        }

        if (root != null)
        {
            root.SetActive(true);
            root.transform.localScale = Vector3.one * 0.85f;
            root.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack).SetUpdate(true);
        }

        RefreshDialogue();
        SetChoiceVisible(false);

        allowInputTime = Time.unscaledTime + 0.15f;
    }

    public void Hide()
    {
        StopTypewriterRoutine();
        ResetAllHoverLines();

        isTyping = false;
        currentLines.Clear();
        currentChoices.Clear();
        currentIndex = 0;

        if (root != null)
        {
            root.SetActive(false);
        }

        if (InteractManagerV2.Instance != null)
        {
            InteractManagerV2.Instance.IsBusy = false;
            InteractManagerV2.Instance.ForceRefresh();
        }

        if (UIManager.Instance != null)
        {
            MenuType targetMenu = (previousMenuType != MenuType.None && previousMenuType != MenuType.LobbyDialogueMenu)
                ? previousMenuType 
                : MenuType.DefaultLobbyInputMenu;

            UIManager.Instance.ChangeMenu(targetMenu);
        }
    }

    private void OnDisable()
    {
        StopTypewriterRoutine();
        ResetAllHoverLines();
    }

    private void RefreshDialogue()
    {
        if (dialogueText == null || currentLines == null || currentIndex >= currentLines.Count)
        {
            return;
        }

        StopTypewriterRoutine();

        DialogueLine line = currentLines[currentIndex];
        
        if (npcNameText != null)
        {
            if (line.speaker == SpeakerType.Player)
            {
                npcNameText.SetTextSafe("<color=#55FFFF>[Player]</color>");
            }
            else
            {
                npcNameText.SetTextSafe(currentNPCName);
            }
        }

        targetFullText = line.text;
        typewriterRoutine = StartCoroutine(TypewriterRoutine(targetFullText));
    }

    private IEnumerator TypewriterRoutine(string fullText)
    {
        isTyping = true;
        dialogueText.SetTextSafe("");

        int totalChars = fullText.Length;
        for (int i = 0; i <= totalChars; i++)
        {
            string currentSubString = fullText.Substring(0, i);
            dialogueText.SetTextSafe(currentSubString);
            yield return new WaitForSecondsRealtime(textSpeedPerChar);
        }

        isTyping = false;
        typewriterRoutine = null;
    }

    private void SkipTypingEffect()
    {
        StopTypewriterRoutine();

        if (dialogueText != null)
        {
            dialogueText.SetTextSafe(targetFullText);
        }

        isTyping = false;
    }

    private void StopTypewriterRoutine()
    {
        if (typewriterRoutine != null)
        {
            StopCoroutine(typewriterRoutine);
            typewriterRoutine = null;
        }
    }

    private void Update()
    {
        if (root == null || !root.activeSelf) return;

        if (InteractManagerV2.Instance != null && InteractManagerV2.Instance.WasInputConsumedThisFrame())
        {
            return;
        }

        if (Time.unscaledTime < allowInputTime) return;

        if (AreChoicesVisible())
        {
            HandleChoiceHotkeys();
            return;
        }

        if (!Input.GetKeyDown(KeyCode.F) && !Input.GetMouseButtonDown(0)) return;

        if (currentLines == null || currentLines.Count == 0) return;

        if (isTyping)
        {
            SkipTypingEffect();
            return;
        }

        if (currentIndex < currentLines.Count - 1)
        {
            currentIndex++;
            RefreshDialogue();
            return;
        }

        SetChoiceVisible(true);
    }

    private bool AreChoicesVisible()
    {
        return (choice1Tab?.button != null && choice1Tab.button.gameObject.activeSelf) ||
               (choice2Tab?.button != null && choice2Tab.button.gameObject.activeSelf) ||
               (choice3Tab?.button != null && choice3Tab.button.gameObject.activeSelf) ||
               (choice4Tab?.button != null && choice4Tab.button.gameObject.activeSelf);
    }

    private void HandleChoiceHotkeys()
    {
        if (currentChoices == null) return;

        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            TriggerChoiceAtIndex(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            TriggerChoiceAtIndex(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
        {
            TriggerChoiceAtIndex(2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4))
        {
            TriggerChoiceAtIndex(3);
        }
    }

    private void TriggerChoiceAtIndex(int index)
    {
        if (index < 0 || index >= currentChoices.Count) return;

        ChoiceTabItem targetTab = (index == 0) ? choice1Tab : (index == 1) ? choice2Tab : (index == 2) ? choice3Tab : choice4Tab;
        if (targetTab != null && targetTab.button != null && targetTab.button.gameObject.activeSelf)
        {
            SetTabHoverVisual(index, true);
            ExecuteChoice(currentChoices[index]);
        }
    }

    private void SetChoiceVisible(bool value)
    {
        ResetAllHoverLines();

        if (!value)
        {
            if (choice1Tab?.button != null) choice1Tab.button.gameObject.SetActive(false);
            if (choice2Tab?.button != null) choice2Tab.button.gameObject.SetActive(false);
            if (choice3Tab?.button != null) choice3Tab.button.gameObject.SetActive(false);
            if (choice4Tab?.button != null) choice4Tab.button.gameObject.SetActive(false);
            return;
        }

        SetupTabChoice(choice1Tab, 0);
        SetupTabChoice(choice2Tab, 1);
        SetupTabChoice(choice3Tab, 2);
        SetupTabChoice(choice4Tab, 3);

        for (int i = 0; i < allChoices.Count; i++)
        {
            var tab = allChoices[i];
            if (tab != null && tab.button != null && tab.button.gameObject.activeSelf)
            {
                tab.button.transform.localScale = Vector3.zero;
                tab.button.transform.DOScale(Vector3.one, 0.2f).SetDelay(i * 0.05f).SetEase(Ease.OutBack).SetUpdate(true);
            }
        }
    }

    private void SetupTabChoice(ChoiceTabItem tab, int index)
    {
        if (tab == null || tab.button == null) return;

        if (index >= currentChoices.Count)
        {
            tab.button.gameObject.SetActive(false);
            return;
        }

        tab.button.gameObject.SetActive(true);
        DialogueChoice choice = currentChoices[index];

        bool isUnlocked = true;
        if (choice.action == DialogueAction.OpenPanel && choice.menuType != MenuType.None && choice.menuType != MenuType.LobbyQuestMenu)
        {
            if (QuestManager.Instance != null)
            {
                isUnlocked = QuestManager.Instance.IsMenuUnlocked(choice.menuType);
            }
        }

        tab.button.interactable = isUnlocked;

        if (tab.text != null)
        {
            if (isUnlocked)
            {
                tab.text.SetTextSafe(choice.choiceText);
                tab.text.color = Color.white;
            }
            else
            {
                tab.text.SetTextSafe($"{choice.choiceText} <color=#888888>(Locked)</color>");
                tab.text.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            }
        }

        if (tab.selectedLine != null)
        {
            tab.selectedLine.SetActive(false);
        }

        tab.button.onClick.RemoveAllListeners();
        if (isUnlocked)
        {
            tab.button.onClick.AddListener(() => ExecuteChoice(choice));
        }
    }

    private void ExecuteChoice(DialogueChoice choice)
    {
        if (InteractManagerV2.Instance != null && InteractManagerV2.Instance.CurrentInteract != null)
        {
            NPCQuestHandler questHandler = InteractManagerV2.Instance.CurrentInteract.GetComponent<NPCQuestHandler>();
            if (questHandler != null)
            {
                if (choice.action == DialogueAction.None && choice.menuType == MenuType.None)
                {
                    if (choice.choiceText.Equals("Back", System.StringComparison.OrdinalIgnoreCase))
                    {
                        questHandler.ShowInitialDialogue();
                    }
                    else
                    {
                        questHandler.ShowQuestDialogue();
                    }
                    return;
                }
                else if (choice.action == DialogueAction.CloseDialogue && 
                        (choice.choiceText.Equals("Accept Quest", System.StringComparison.OrdinalIgnoreCase) || 
                         choice.choiceText.Equals("Claim Reward", System.StringComparison.OrdinalIgnoreCase)))
                {
                    questHandler.OnQuestDialogueActionTriggered();
                }
            }
        }

        switch (choice.action)
        {
            case DialogueAction.OpenPanel:
                // Nếu người chơi mở Shop khi đang nhận Quest 3 -> Hoàn thành nhiệm vụ
                if (choice.menuType == MenuType.LobbyShopMenu)
                {
                    if (QuestManager.Instance != null)
                    {
                        QuestManager.Instance.AddQuestProgress("QUEST_VISIT_SHOP", 1);
                    }
                }

                StopTypewriterRoutine();
                if (root != null) root.SetActive(false);

                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ChangeMenu(choice.menuType);
                }
                break;

            case DialogueAction.CloseDialogue:
            default:
                Hide();
                break;
        }
    }
}