using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class LevelUpPopupUI : LoadComponents
{
    public static LevelUpPopupUI Instance;

    [Header("Panel Root & Animation Containers")]
    [SerializeField] private GameObject levelUpPanel;
    [SerializeField] private RectTransform popupContainer; // Khung chữ nhật chính chứa UI
    [SerializeField] private CanvasGroup bgOverlayCanvasGroup; // Tấm nền mờ phía sau

    [Header("Texts")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text levelNoticeText; // Hiển thị Level (VD: LEVEL 4 -> LEVEL 5)
    [SerializeField] private TMP_Text bonusStatsText;  // Hiển thị chỉ số cộng thêm (+10 HP...)

    [Header("Reward Display")]
    [SerializeField] private CostDisplayUI rewardDisplayUI; // Kéo Prefab CostDisplayUI vào đây

    [Header("Auto Hide Settings")]
    [SerializeField] private float autoHideDelay = 2.5f; // Số giây tự động đóng Popup

    private bool isAnimating = false;

    protected override void Awake()
    {
        base.Awake();
        Instance = this;

        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Hàm Show chính: Hiển thị Level, Chỉ số cộng thêm và Cụm Icon Phần thưởng, tự động tắt sau vài giây.
    /// </summary>
    public void Show(string title, int oldLevel, int newLevel, List<CostData> rewards, string bonusText)
    {
        if (levelUpPanel == null) return;

        // Hủy lịch tự động ẩn cũ nếu có
        CancelInvoke(nameof(Hide));

        if (titleText != null) titleText.SetTextSafe(title);
        
        // Dùng dấu -> chuẩn text không bị lỗi font
        if (levelNoticeText != null) levelNoticeText.SetTextSafe($"LEVEL {oldLevel} -> <color=#00FFCC>LEVEL {newLevel}</color>");
        
        if (bonusStatsText != null) bonusStatsText.SetTextSafe(bonusText);

        if (rewardDisplayUI != null && rewards != null)
        {
            rewardDisplayUI.SetupCost(rewards);
        }

        levelUpPanel.SetActive(true);

        // HIỆU ỨNG MỞ POPUP (DOTWEEN)
        isAnimating = true;

        if (bgOverlayCanvasGroup != null)
        {
            bgOverlayCanvasGroup.alpha = 0f;
            bgOverlayCanvasGroup.DOFade(1f, 0.25f).SetUpdate(true);
        }

        if (popupContainer != null)
        {
            popupContainer.transform.localScale = Vector3.one * 0.7f;
            popupContainer.transform.DOScale(Vector3.one, 0.35f)
                .SetEase(Ease.OutBack)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    isAnimating = false;
                    // Lên lịch tự động ẩn popup sau autoHideDelay giây
                    Invoke(nameof(Hide), autoHideDelay);
                });
        }
        else
        {
            isAnimating = false;
            Invoke(nameof(Hide), autoHideDelay);
        }
    }

    // Overload cũ
    public void Show(string title, string reward)
    {
        CancelInvoke(nameof(Hide));

        if (titleText != null) titleText.SetTextSafe(title);
        if (bonusStatsText != null) bonusStatsText.SetTextSafe(reward);

        if (levelUpPanel != null) levelUpPanel.SetActive(true);

        Invoke(nameof(Hide), autoHideDelay);
    }

    public void Hide()
    {
        CancelInvoke(nameof(Hide));

        if (levelUpPanel == null || !levelUpPanel.activeSelf) return;

        isAnimating = true;

        // HIỆU ỨNG TẮT POPUP (DOTWEEN)
        if (bgOverlayCanvasGroup != null)
        {
            bgOverlayCanvasGroup.DOFade(0f, 0.2f).SetUpdate(true);
        }

        if (popupContainer != null)
        {
            popupContainer.transform.DOScale(Vector3.zero, 0.2f)
                .SetEase(Ease.InBack)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    levelUpPanel.SetActive(false);
                    isAnimating = false;
                });
        }
        else
        {
            levelUpPanel.SetActive(false);
            isAnimating = false;
        }
    }

    protected override void LoadComponent()
    {
        if (levelUpPanel == null)
        {
            levelUpPanel = transform.Find("LevelUpPanel")?.gameObject ?? gameObject;
        }

        if (titleText == null)
        {
            titleText = transform.Find("TitleText")?.GetComponent<TMP_Text>();
        }

        if (bonusStatsText == null)
        {
            bonusStatsText = transform.Find("BonusStatsText")?.GetComponent<TMP_Text>();
        }

        if (rewardDisplayUI == null)
        {
            rewardDisplayUI = GetComponentInChildren<CostDisplayUI>();
        }
    }

    protected override void LoadComponentRuntime()
    {
    }
}