using UnityEngine;
using TMPro;

public class RunResultSummary : MonoBehaviour
{
    public static RunResultSummary Instance;

    [Header("UI Panels")]
    [SerializeField] private GameObject summaryPanel; 

    [Header("Summary Texts")]
    [SerializeField] private TMP_Text txtStatsNotify; // Hiển thị thông số trận đấu (Tiếng Anh)
    [SerializeField] private TMP_Text txtRewards;     // Hiển thị phần thưởng xịn (Tiếng Anh)

    private int calculatedGem;
    private int calculatedExp;
    private int calculatedShards; // MỚI: Biến tạm tính toán số Shards nhận được

    private void Awake()
    {
        Instance = this;
        if (summaryPanel != null) summaryPanel.SetActive(false); 
    }

    public void DisplaySummary(int kills, int score)
    {
        if (summaryPanel == null) return;

        // 1. Tính toán tài nguyên cơ bản (Điều chỉnh lại tỷ lệ quy đổi)
        calculatedGem = score / 20; // Giảm bớt lượng Gem rớt ra từ Dungeon
        calculatedExp = kills * 15;
        calculatedShards = score * 2; // Tặng nhiều Mảnh Cổ Tự để người chơi làm nguyên liệu nâng cấp ngọc

        int upgradeShards = Mathf.Clamp(kills / 10, 1, 5);     

        // 2. Hiển thị thông báo dạng TIẾNG ANH chuẩn chỉnh, không lo lỗi font
        if (txtStatsNotify != null)
        {
            txtStatsNotify.text = $"<b><color=#FFCC00>VICTORY ACHIEVED</color></b>\n\n" +
                                  $"Monsters Vanquished: <color=#FF3333>{kills}</color>\n" +
                                  $"Battle Score: <color=#FFFF66>{score}</color>";
        }

        if (txtRewards != null)
        {
            txtRewards.text = $"<b><color=#00FFCC>REWARDS ACQUIRED</color></b>\n\n" +
                              $"- Crystals: <color=#33FFFF>+{calculatedGem}</color>\n" +
                              $"- Rune Shards: <color=#CC66FF>+{calculatedShards}</color>\n" + // ĐỔI HIỂN THỊ THÀNH TIỀN MỚI
                              $"- Account EXP: <color=#33FF33>+{calculatedExp}</color>\n" +
                              $"- Weapon Shards: <color=#FFA500>+{upgradeShards}</color>";
        }

        // Bật bảng UI kết quả
        summaryPanel.SetActive(true);

        // 3. Gửi dữ liệu cốt lõi về cho RunManager xử lý (Đồng bộ hàm nhận thưởng mới truyền thêm biến Shards)
        if (RunManager.Instance != null)
        {
            RunManager.Instance.SetPendingRewards(calculatedGem, calculatedExp, calculatedShards);
        }

        // Tặng thêm Ngọc Cổ Tự ngẫu nhiên (Giữ nguyên logic của bạn)
        if (RuneInventoryManager.Instance != null)
        {
            RuneColor randomColor = (RuneColor)Random.Range(0, 3);
            RuneRarity randomRarity = (RuneRarity)Random.Range(0, 3);
            RuneData newRune = new RuneData(randomColor, randomRarity)
            {
                runeName = $"Relic: {randomRarity} {randomColor}",
                runeLore = "An ancient relic recovered from the deep nightmare."
            };
            RuneInventoryManager.Instance.AddRune(newRune);
        }
    }

    // Gắn vào nút bấm quay về sảnh
    public void OnConfirmAndReturn()
    {
        if (RunManager.Instance != null)
        {
            RunManager.Instance.ReturnToLobby();
        }
    }
}