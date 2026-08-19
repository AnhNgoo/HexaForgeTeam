using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class LeaderboardUI : MonoBehaviour
{
    [System.Serializable]
    public class LeaderboardTabItem
    {
        public LeaderboardTab tab;
        public Button button;
        public GameObject selectedLine;
        public TMP_Text text;
        [HideInInspector] public EventTrigger trigger;
    }

    [Header("Panel")]
    [SerializeField] private GameObject panel;

    [Header("My Info")]
    [SerializeField] private TMP_Text myRankText;
    [SerializeField] private TMP_Text myScoreText;
    [SerializeField] private TMP_Text myDetailText;

    [Header("Content")]
    [SerializeField] private Transform content;

    [Header("Prefab")]
    [SerializeField] private GameObject leaderboardItemPrefab;

    [Header("Tab Items (With Hover/Selected Line)")]
    [SerializeField] private List<LeaderboardTabItem> tabItems = new List<LeaderboardTabItem>();

    [Header("Tab Visual Config")]
    [SerializeField] private Color normalTextColor = Color.white;
    [SerializeField] private Color selectedTextColor = new Color(1f, 0.85f, 0.2f);

    private List<LeaderboardItemUI> activeItems = new List<LeaderboardItemUI>();
    private LeaderboardTab currentTab = LeaderboardTab.Power;

    private void Awake()
    {
        InitTabHoverTriggers();
    }

    private void Start()
    {
        for (int i = 0; i < tabItems.Count; i++)
        {
            var item = tabItems[i];
            if (item != null && item.button != null)
            {
                LeaderboardTab tabType = item.tab;
                item.button.onClick.RemoveAllListeners();
                item.button.onClick.AddListener(() => OnTabSelected(tabType));
            }
        }
    }

    private void InitTabHoverTriggers()
    {
        for (int i = 0; i < tabItems.Count; i++)
        {
            int index = i;
            var tab = tabItems[index];
            if (tab == null || tab.button == null) continue;

            EventTrigger trigger = tab.button.gameObject.GetComponent<EventTrigger>();
            if (trigger == null) trigger = tab.button.gameObject.AddComponent<EventTrigger>();
            tab.trigger = trigger;

            trigger.triggers.Clear();

            // Hover chuột vào -> Hiện line nếu tab chưa được chọn
            EventTrigger.Entry enterEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            enterEntry.callback.AddListener((data) => { SetTabHoverVisual(index, true); });
            trigger.triggers.Add(enterEntry);

            // Rê chuột ra -> Thu line lại nếu tab không active
            EventTrigger.Entry exitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            exitEntry.callback.AddListener((data) => { SetTabHoverVisual(index, false); });
            trigger.triggers.Add(exitEntry);

            if (tab.selectedLine != null)
            {
                tab.selectedLine.SetActive(false);
            }
        }
    }

    private void SetTabHoverVisual(int index, bool isHovered)
    {
        if (index < 0 || index >= tabItems.Count) return;
        var tab = tabItems[index];
        if (tab == null) return;

        if (tab.tab == currentTab) return;

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
            tab.text.color = isHovered ? selectedTextColor : normalTextColor;
        }
    }

    private void Update()
    {
        if (panel != null && !panel.activeSelf) return;

        // Phím tắt 1, 2, 3, 4 chuyển nhanh các Tab
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            OnTabSelected(LeaderboardTab.Power);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            OnTabSelected(LeaderboardTab.Achievement);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
        {
            OnTabSelected(LeaderboardTab.Hunt);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4))
        {
            OnTabSelected(LeaderboardTab.Run);
        }
    }

    public void OpenPanel()
    {
        if (panel != null) panel.SetActive(true);

        if (LeaderboardManager.Instance != null)
        {
            LeaderboardManager.Instance.UpdateAllStatistics();
        }

        OnTabSelected(LeaderboardTab.Power);
    }

    public void ClosePanel()
    {
        if (panel != null) panel.SetActive(false);
    }

    public void OnTabSelected(LeaderboardTab tab)
    {
        currentTab = tab;

        if (LeaderboardManager.Instance != null)
        {
            LeaderboardManager.Instance.SetCurrentTab(tab);
            LeaderboardManager.Instance.UpdateAllStatistics();
        }

        RefreshTabUIVisual(tab);

        if (LeaderboardManager.Instance != null)
        {
            LeaderboardManager.Instance.LoadLeaderboard(this);
        }
    }

    private void RefreshTabUIVisual(LeaderboardTab activeTab)
    {
        for (int i = 0; i < tabItems.Count; i++)
        {
            var item = tabItems[i];
            if (item == null) continue;

            bool isSelected = (item.tab == activeTab);

            if (item.selectedLine != null)
            {
                item.selectedLine.transform.DOKill();
                if (isSelected)
                {
                    item.selectedLine.SetActive(true);
                    item.selectedLine.transform.DOScaleX(1f, 0.2f).SetUpdate(true);
                }
                else
                {
                    item.selectedLine.transform.DOScaleX(0f, 0.15f).SetUpdate(true).OnComplete(() =>
                    {
                        item.selectedLine.SetActive(false);
                    });
                }
            }

            if (item.text != null)
            {
                item.text.color = isSelected ? selectedTextColor : normalTextColor;
            }
        }
    }

    public void SetMyInfo(int rank, int score, string detailInfo = "")
    {
        if (myRankText != null) myRankText.SetTextSafe($"Rank #{rank}");

        if (myScoreText != null)
        {
            DOVirtual.Int(0, score, 0.6f, (val) => {
                myScoreText.SetTextSafe($"Score : {val:N0}");
            }).SetEase(Ease.OutCubic);
        }

        if (myDetailText != null)
        {
            myDetailText.gameObject.SetActive(!string.IsNullOrEmpty(detailInfo));
            myDetailText.SetTextSafe(detailInfo);
        }
    }

    public void ClearItems()
    {
        for (int i = 0; i < activeItems.Count; i++)
        {
            if (activeItems[i] != null)
            {
                activeItems[i].gameObject.SetActive(false);
            }
        }
    }

    public void AddItem(int rank, string playerName, int score, string detailInfo = "", bool isMe = false)
    {
        int index = rank - 1;
        LeaderboardItemUI itemUI = null;

        if (index < activeItems.Count && activeItems[index] != null)
        {
            itemUI = activeItems[index];
        }
        else
        {
            GameObject itemObj = Instantiate(leaderboardItemPrefab, content);
            itemUI = itemObj.GetComponent<LeaderboardItemUI>();
            activeItems.Add(itemUI);
        }

        if (itemUI != null)
        {
            itemUI.gameObject.SetActive(true);
            itemUI.transform.SetSiblingIndex(index);
            itemUI.Setup(rank, playerName, score, detailInfo, isMe);
        }
    }
}