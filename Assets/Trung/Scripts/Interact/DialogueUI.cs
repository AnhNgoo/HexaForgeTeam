using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    public static DialogueUI Instance;

    [Header("Root")]
    [SerializeField]
    private GameObject root;

    [Header("UI")]
    [SerializeField]
    private TMP_Text npcNameText;

    [SerializeField]
    private TMP_Text dialogueText;

    [Header("Choice")]
    [SerializeField]
    private Button choice1Button;

    [SerializeField]
    private Button choice2Button;

    [SerializeField]
    private Button choice3Button;

    [SerializeField]
    private TMP_Text choice1Text;

    [SerializeField]
    private TMP_Text choice2Text;

    [SerializeField]
    private TMP_Text choice3Text;

    private void Awake()
    {
        
        Instance = this;

        Hide();
    }
    private DialogueData currentDialogue;

private int currentIndex;

    public void Show(
    DialogueData data)
{
    if (data == null)
    {
        return;
    }

    currentDialogue = data;

    currentIndex = 0;

    UIManager.Instance.ChangeMenu(
    MenuType.LobbyDialogueMenu);

    if (npcNameText != null)
    {
        npcNameText.text =
            data.npcName;
    }

    RefreshDialogue();

    SetChoiceVisible(false);

    if (InteractManagerV2.Instance != null)
    {
        InteractManagerV2.Instance
            .IsBusy = true;
    }

    if (InteractUIV2.Instance != null)
    {
        InteractUIV2.Instance
            .Hide();
    }
}

    public void Hide()
{
    if (UIManager.Instance != null)
{
    UIManager.Instance.CloseAllMenus();
}
    if (InteractManagerV2.Instance != null)
    {
        InteractManagerV2.Instance
            .IsBusy = false;

        InteractManagerV2.Instance
            .ForceRefresh();
    }
}

    private void SetupButton(
        Button button,
        TMP_Text text,
        DialogueData data,
        int index)
    {
        if (button == null)
        {
            return;
        }

        if (index >= data.choices.Count)
        {
            button.gameObject
                .SetActive(false);

            return;
        }

        button.gameObject
            .SetActive(true);

        DialogueChoice choice =
            data.choices[index];

        if (text != null)
        {
            text.text =
                choice.choiceText;
        }

        button.onClick
            .RemoveAllListeners();

        button.onClick
            .AddListener(() =>
            ExecuteChoice(choice));
    }

    private void ExecuteChoice(
        DialogueChoice choice)
    {
        switch (choice.action)
        {
            case DialogueAction.OpenPanel:

    UIManager.Instance.ChangeMenu(
        choice.menuType);

    if (InteractManagerV2.Instance != null)
    {
        InteractManagerV2.Instance.IsBusy = false;
        InteractManagerV2.Instance.ForceRefresh();
    }

    break;

            case DialogueAction.CloseDialogue:

                Hide();

                break;

            default:

                Hide();

                break;
        }
    }
    private void RefreshDialogue()
{
    if (dialogueText != null)
    {
        dialogueText.text =
            currentDialogue
            .dialogues[currentIndex];
    }
}
private void SetChoiceVisible(
    bool value)
{
    if (choice1Button != null)
    {
        choice1Button.gameObject
            .SetActive(value);
    }

    if (choice2Button != null)
    {
        choice2Button.gameObject
            .SetActive(value);
    }

    if (choice3Button != null)
    {
        choice3Button.gameObject
            .SetActive(value);
    }

    if (!value)
    {
        return;
    }

    SetupButton(
        choice1Button,
        choice1Text,
        currentDialogue,
        0);

    SetupButton(
        choice2Button,
        choice2Text,
        currentDialogue,
        1);

    SetupButton(
        choice3Button,
        choice3Text,
        currentDialogue,
        2);
}
private void Update()
{
    if (root == null || !root.activeSelf) return;

    // MỚI: Nếu các nút lựa chọn đang hiện, ngừng xử lý bấm F/Click chuột để tránh nhảy index loạn
    if (choice1Button != null && choice1Button.gameObject.activeSelf) return;

    if (!Input.GetKeyDown(KeyCode.F) && !Input.GetMouseButtonDown(0)) return;

    if (currentDialogue == null) return;

    if (currentIndex < currentDialogue.dialogues.Count - 1)
    {
        currentIndex++;
        RefreshDialogue();
        return;
    }

    SetChoiceVisible(true);
}
}