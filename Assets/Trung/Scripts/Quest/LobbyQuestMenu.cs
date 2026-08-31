using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyQuestMenu : MenuBase
{
    public override MenuType menuType => MenuType.LobbyQuestMenu;

    [Header("Panel Root")]
    [SerializeField] private GameObject questPanelRoot;

    [Header("Grid Content")]
    [SerializeField] private Transform contentParent;
    [SerializeField] private QuestCardUI questCardPrefab;
    [SerializeField] private Button closeButton;

    [Header("Hotkey")]
    [SerializeField] private KeyCode toggleHotkey = KeyCode.J;

    private List<QuestCardUI> activeCards = new List<QuestCardUI>();

    protected override void LoadComponent()
    {
        if (questPanelRoot == null) questPanelRoot = transform.Find("QuestPanel")?.gameObject ?? gameObject;
        if (contentParent == null) contentParent = transform.Find("QuestPanel/Scroll View/Viewport/Content");
        if (closeButton == null) closeButton = GetComponentInChildren<Button>();
    }

    protected override void LoadComponentRuntime() { }

    private void Start()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseMenu);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleHotkey) || Input.GetKeyDown(KeyCode.Escape))
        {
            CloseMenu();
        }
    }

    public override void Open(object data = null)
    {
        base.Open(data);

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestUpdated += RefreshQuestList;
        }

        if (LobbyHUDTopBar.Instance != null)
        {
            LobbyHUDTopBar.Instance.ShowCurrencyOnly();
        }

        if (questPanelRoot != null) questPanelRoot.SetActive(true);

        RefreshQuestList();
    }

    public override void Close()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestUpdated -= RefreshQuestList;
        }

        if (questPanelRoot != null) questPanelRoot.SetActive(false);

        base.Close();

        if (LobbyHUDTopBar.Instance != null)
        {
            LobbyHUDTopBar.Instance.ShowFullHUD();
        }
    }

    public void CloseMenu()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ChangeMenu(MenuType.DefaultLobbyInputMenu);
        }

        if (InteractManagerV2.Instance != null)
        {
            InteractManagerV2.Instance.IsBusy = false;
            InteractManagerV2.Instance.ForceRefresh();
        }
    }

    public void RefreshQuestList()
    {
        ClearGrid();

        if (QuestManager.Instance == null || questCardPrefab == null || contentParent == null) return;

        List<QuestData> allQuests = QuestManager.Instance.GetAllQuests();
        for (int i = 0; i < allQuests.Count; i++)
        {
            if (allQuests[i] == null) continue;

            if (allQuests[i].state == QuestState.InProgress || allQuests[i].state == QuestState.CanClaim || allQuests[i].state == QuestState.Completed)
            {
                QuestCardUI card = Instantiate(questCardPrefab, contentParent);
                card.Setup(allQuests[i]);
                activeCards.Add(card);
            }
        }
    }

    private void ClearGrid()
    {
        for (int i = activeCards.Count - 1; i >= 0; i--)
        {
            if (activeCards[i] != null) Destroy(activeCards[i].gameObject);
        }
        activeCards.Clear();

        if (contentParent != null)
        {
            for (int i = contentParent.childCount - 1; i >= 0; i--)
            {
                Destroy(contentParent.GetChild(i).gameObject);
            }
        }
    }
}