using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; // Yêu cầu dự án đã có DOTween

public class LobbyHUDTopBar : MonoBehaviour
{
    public static LobbyHUDTopBar Instance;

    [Header("Visual Groups (Dùng để bật/ẩn nhanh theo cụm nếu cần)")]
    [SerializeField] private GameObject levelGroup;      // Cụm chứa Level, Avatar, Thanh Exp
    [SerializeField] private GameObject currencyGroup;   // Cụm chứa Gem, Rune Shard

    [Header("Gem & Rune Shard UI")]
    [SerializeField] private TMP_Text gemText;
    [SerializeField] private TMP_Text runeShardText;    // Text hiển thị Mảnh Cổ Tự

    [Header("Account Level UI")]
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text userNameText;
    [SerializeField] private TMP_Text expText;
    [SerializeField] private Slider expBar;

    // Các biến phục vụ cho hiệu ứng chạy số và tăng Slider mượt của DOTween
    private float animatedCurrentExp;
    private int cachedRequiredExp;
    private Tween activeExpTween;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Khi vừa vào Game, nạp ngay dữ liệu tiền tệ lộ thiên lên màn hình
        RefreshCurrencyUI();
        
        // Tự động đồng bộ cấp độ ban đầu khi HUD khởi tạo
        if (AccountLevelManager.Instance != null)
        {
            int currentLv = AccountLevelManager.Instance.GetLevel();
            if (levelText != null) levelText.SetTextSafe(currentLv.ToString());
        }
    }

    /// <summary>
    /// Hàm làm mới hiển thị số liệu Tiền tệ (Gem và Rune Shard)
    /// </summary>
    public void RefreshCurrencyUI()
    {
        // 1. Cập nhật số lượng Gem từ GemManager
        if (gemText != null && GemManager.Instance != null)
        {
            gemText.SetTextSafe(GemManager.Instance.GetCurrentGem().ToString("N0"));
        }

        // 2. Cập nhật số lượng Rune Shard từ dữ liệu Save ngầm
        if (runeShardText != null && SaveLoadManager.Instance != null && SaveLoadManager.Instance.SaveData != null)
        {
            runeShardText.SetTextSafe(SaveLoadManager.Instance.SaveData.runeShards.ToString("N0"));
        }

        // 3. Cập nhật Tên người chơi
        if (userNameText != null)
        {
            userNameText.SetTextSafe(PlayerPrefs.GetString("DisplayName", "Unknown"));
        }
    }

    /// <summary>
    /// Hàm chạy hiệu ứng tăng thanh kinh nghiệm mượt mà bằng DOTween (Kế thừa từ AccountLevelUI cũ)
    /// </summary>
    public void RefreshLevelUI(int level, int currentExp, int requiredExp)
    {
        if (levelText != null) levelText.SetTextSafe(level.ToString());
        if (requiredExp <= 0) return;

        cachedRequiredExp = requiredExp;
        float targetValue = (float)currentExp / requiredExp;

        if (activeExpTween != null) activeExpTween.Kill();
        Sequence expSequence = DOTween.Sequence();

        if (expBar != null)
        {
            expSequence.Join(expBar.DOValue(targetValue, 0.5f).SetEase(Ease.OutQuad));
        }

        if (expText != null)
        {
            expSequence.Join(DOTween.To(() => animatedCurrentExp, x => animatedCurrentExp = x, currentExp, 0.5f)
                .SetEase(Ease.OutQuad)
                .OnUpdate(() =>
                {
                    expText.SetTextSafe($"{(int)animatedCurrentExp:N0} / {cachedRequiredExp:N0}");
                }));
        }
        activeExpTween = expSequence;
    }

    // =========================================================================
    // CÁC CHẾ ĐỘ ĐIỀU KHIỂN BẬT/ẨN THEO NHU CẦU CỦA CÁC PANEL UI KHÁC
    // =========================================================================

    public void ShowFullHUD()
    {
        if (levelGroup != null) levelGroup.SetActive(true);
        if (currencyGroup != null) currencyGroup.SetActive(true);
        RefreshCurrencyUI();
    }

    public void ShowCurrencyOnly()
    {
        if (levelGroup != null) levelGroup.SetActive(false); // Ẩn cụm cấp độ đi khi mở Hòm đồ/Gacha cho thoáng
        if (currencyGroup != null) currencyGroup.SetActive(true);  // Giữ lại tiền để xem biến động
        RefreshCurrencyUI();
    }
}