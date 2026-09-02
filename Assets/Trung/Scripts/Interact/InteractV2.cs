using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public class InteractV2 : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private string interactText = "Interact";
    [SerializeField] private Sprite interactIcon;
    [SerializeField] private int priority = 0;

    [Header("Trigger")]
    [SerializeField] private string playerTag = "Player";

    private bool isSelected;

    [SerializeField] private bool openPanel;
    [SerializeField] private MenuType menuType = MenuType.None;

    #region Property
    public string InteractText => interactText;
    public Sprite InteractIcon => interactIcon;
    public int Priority => priority;
    public bool IsSelected => isSelected;
    public MenuType MenuType => menuType;
    public bool OpenPanel => openPanel;
    #endregion

    private void Awake()
    {
        CheckFeatureUnlockStatus();
    }

    private void Start()
    {
        CheckFeatureUnlockStatus();
    }

    private void OnEnable()
    {
        CheckFeatureUnlockStatus();
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestUpdated += CheckFeatureUnlockStatus;
        }
    }

    public bool IsFeatureUnlocked()
    {
        if (!openPanel) return true;

        if (menuType == MenuType.LobbyRuneInventoryMenu || 
            menuType == MenuType.LobbyCharacterMenu || 
            menuType == MenuType.LobbyBossSelectMenu)
        {
            if (QuestManager.Instance != null)
            {
                return QuestManager.Instance.IsMenuUnlocked(menuType);
            }
        }

        return true;
    }

    public void CheckFeatureUnlockStatus()
    {
        bool unlocked = IsFeatureUnlocked();

        Collider[] allCols = GetComponents<Collider>();
        for (int i = 0; i < allCols.Length; i++)
        {
            if (allCols[i] != null && allCols[i].isTrigger)
            {
                allCols[i].enabled = unlocked;
            }
        }

        if (!unlocked)
        {
            if (InteractManagerV2.Instance != null)
            {
                InteractManagerV2.Instance.Unregister(this);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag) && !other.gameObject.name.Contains("Player")) return;

        // Tự động kiểm tra lại điều kiện unlock ngay khi người chơi vừa chạm trigger
        CheckFeatureUnlockStatus();

        if (!IsFeatureUnlocked()) return;

        if (InteractManagerV2.Instance != null)
        {
            InteractManagerV2.Instance.Register(this);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag(playerTag) && !other.gameObject.name.Contains("Player")) return;

        // Nếu đứng chờ sẵn ở bệ, liên tục kiểm tra nếu quest vừa hoàn tất hoặc vừa mở khóa
        if (!IsFeatureUnlocked())
        {
            CheckFeatureUnlockStatus();
            if (!IsFeatureUnlocked())
            {
                if (InteractManagerV2.Instance != null)
                {
                    InteractManagerV2.Instance.Unregister(this);
                }
                return;
            }
        }

        if (InteractManagerV2.Instance != null)
        {
            InteractManagerV2.Instance.Register(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag) && !other.gameObject.name.Contains("Player")) return;

        if (InteractManagerV2.Instance != null)
        {
            InteractManagerV2.Instance.Unregister(this);
        }
    }

    public virtual void Execute()
    {
        // 1. Kích hoạt OnInteract trên NPC để ép xoay mặt về người chơi ngay lập tức
        SendMessage("OnInteract", SendMessageOptions.DontRequireReceiver);

        // 2. NPC Quest
        NPCQuestHandler questHandler = GetComponent<NPCQuestHandler>();
        if (questHandler != null)
        {
            questHandler.ShowInitialDialogue();
            return;
        }

        // 3. NPC Thoại thường
        NPCDialogue dialogue = GetComponent<NPCDialogue>();
        if (dialogue != null)
        {
            if (DialogueUI.Instance != null)
            {
                DialogueUI.Instance.Show(dialogue.GetDialogue());
            }
            return;
        }

        // 4. Tương tác mở Menu Panel
        if (openPanel)
        {
            if (!IsFeatureUnlocked()) return;

            if (menuType == MenuType.LobbyRuneInventoryMenu && QuestManager.Instance != null)
            {
                QuestManager.Instance.AddQuestProgress("QUEST_OPEN_RUNE_INVENTORY", 1);
            }

            if (menuType == MenuType.LobbyCharacterMenu && QuestManager.Instance != null)
            {
                QuestManager.Instance.AddQuestProgress("QUEST_SELECT_CHARACTER", 1);
            }

            if (InteractManagerV2.Instance != null)
            {
                InteractManagerV2.Instance.IsBusy = true;
                if (InteractUIV2.Instance != null)
                {
                    InteractUIV2.Instance.Hide();
                }
            }

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ChangeMenu(menuType);
            }

            return;
        }
    }

    public void SetSelected(bool value)
    {
        if (isSelected == value) return;
        isSelected = value;
        if (value)
        {
            SendMessage("OnSelected", SendMessageOptions.DontRequireReceiver);
        }
        else
        {
            SendMessage("OnDeselected", SendMessageOptions.DontRequireReceiver);
        }
    }

    public void SetInteractText(string value)
    {
        interactText = value;
        if (InteractManagerV2.Instance != null)
        {
            InteractManagerV2.Instance.ForceRefresh();
        }
    }

    private void OnDisable()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestUpdated -= CheckFeatureUnlockStatus;
        }

        if (InteractManagerV2.Instance != null)
        {
            InteractManagerV2.Instance.Unregister(this);
        }
    }

    private void OnDestroy()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestUpdated -= CheckFeatureUnlockStatus;
        }

        if (InteractManagerV2.Instance != null)
        {
            InteractManagerV2.Instance.Unregister(this);
        }
    }
}