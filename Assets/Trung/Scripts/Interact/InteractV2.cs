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
    #endregion

    private void OnEnable()
    {
        CheckFeatureUnlockStatus();
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestUpdated += CheckFeatureUnlockStatus;
        }
    }

    #region Trigger
    private void Reset()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    public void CheckFeatureUnlockStatus()
    {
        if (openPanel)
        {
            if (menuType == MenuType.LobbyRuneInventoryMenu || menuType == MenuType.LobbyCharacterMenu)
            {
                bool isUnlocked = QuestManager.Instance != null && QuestManager.Instance.IsMenuUnlocked(menuType);
                Collider col = GetComponent<Collider>();
                if (col != null)
                {
                    col.enabled = isUnlocked;
                }

                if (!isUnlocked)
                {
                    playerInside = false;
                    if (InteractManagerV2.Instance != null)
                    {
                        InteractManagerV2.Instance.Unregister(this);
                    }
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag) && !other.gameObject.name.Contains("Player")) return;

        if (openPanel && (menuType == MenuType.LobbyRuneInventoryMenu || menuType == MenuType.LobbyCharacterMenu))
        {
            if (QuestManager.Instance != null && !QuestManager.Instance.IsMenuUnlocked(menuType))
            {
                return;
            }
        }

        playerInside = true;
        if (InteractManagerV2.Instance != null)
        {
            InteractManagerV2.Instance.Register(this);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag(playerTag) && !other.gameObject.name.Contains("Player")) return;

        if (openPanel && (menuType == MenuType.LobbyRuneInventoryMenu || menuType == MenuType.LobbyCharacterMenu))
        {
            if (QuestManager.Instance != null && !QuestManager.Instance.IsMenuUnlocked(menuType))
            {
                return;
            }
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
    #endregion

    #region Execute
    public virtual void Execute()
    {
        if (!playerInside || (InteractManagerV2.Instance != null && InteractManagerV2.Instance.IsBusy))
        {
            return;
        }

        NPCQuestHandler questHandler = GetComponent<NPCQuestHandler>();
        if (questHandler != null)
        {
            questHandler.ShowInitialDialogue();
            return;
        }

        NPCDialogue dialogue = GetComponent<NPCDialogue>();
        if (dialogue != null)
        {
            if (DialogueUI.Instance != null)
            {
                DialogueUI.Instance.Show(dialogue.GetDialogue());
            }
            return;
        }

        if (openPanel)
        {
            if (menuType == MenuType.LobbyRuneInventoryMenu || menuType == MenuType.LobbyCharacterMenu)
            {
                if (QuestManager.Instance != null && !QuestManager.Instance.IsMenuUnlocked(menuType))
                {
                    return;
                }

                // Chỉ hoàn thành Quest khi người chơi thực sự ấn tương tác mở panel
                if (menuType == MenuType.LobbyRuneInventoryMenu && QuestManager.Instance != null)
                {
                    QuestManager.Instance.AddQuestProgress("QUEST_OPEN_RUNE_INVENTORY", 1);
                }

                if (menuType == MenuType.LobbyCharacterMenu && QuestManager.Instance != null)
                {
                    QuestManager.Instance.AddQuestProgress("QUEST_SELECT_CHARACTER", 1);
                }
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
    #endregion

    #region Selected
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
    #endregion

    #region Public
    public void SetInteractText(string value)
    {
        interactText = value;
        if (InteractManagerV2.Instance != null)
        {
            InteractManagerV2.Instance.ForceRefresh();
        }
    }
    #endregion

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