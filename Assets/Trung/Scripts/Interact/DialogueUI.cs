using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
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

    [Header("Typewriter Settings")]
    [SerializeField] private float textSpeedPerChar = 0.03f;

    private float allowInputTime = 0f;
    private DialogueData currentDialogue;
    private int currentIndex;

    private Coroutine typewriterRoutine;
    private bool isTyping = false;
    private string targetFullText = "";

    private List<ChoiceTabItem> allChoices = new List<ChoiceTabItem>();

    private void Awake()
    {
        Instance = this;

        allChoices = new List<ChoiceTabItem>() { choice1Tab, choice2Tab, choice3Tab };
        InitTabHoverTriggers();

        Hide();

        if (InteractManagerV2.Instance != null)
        {
            InteractManagerV2.Instance.IsBusy = false;
        }
    }

    private void OnEnable()
    {
        // ✅ Tự dịch lại khi đổi ngôn ngữ
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }

    private void OnLocaleChanged(Locale locale)
    {
        // Đang mở dialogue → dịch lại tên NPC, lời thoại và các nút
        if (root != null && root.activeSelf && currentDialogue != null)
        {
            if (npcNameText != null)
                npcNameText.SetTextSafe(T(currentDialogue.npcName));

            RefreshDialogue();
            if (AreChoicesVisible())
                RefreshChoices();
        }
    }

    // ✅ Helper dịch text (bọc SettingsLocalizationData.Translate)
    private string T(string text)
    {
        return SettingsLocalizationData.Translate(text);
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

        currentDialogue = data;
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
            UIManager.Instance.ChangeMenu(MenuType.LobbyDialogueMenu);
        }

        if (npcNameText != null)
        {
            // ✅ Dịch tên NPC
            npcNameText.SetTextSafe(T(data.npcName));
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
        currentDialogue = null;
        currentIndex = 0;

        if (root != null)
        {
            root.SetActive(false);
        }

        if (UIManager.Instance != null)
        {
            string sceneLobbyName = GameSceneData.Instance != null ? GameSceneData.Instance.GetSceneName(SceneType.LobbyMain) : "LobbyMain Scene";
            string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            if (currentSceneName == sceneLobbyName)
            {
                UIManager.Instance.ChangeMenu(MenuType.DefaultLobbyInputMenu);
            }
            else
            {
                UIManager.Instance.ChangeMenu(MenuType.GameplayMenu);
            }
        }

        if (InteractManagerV2.Instance != null)
        {
            InteractManagerV2.Instance.SetCooldown(0.25f);
            InteractManagerV2.Instance.IsBusy = false;
            InteractManagerV2.Instance.ForceRefresh();
        }
    }

    private void RefreshDialogue()
    {
        if (dialogueText == null || currentDialogue == null || currentIndex >= currentDialogue.dialogues.Count)
        {
            return;
        }

        StopTypewriterRoutine();

        // ✅ Dịch lời thoại TRƯỚC khi chạy typewriter
        targetFullText = T(currentDialogue.dialogues[currentIndex]);
        typewriterRoutine = StartCoroutine(TypewriterRoutine(targetFullText));
    }

    // ✅ Thêm method này để refresh các nút khi đổi ngôn ngữ
    private void RefreshChoices()
    {
        if (currentDialogue == null || currentDialogue.choices == null) return;

        SetupTabChoice(choice1Tab, currentDialogue, 0);
        SetupTabChoice(choice2Tab, currentDialogue, 1);
        SetupTabChoice(choice3Tab, currentDialogue, 2);
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

        if (currentDialogue == null) return;

        if (isTyping)
        {
            SkipTypingEffect();
            return;
        }

        if (currentIndex < currentDialogue.dialogues.Count - 1)
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
               (choice3Tab?.button != null && choice3Tab.button.gameObject.activeSelf);
    }

    private void HandleChoiceHotkeys()
    {
        if (currentDialogue == null || currentDialogue.choices == null) return;

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
    }

    private void TriggerChoiceAtIndex(int index)
    {
        if (index < 0 || index >= currentDialogue.choices.Count) return;

        ChoiceTabItem targetTab = (index == 0) ? choice1Tab : (index == 1) ? choice2Tab : choice3Tab;
        if (targetTab != null && targetTab.button != null && targetTab.button.gameObject.activeSelf)
        {
            SetTabHoverVisual(index, true);
            ExecuteChoice(currentDialogue.choices[index]);
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
            return;
        }

        RefreshChoices();

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

    private void SetupTabChoice(ChoiceTabItem tab, DialogueData data, int index)
    {
        if (tab == null || tab.button == null) return;

        if (index >= data.choices.Count)
        {
            tab.button.gameObject.SetActive(false);
            return;
        }

        tab.button.gameObject.SetActive(true);
        DialogueChoice choice = data.choices[index];

        if (tab.text != null)
        {
            // ✅ Dịch text của nút lựa chọn
            tab.text.SetTextSafe(T(choice.choiceText));
            tab.text.color = Color.white;
        }

        if (tab.selectedLine != null)
        {
            tab.selectedLine.SetActive(false);
        }

        tab.button.onClick.RemoveAllListeners();
        tab.button.onClick.AddListener(() => ExecuteChoice(choice));
    }

    private void ExecuteChoice(DialogueChoice choice)
    {
        switch (choice.action)
        {
            case DialogueAction.OpenPanel:
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