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

    private bool playerInside;
    private bool isSelected;

    [SerializeField] private bool openPanel;
    [SerializeField] private MenuType menuType = MenuType.None;

    #region Property
    public string InteractText => interactText;
    public Sprite InteractIcon => interactIcon;
    public int Priority => priority;
    public bool PlayerInside => playerInside;
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
            playerInside = false;
            if (InteractManagerV2.Instance != null)
            {
                InteractManagerV2.Instance.Unregister(this);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag) && !other.gameObject.name.Contains("Player")) return;
        if (!IsFeatureUnlocked()) return;

        playerInside = true;
        if (InteractManagerV2.Instance != null)
        {
            InteractManagerV2.Instance.Register(this);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag(playerTag) && !other.gameObject.name.Contains("Player")) return;

        if (!IsFeatureUnlocked())
        {
            if (playerInside)
            {
                playerInside = false;
                if (InteractManagerV2.Instance != null)
                {
                    InteractManagerV2.Instance.Unregister(this);
                }
            }
            return;
        }

        if (!playerInside)
        {
            playerInside = true;
            if (InteractManagerV2.Instance != null)
            {
                InteractManagerV2.Instance.Register(this);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag) && !other.gameObject.name.Contains("Player")) return;

        playerInside = false;
        if (InteractManagerV2.Instance != null)
        {
            InteractManagerV2.Instance.Unregister(this);
        }
    }

    public virtual void Execute()
    {
        // 1. NPC Quest
        NPCQuestHandler questHandler = GetComponent<NPCQuestHandler>();
        if (questHandler != null)
        {
            questHandler.ShowInitialDialogue();
            return;
        }

        // 2. NPC Thoại thường
        NPCDialogue dialogue = GetComponent<NPCDialogue>();
        if (dialogue != null)
        {
            if (DialogueUI.Instance != null)
            {
                DialogueUI.Instance.Show(dialogue.GetDialogue());
            }
            return;
        }

        // 3. Tương tác mở Menu
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

        SendMessage("OnInteract", SendMessageOptions.DontRequireReceiver);
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

        playerInside = false;
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

        playerInside = false;
        if (InteractManagerV2.Instance != null)
        {
            InteractManagerV2.Instance.Unregister(this);
        }
    }
}