using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RuneRerollUI : MonoBehaviour
{
    public static RuneRerollUI Instance;

    [Header("UI Panels")]
    [SerializeField] private GameObject rerollPanelRoot;

    [Header("Ingredients Display")]
    [SerializeField] private Transform cardPreviewParent;
    [SerializeField] private RuneCardUI cardPrefabSample;

    [Header("Affix Row Container (Nơi chứa các dòng để chọn)")]
    [SerializeField] private Transform affixRowsContainer;
    [SerializeField] private Button affixRowButtonPrefab; // Prefab Nút bấm đại diện cho 1 dòng Affix

    [Header("Action Buttons")]
    [SerializeField] private Button rerollActionButton;
    [SerializeField] private Button closePanelButton;

    [Header("Status Texts")]
    [SerializeField] private TMP_Text costText;
    [SerializeField] private TMP_Text statusNoticeText;

    [Header("Cheat/Cost Settings")]
    [SerializeField] private int baseRerollCost = 150; // Tẩy dòng tốn 150 mảnh mặc định

    [Header("Target Reroll Settings (Chức năng định hướng dòng)")]
    [SerializeField] private Toggle useTargetRerollToggle;     // Checkbox dùng để bật/tắt chế độ chọn dòng (Giao diện Tiếng Anh: Target Reroll)
    [SerializeField] private TMP_Dropdown statTargetDropdown;  // Bảng thả xuống chọn thuộc tính đích hướng

    // Bộ đệm lưu trữ dữ liệu tính năng
    private RuneData targetRuneData;
    private int selectedAffixIndex = -1; // Chỉ số dòng đang chọn để tẩy (-1 là chưa chọn)
    private bool isAnimating = false;    // Trạng thái đang chạy hiệu ứng gacha cuộn chữ

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (rerollPanelRoot != null) rerollPanelRoot.SetActive(false);

        // Đăng ký sự kiện nút bấm
        if (rerollActionButton != null) rerollActionButton.onClick.AddListener(OnRerollActionButtonPressed);
        if (closePanelButton != null) closePanelButton.onClick.AddListener(ClosePanel);

        // Đăng ký sự kiện thay đổi Dropdown/Toggle
        if (useTargetRerollToggle != null) useTargetRerollToggle.onValueChanged.AddListener((x) => UpdateCostVisual());
        if (statTargetDropdown != null) statTargetDropdown.onValueChanged.AddListener((x) => UpdateCostVisual());

        PopulateDropdownStats();
    }

    /// <summary>
    /// Hàm công khai dùng để gọi lật mở Panel Tẩy dòng từ Card bên ngoài sảnh
    /// </summary>
    public void OpenPanel(RuneData rune)
    {
        if (rune == null) return;
        if (isAnimating) return; // Đang quay số cấm mở đè

        targetRuneData = rune;
        selectedAffixIndex = -1; // Reset dòng tích chọn

        if (rerollPanelRoot != null) rerollPanelRoot.SetActive(true);

        // Tạo bản xem trước Card ở góc Panel
        ClearContainer(cardPreviewParent);
        if (cardPrefabSample != null)
        {
            RuneCardUI previewCard = Instantiate(cardPrefabSample, cardPreviewParent);
            
            // FIX 1: Đổi tham số thứ 2 thành 'false' để bài ngửa mặt lên ngay lập tức
            previewCard.Setup(rune, false); 

            // FIX 2: Ép RectTransform của lá bài nằm khít chuẩn 100% vào chính giữa tâm ô Preview
            RectTransform rect = previewCard.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = Vector2.zero;
                rect.localPosition = Vector3.zero;
            }
            previewCard.transform.localScale = Vector3.one; // Trả lại kích thước scale gốc chuẩn

            if (previewCard.GetComponent<CanvasGroup>() != null) 
                previewCard.GetComponent<CanvasGroup>().blocksRaycasts = false;
        }

        if (statusNoticeText != null) statusNoticeText.text = "Select an Affix line from below to reroll.";

        RefreshAffixRows();
        UpdateCostVisual();
    }

    public void ClosePanel()
    {
        if (isAnimating) return; // Đang chạy hiệu ứng cuộn chữ chặn không cho tắt UI ngang xương

        if (rerollPanelRoot != null) rerollPanelRoot.SetActive(false);
        targetRuneData = null;
        selectedAffixIndex = -1;

        // Làm tươi lại giao diện hòm đồ chính để cập nhật chỉ số mới
        if (InventoryUI.Instance != null) InventoryUI.Instance.RefreshInventory();
    }

    private void RefreshAffixRows()
    {
        ClearContainer(affixRowsContainer);
        if (targetRuneData == null || affixRowButtonPrefab == null) return;

        for (int i = 0; i < targetRuneData.affixes.Count; i++)
        {
            int index = i; // Khóa chỉ số cho delegate button tránh lỗi loop index
            RuneAffixData affix = targetRuneData.affixes[i];

            Button rowBtn = Instantiate(affixRowButtonPrefab, affixRowsContainer);
            TMP_Text btnText = rowBtn.GetComponentInChildren<TMP_Text>();

            // Định dạng text hiển thị dòng thuộc tính (Ví dụ: [ATK] +45)
            string isPercent = IsPercentStat(affix.statType) ? "%" : "";
            if (btnText != null) btnText.text = $"{GetStatName(affix.statType)}: +{affix.value}{isPercent}";

            // Đổi màu viền/nền nếu dòng này đang được click chọn để chuẩn bị tẩy
            Image rowImg = rowBtn.GetComponent<Image>();
            if (rowImg != null) rowImg.color = (selectedAffixIndex == index) ? Color.yellow : Color.white;

            rowBtn.onClick.AddListener(() =>
            {
                if (isAnimating) return;
                selectedAffixIndex = index;
                RefreshAffixRows(); // Vẽ lại để đổi màu highlight vàng
                UpdateCostVisual();
            });
        }
    }

    private void UpdateCostVisual()
    {
        bool isTargetMode = useTargetRerollToggle != null && useTargetRerollToggle.isOn;
        // Chế độ Target Reroll định hướng dòng chuẩn tốn gấp 3 lần chi phí cày cuốc
        int currentCost = isTargetMode ? baseRerollCost * 3 : baseRerollCost;

        if (costText != null)
        {
            if (selectedAffixIndex == -1)
            {
                costText.text = "Cost: -- Shards"; // SỬA CHỮ GEMS THÀNH SHARDS
                if (rerollActionButton != null) rerollActionButton.interactable = false;
            }
            else
            {
                costText.text = $"Cost: <color=#FFD700>{currentCost} Shards</color>"; // SỬA CHỮ GEMS THÀNH SHARDS
                if (rerollActionButton != null) rerollActionButton.interactable = true;
            }
        }

        // Ẩn/Hiện bảng thả xuống chọn thuộc tính tương ứng theo trạng thái checkbox
        if (statTargetDropdown != null) statTargetDropdown.gameObject.SetActive(isTargetMode);
    }

    /// <summary>
    /// Sự kiện cốt lõi xử lý khi người chơi nhấn nút REROLL hành động
    /// </summary>
    private void OnRerollActionButtonPressed()
    {
        if (targetRuneData == null || selectedAffixIndex == -1 || isAnimating) return;

        bool isTargetMode = useTargetRerollToggle != null && useTargetRerollToggle.isOn;
        int totalCost = isTargetMode ? baseRerollCost * 3 : baseRerollCost;

        // =========================================================================
        // SỬA ĐỔI TIỀN TỆ: KIỂM TRA SỐ DƯ VÀ TRỪ RUNE SHARDS THAY VÌ GEM
        // =========================================================================
        if (RuneShardManager.Instance == null || !RuneShardManager.Instance.SpendShards(totalCost))
        {
            if (LobbyNotifyManager.Instance != null)
            {
                LobbyNotifyManager.Instance.ShowNotify("Not enough Rune Shards to reroll affixes!", Color.red);
            }
            return; // Khóa chặn, không cho chạy vòng lặp xáo chữ bên dưới
        }

        // Chốt dòng thuộc tính đích nếu bật chế độ định hướng định hình
        RuneStatType finalTargetStat = RuneStatType.HP;
        if (isTargetMode && statTargetDropdown != null)
        {
            finalTargetStat = (RuneStatType)statTargetDropdown.value;
        }

        // Khởi chạy Coroutine hiệu ứng xáo chữ nhấp nháy đồ họa tăng tính hồi hộp
        StartCoroutine(RerollGachaRoutine(isTargetMode, finalTargetStat));
    }

    private System.Collections.IEnumerator RerollGachaRoutine(bool isTargetMode, RuneStatType targetStat)
    {
        isAnimating = true;
        if (rerollActionButton != null) rerollActionButton.interactable = false;
        if (closePanelButton != null) closePanelButton.interactable = false;

        // Lấy tham chiếu trực tiếp text dòng đang được đập đi xây lại
        Transform selectedRow = affixRowsContainer.GetChild(selectedAffixIndex);
        TMP_Text btnText = selectedRow.GetComponentInChildren<TMP_Text>();

        float duration = 1.8f; // Chạy hiệu ứng cuộn chữ trong 1.8 giây
        float elapsed = 0f;
        float delayTick = 0.06f;

        while (elapsed < duration)
        {
            elapsed += delayTick;
            // Lấy ngẫu nhiên thuộc tính từ Pool để nhấp nháy cuộn chữ liên tục
            RuneStatType randomStat = GetRandomStatPool();
            if (btnText != null)
            {
                string ModePrefix = isTargetMode ? "LOCKING" : "ROLLING";
                btnText.text = $"<color=#FFD700>{ModePrefix} → </color>{GetStatName(randomStat)}";
            }
            yield return new WaitForSeconds(delayTick);
        }

        // === THỰC HIỆN ĐỔI CHỈ SỐ THỰC TẾ TRONG LOGIC DỮ LIỆU ===
        RuneAffixData activeAffix = targetRuneData.affixes[selectedAffixIndex];
        
        if (isTargetMode)
        {
            // Chế độ Target Mode: Ép ra chính xác loại chỉ số mà người chơi chọn trên Dropdown
            activeAffix.statType = targetStat;
        }
        else
        {
            // Chế độ Thường: Xúc xắc ngẫu nhiên thuộc tính mới
            activeAffix.statType = GetRandomStatPool();
        }

        // Tính toán lại giá trị thuộc tính mới ngẫu nhiên dựa trên phẩm chất độ hiếm của ngọc
        activeAffix.value = GenerateNewValueByRarity(targetRuneData.runeRarity, activeAffix.statType);

        // Lưu dữ liệu ngọc cổ tự vào hệ thống file cục bộ và đánh dấu dirty cloud
        if (RuneInventoryManager.Instance != null)
        {
            RuneInventoryManager.Instance.AddRune(null); // Kích hoạt lệnh gián tiếp Save/Refresh của InventoryManager bằng cách truyền null (hoặc gọi hàm lưu nếu có)
        }

        // Cập nhật lại giao diện xem trước Card sau khi chỉ số phụ đã biến đổi
        ClearContainer(cardPreviewParent);
        if (cardPrefabSample != null)
        {
            RuneCardUI previewCard = Instantiate(cardPrefabSample, cardPreviewParent);
            
            // FIX 1: Đổi tham số thứ 2 thành 'false' để bài tiếp tục ngửa mặt sau khi quay xong
            previewCard.Setup(targetRuneData, false);

            // FIX 2: Ép RectTransform nằm khít chính giữa tâm ô Preview sau khi quay xong
            RectTransform rect = previewCard.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = Vector2.zero;
                rect.localPosition = Vector3.zero;
            }
            previewCard.transform.localScale = Vector3.one; // Trả lại kích thước scale gốc chuẩn
        }

        if (statusNoticeText != null) statusNoticeText.text = "<color=#00FFCC>Affix successfully transmuted!</color>";
        if (LobbyNotifyManager.Instance != null) LobbyNotifyManager.Instance.ShowNotify("Reroll Complete!", Color.green);

        // Ép đồng bộ lại chỉ số tổng sảnh sảnh lập tức đề phòng người chơi đang đeo viên này trên người
        if (LobbyStatManager.Instance != null) LobbyStatManager.Instance.RecalculateStats();

        isAnimating = false;
        if (closePanelButton != null) closePanelButton.interactable = true;
        RefreshAffixRows();
        UpdateCostVisual();
    }

    private RuneStatType GetRandomStatPool()
    {
        // Trả về ngẫu nhiên thuộc tính trừ dòng AllStats tối thượng
        return (RuneStatType)Random.Range(0, 14);
    }

    private float GenerateNewValueByRarity(RuneRarity rarity, RuneStatType stat)
    {
        float multiplier = IsPercentStat(stat) ? 0.1f : 1.0f;
        float baseVal = rarity == RuneRarity.Common ? Random.Range(10, 25) :
                        rarity == RuneRarity.Rare ? Random.Range(25, 55) :
                        rarity == RuneRarity.Epic ? Random.Range(55, 100) : Random.Range(100, 220);

        return baseVal * multiplier;
    }

    private void PopulateDropdownStats()
    {
        if (statTargetDropdown == null) return;
        statTargetDropdown.options.Clear();

        // Nạp danh sách 14 thuộc tính cơ bản vào bảng thả xuống (Bỏ dòng AllStats đặc biệt)
        for (int i = 0; i < 14; i++)
        {
            statTargetDropdown.options.Add(new TMP_Dropdown.OptionData(GetStatName((RuneStatType)i)));
        }
        statTargetDropdown.RefreshShownValue();
    }

    private void ClearContainer(Transform container)
    {
        if (container == null) return;
        for (int i = container.childCount - 1; i >= 0; i--)
        {
            Destroy(container.GetChild(i).gameObject);
        }
    }

    private bool IsPercentStat(RuneStatType statType)
    {
        switch (statType)
        {
            case RuneStatType.HPPercent: case RuneStatType.MPPercent: case RuneStatType.StaminaPercent:
            case RuneStatType.ATKPercent: case RuneStatType.DEFPercent: case RuneStatType.CritChance:
            case RuneStatType.CritDamage: case RuneStatType.ArmorPenetration: case RuneStatType.StaminaRegen:
                return true;
        }
        return false;
    }

    private string GetStatName(RuneStatType statType)
    {
        switch (statType)
        {
            case RuneStatType.HP: return "Max HP";
            case RuneStatType.HPPercent: return "HP Modifier";
            case RuneStatType.MP: return "Max MP";
            case RuneStatType.MPPercent: return "MP Modifier";
            case RuneStatType.Stamina: return "Stamina Cap";
            case RuneStatType.StaminaPercent: return "Stamina Modifier";
            case RuneStatType.ATK: return "Attack Power";
            case RuneStatType.ATKPercent: return "Attack Modifier";
            case RuneStatType.DEF: return "Defense Rating";
            case RuneStatType.DEFPercent: return "Defense Modifier";
            case RuneStatType.CritChance: return "Critical Chance";
            case RuneStatType.CritDamage: return "Critical Damage";
            case RuneStatType.ArmorPenetration: return "Armor Penetration";
            case RuneStatType.StaminaRegen: return "Stamina Regeneration";
            case RuneStatType.AllStats: return "All Attributes";
        }
        return "Unknown Stat";
    }
}