using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class LeaderboardUI : MonoBehaviour
{
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

    [Header("Tab Buttons")]
    [SerializeField] private Button tabPowerBtn;
    [SerializeField] private Button tabAchievementBtn;
    [SerializeField] private Button tabHuntBtn;
    [SerializeField] private Button tabRunBtn;

    [Header("Tab Visual Config")]
    [SerializeField] private Color selectedColor = new Color(1f, 1f, 1f, 0.4f);
    [SerializeField] private Color normalColor = Color.white;

    private List<LeaderboardItemUI> activeItems = new List<LeaderboardItemUI>();

    private void Start()
    {
        if (tabPowerBtn != null) tabPowerBtn.onClick.AddListener(() => OnTabSelected(LeaderboardTab.Power));
        if (tabAchievementBtn != null) tabAchievementBtn.onClick.AddListener(() => OnTabSelected(LeaderboardTab.Achievement));
        if (tabHuntBtn != null) tabHuntBtn.onClick.AddListener(() => OnTabSelected(LeaderboardTab.Hunt));
        if (tabRunBtn != null) tabRunBtn.onClick.AddListener(() => OnTabSelected(LeaderboardTab.Run));
    }

    public void OpenPanel()
    {
        panel.SetActive(true);

        // ===== FORCE SYNC ĐIỂM SỐ MỚI NHẤT TRƯỚC KHI TẢI TAB =====
        if (LeaderboardManager.Instance != null)
        {
            LeaderboardManager.Instance.UpdateAllStatistics();
        }

        OnTabSelected(LeaderboardTab.Power);
    }

    public void ClosePanel()
    {
        panel.SetActive(false);
    }

    public void OnTabSelected(LeaderboardTab tab)
    {
        if (LeaderboardManager.Instance != null)
        {
            LeaderboardManager.Instance.SetCurrentTab(tab);
            // Ép đồng bộ lại dữ liệu cục bộ lên Server ngay lúc đổi Tab
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
        UpdateSingleTabState(tabPowerBtn, activeTab == LeaderboardTab.Power);
        UpdateSingleTabState(tabAchievementBtn, activeTab == LeaderboardTab.Achievement);
        UpdateSingleTabState(tabHuntBtn, activeTab == LeaderboardTab.Hunt);
        UpdateSingleTabState(tabRunBtn, activeTab == LeaderboardTab.Run);
    }

    private void UpdateSingleTabState(Button btn, bool isSelected)
    {
        if (btn == null) return;

        btn.interactable = !isSelected;

        Image btnImg = btn.GetComponent<Image>();
        if (btnImg != null)
        {
            btnImg.color = isSelected ? selectedColor : normalColor;
        }

        TMP_Text btnText = btn.GetComponentInChildren<TMP_Text>();
        if (btnText != null)
        {
            Color c = btnText.color;
            c.a = isSelected ? 0.4f : 1.0f;
            btnText.color = c;
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

    // ===== BỔ SUNG THAM SỐ `isMe` ĐỂ TRUYỀN VÀO ITEM =====
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

            // Cập nhật dữ liệu và vẽ Highlight nếu là chính mình
            itemUI.Setup(rank, playerName, score, detailInfo, isMe);
        }
    }
}