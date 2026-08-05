using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

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

    [Header("Typewriter Settings")]
    [SerializeField]
    private float textSpeedPerChar = 0.03f;

    private float allowInputTime = 0f;
    private DialogueData currentDialogue;
    private int currentIndex;

    private Coroutine typewriterRoutine;
    private bool isTyping = false;
    private string targetFullText = "";

    private void Awake()
    {
        Instance = this;
        Hide();
    }

    public void Show(DialogueData data)
    {
        if (data == null)
        {
            return;
        }

        currentDialogue = data;
        currentIndex = 0;

        UIManager.Instance.ChangeMenu(MenuType.LobbyDialogueMenu);

        if (npcNameText != null)
        {
            npcNameText.SetTextSafe(data.npcName);
        }

        if (root != null)
        {
            root.transform.localScale = Vector3.one * 0.85f;
            root.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack).SetUpdate(true);
        }

        RefreshDialogue();
        SetChoiceVisible(false);

        if (InteractManagerV2.Instance != null)
        {
            InteractManagerV2.Instance.IsBusy = true;
        }

        if (InteractUIV2.Instance != null)
        {
            InteractUIV2.Instance.Hide();
        }

        allowInputTime = Time.unscaledTime + 0.15f;
    }

    public void Hide()
    {
        StopTypewriterRoutine();

        isTyping = false;
        currentDialogue = null;
        currentIndex = 0;

        if (UIManager.Instance != null)
        {
            UnityEngine.SceneManagement.Scene activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (activeScene.name == "Run Scene")
            {
                UIManager.Instance.ChangeMenu(MenuType.GameplayMenu);
            }
            else
            {
                UIManager.Instance.ChangeMenu(MenuType.DefaultLobbyInputMenu);
            }
        }

        if (InteractManagerV2.Instance != null)
        {
            InteractManagerV2.Instance.IsBusy = false;
            InteractManagerV2.Instance.ForceRefresh();
        }
    }

    private void OnDisable()
    {
        StopTypewriterRoutine();

        if (InteractManagerV2.Instance != null)
        {
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

        targetFullText = currentDialogue.dialogues[currentIndex];
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

        if (Time.unscaledTime < allowInputTime) return;

        // Nếu đã xuất hiện các nút Lựa chọn (Choice) thì không cho bấm F/Click chuyển tiếp
        if (choice1Button != null && choice1Button.gameObject.activeSelf) return;

        if (!Input.GetKeyDown(KeyCode.F) && !Input.GetMouseButtonDown(0)) return;

        if (currentDialogue == null) return;

        // Nếu chữ đang chạy từ từ -> Bấm F hoặc Click sẽ hiện full ngay
        if (isTyping)
        {
            SkipTypingEffect();
            return;
        }

        // Nếu chữ đã gõ xong -> Bấm F hoặc Click sẽ chuyển sang câu tiếp theo
        if (currentIndex < currentDialogue.dialogues.Count - 1)
        {
            currentIndex++;
            RefreshDialogue();
            return;
        }

        SetChoiceVisible(true);
    }

    private void SetChoiceVisible(bool value)
    {
        if (choice1Button != null) choice1Button.gameObject.SetActive(value);
        if (choice2Button != null) choice2Button.gameObject.SetActive(value);
        if (choice3Button != null) choice3Button.gameObject.SetActive(value);

        if (!value) return;

        SetupButton(choice1Button, choice1Text, currentDialogue, 0);
        SetupButton(choice2Button, choice2Text, currentDialogue, 1);
        SetupButton(choice3Button, choice3Text, currentDialogue, 2);

        Button[] buttons = { choice1Button, choice2Button, choice3Button };
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] != null && buttons[i].gameObject.activeSelf)
            {
                buttons[i].transform.localScale = Vector3.zero;
                buttons[i].transform.DOScale(Vector3.one, 0.2f).SetDelay(i * 0.05f).SetEase(Ease.OutBack).SetUpdate(true);
            }
        }
    }

    private void SetupButton(Button button, TMP_Text text, DialogueData data, int index)
    {
        if (button == null) return;

        if (index >= data.choices.Count)
        {
            button.gameObject.SetActive(false);
            return;
        }

        button.gameObject.SetActive(true);
        DialogueChoice choice = data.choices[index];

        if (text != null)
        {
            text.SetTextSafe(choice.choiceText);
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => ExecuteChoice(choice));
    }

    private void ExecuteChoice(DialogueChoice choice)
    {
        switch (choice.action)
        {
            case DialogueAction.OpenPanel:
                UIManager.Instance.ChangeMenu(choice.menuType);

                if (InteractManagerV2.Instance != null)
                {
                    InteractManagerV2.Instance.IsBusy = false;
                    InteractManagerV2.Instance.ForceRefresh();
                }
                break;

            case DialogueAction.CloseDialogue:
            default:
                Hide();
                break;
        }
    }
}