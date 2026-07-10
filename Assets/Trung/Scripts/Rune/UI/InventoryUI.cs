using System.Collections.Generic;
using UnityEngine;
using TMPro;    
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance;
    
    [Header("Panel Root")]
    [SerializeField] private GameObject inventoryPanel;

    [Header("UI Grid Layout Content")]
    [SerializeField] private Transform contentParent;
    [SerializeField] private ScrollRect scrollRect;

    [Header("Card Prefab Template")]
    [SerializeField] private RuneCardUI cardPrefab;

    [Header("Select Mode Layout configuration (Sử dụng Button Mới)")]
    [SerializeField] private GameObject selectAllButtonObj;         // Kéo thả độc lập nút Select All vào đây
    [SerializeField] private GameObject bulkDeleteButtonObj;       // THÊM Ô NÀY: Kéo thả độc lập nút Delete loạt vào đây
    [SerializeField] private TMP_Text selectModeToggleButtonText;    // Text của nút Select Mode gốc
    private bool isSelectModeActive = false;                         

    [Header("NEW: Fusion Layout Toggle")]
    [SerializeField] private GameObject runeEquipPanelObj;  
    [SerializeField] private GameObject runeFusionPanelObj; 
    [SerializeField] private TMP_Text fusionToggleBtnText;
    [SerializeField] private Toggle fusionToggle;      
    [Header("Inventory Capacity Settings")]
    [SerializeField] private TMP_Text capacityText; // Kéo thả văn bản hiển thị "Slots: 0/100" vào đây
    [SerializeField] private int maxInventorySlots = 100; // Giới hạn hòm đồ tối đa là 100 ô

    // Danh sách lưu trữ card để tái sử dụng (Object Pooling)
    private List<RuneCardUI> pooledCards = new List<RuneCardUI>();    

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void OnEnable()
    {
        if (runeEquipPanelObj != null) runeEquipPanelObj.SetActive(true);
        if (runeFusionPanelObj != null) runeFusionPanelObj.SetActive(false);
        
        try
        {
            if (fusionToggleBtnText != null) fusionToggleBtnText.text = "Fusion Mode";
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[InventoryUI Protect] Chặn lỗi Font chữ: {e.Message}");
        }

        if (fusionToggle != null) fusionToggle.isOn = false;
        if (RuneFilterPanel.Instance != null) RuneFilterPanel.Instance.ResetFilterToDefault();

        DisableSelectMode();
        RefreshInventory();
    }

    public void OpenInventory()
    {
        if (inventoryPanel != null) inventoryPanel.SetActive(true);
        RefreshInventory();
    }

    public void CloseInventory()
    {
        if (fusionToggle != null) fusionToggle.isOn = false;
        if (runeFusionPanelObj != null) runeFusionPanelObj.SetActive(false);
        if (runeEquipPanelObj != null) runeEquipPanelObj.SetActive(true);
        
        if (RuneFusionUI.Instance != null) RuneFusionUI.Instance.ClearFusionSlots();
        
        if (RuneFilterPanel.Instance != null) 
        {
            RuneFilterPanel.Instance.ResetFilterToDefault();
            RuneFilterPanel.Instance.CloseFilterPanel();
        }
        
        DisableSelectMode();

        if (inventoryPanel != null) inventoryPanel.SetActive(false);
    }

    /// <summary>
    /// Gắn hàm này vào sự kiện OnClick() của nút Button Select Mode gốc
    /// </summary>
    public void ToggleSelectMode()
    {
        isSelectModeActive = !isSelectModeActive;

        // Bật hoặc Tắt độc lập cho từng nút ngoài giao diện theo trạng thái Active
        if (selectAllButtonObj != null) 
            selectAllButtonObj.SetActive(isSelectModeActive);
            
        if (bulkDeleteButtonObj != null) 
            bulkDeleteButtonObj.SetActive(isSelectModeActive);

        try
        {
            if (selectModeToggleButtonText != null)
                selectModeToggleButtonText.text = isSelectModeActive ? "Cancel Select" : "Select Mode";
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[InventoryUI Protect] Chặn lỗi Font chữ nút Select: {e.Message}");
        }

        RefreshInventory();
    }

    private void DisableSelectMode()
    {
        isSelectModeActive = false;
        
        // Ẩn sạch cả 2 nút độc lập khi thoát chế độ chọn đồ
        if (selectAllButtonObj != null) selectAllButtonObj.SetActive(false);
        if (bulkDeleteButtonObj != null) bulkDeleteButtonObj.SetActive(false);
        
        try
        {
            if (selectModeToggleButtonText != null) selectModeToggleButtonText.text = "Select Mode";
        }
        catch {}
    }

    public void SmartSelectAllVisibleRunes()
    {
        if (!isSelectModeActive) return;

        RuneCardUI[] spawnedCards = contentParent.GetComponentsInChildren<RuneCardUI>();
        foreach (RuneCardUI card in spawnedCards)
        {
            if (card != null) card.SetSelected(true); 
        }

        if (LobbyNotifyManager.Instance != null)
        {
            LobbyNotifyManager.Instance.ShowNotify("All visible filtered runes highlighted!", Color.yellow);
        }
    }

    /// <summary>
    /// TÍNH NĂNG PHÂN RÃ LOẠT: Chỉ rã những viên ngọc đang xuất hiện và được người chơi tích chọn V viền vàng
    /// </summary>
    public void DismantleSelectedRunesLoat()
    {
        if (!isSelectModeActive) return;

        RuneCardUI[] cards = contentParent.GetComponentsInChildren<RuneCardUI>();
        bool hasDeletedAny = false;
        int totalRefundShards = 0; // ĐỔI BIẾN: Chuyển sang lưu trữ mảnh ngọc hoàn lại

        for (int i = cards.Length - 1; i >= 0; i--)
        {
            if (!cards[i].IsSelected()) continue;

            RuneData runeData = cards[i].GetRuneData();
            if (runeData == null) continue;

            // Ép tháo trang bị ma khỏi nhân vật Layout sảnh trước khi xóa
            CharacterType[] allChars = (CharacterType[])System.Enum.GetValues(typeof(CharacterType));
            foreach (CharacterType charType in allChars)
            {
                var build = CharacterManager.Instance.GetCharacterRuneBuild(charType);
                if (build != null)
                {
                    for (int slot = 0; slot < build.equippedRuneIDs.Length; slot++)
                    {
                        if (build.equippedRuneIDs[slot] == runeData.runeID) build.equippedRuneIDs[slot] = ""; 
                    }
                }
            }

            int refundAmount = 0;
            switch (runeData.runeRarity)
            {
                case RuneRarity.Common: refundAmount = 50; break;
                case RuneRarity.Rare: refundAmount = 120; break;
                case RuneRarity.Epic: refundAmount = 300; break;
                case RuneRarity.Legendary: refundAmount = 800; break;
            }

            totalRefundShards += refundAmount;

            if (RuneInventoryManager.Instance != null)
            {
                RuneInventoryManager.Instance.RemoveRune(runeData.runeID);
            }

            hasDeletedAny = true;
        }

        if (hasDeletedAny)
        {
            // SỬA TẠI ĐÂY: Thay vì cộng Gem, ta tiến hành cộng Shards vào tài khoản
            if (RuneShardManager.Instance != null)
            {
                RuneShardManager.Instance.AddShards(totalRefundShards);
            }

            Debug.Log($"<color=#CC66FF><b>[PHÂN RÃ LOẠT]</b> Đã giải phóng ô trang bị và rã loạt ngọc. Hoàn trả +{totalRefundShards} Rune Shards.</color>");
            if (LobbyNotifyManager.Instance != null)
                LobbyNotifyManager.Instance.ShowNotify($"Bulk dismantle complete! Gained +{totalRefundShards} Rune Shards.", Color.green);

            if (RuneEquipUI.Instance != null) RuneEquipUI.Instance.RefreshEquipUI();
            if (LobbyStatManager.Instance != null) LobbyStatManager.Instance.RecalculateStats();
        }

        RefreshInventory();
    }

    public void RefreshInventory()
    {
        // 1. Cập nhật hiển thị sức chứa hòm đồ thực tế (Ví dụ: Slots: 45 / 100)
        if (capacityText != null && RuneInventoryManager.Instance != null)
        {
            int currentCount = RuneInventoryManager.Instance.runes.Count;
            
            try
            {
                capacityText.text = $"Slots: {currentCount} / {maxInventorySlots}";
                // Nếu hòm đồ sắp đầy (trên 90%), đổi sang màu đỏ để cảnh báo người chơi
                capacityText.color = (currentCount >= maxInventorySlots * 0.9f) ? Color.red : Color.white;
            }
            catch {}
        }

        // 2. Ẩn toàn bộ card cũ trong pool đi trước khi quét bộ lọc mới
        for (int i = 0; i < pooledCards.Count; i++)
        {
            if (pooledCards[i] != null) pooledCards[i].gameObject.SetActive(false);
        }

        if (RuneInventoryManager.Instance == null) return;

        List<RuneData> sortedRunes = new List<RuneData>(RuneInventoryManager.Instance.runes);
        sortedRunes.Sort((a, b) => b.runeRarity.CompareTo(a.runeRarity));

        int visibleCardCount = 0; // Biến đếm số lượng card thỏa mãn bộ lọc hiện tại

        try
        {
            for (int i = 0; i < sortedRunes.Count; i++)
            {
                if (RuneFilterPanel.Instance != null)
                {
                    if (!RuneFilterPanel.Instance.EvaluateRuneFilter(sortedRunes[i])) continue;
                }

                // Thực hiện Pooling: Cấp phát hoặc tái sử dụng Card
                GetOrCreatePooledCard(sortedRunes[i], visibleCardCount);
                visibleCardCount++;
            }
            Canvas.ForceUpdateCanvases();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("[InventoryUI pooling] Bỏ qua lỗi render: " + e.Message);
        }

        if (scrollRect != null) scrollRect.verticalNormalizedPosition = 1f;
    }

    /// <summary>
    /// Thuật toán Object Pooling: Tái sử dụng GameObject cũ giúp hòm đồ mượt mà 100%
    /// </summary>
    private void GetOrCreatePooledCard(RuneData runeData, int index)
    {
        RuneCardUI card;

        // Nếu trong danh sách pool đã có sẵn card cũ đang rảnh (bị ẩn), lấy ra dùng lại
        if (index < pooledCards.Count)
        {
            card = pooledCards[index];
            if (card == null) // Phòng trường hợp object bị mất tham chiếu ngoài ý muốn
            {
                card = Instantiate(cardPrefab, contentParent);
                pooledCards[index] = card;
            }
        }
        else
        {
            // Nếu hòm đồ hiển thị nhiều hơn số lượng card đang lưu trong pool, sinh thêm card mới gối đầu
            card = Instantiate(cardPrefab, contentParent);
            pooledCards.Add(card);
        }

        card.gameObject.SetActive(true);
        card.Setup(runeData, false);

        // Đồng bộ trạng thái Select Mode
        var selectField = card.GetType().GetField("isDeleteMode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (selectField != null) selectField.SetValue(card, isSelectModeActive);

        card.UpdateSelectModeVisual();
    }

    private void SpawnCard(RuneData runeData)
    {
        RuneCardUI card = Instantiate(cardPrefab, contentParent);
        card.Setup(runeData, false);
        
        var selectField = card.GetType().GetField("isDeleteMode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (selectField != null) selectField.SetValue(card, isSelectModeActive);
        
        card.UpdateSelectModeVisual();
    }

    public void OpenFilterPanel()
    {
        if (RuneFilterPanel.Instance != null) RuneFilterPanel.Instance.OpenFilterPanel();
    }

    public void CloseFilterPanel()
    {
        if (RuneFilterPanel.Instance != null) RuneFilterPanel.Instance.CloseFilterPanel();
    }

    public void ToggleFusionMode()
    {
        if (runeEquipPanelObj == null || runeFusionPanelObj == null || fusionToggle == null) return;

        bool isFusionOpening = fusionToggle.isOn;
        runeEquipPanelObj.SetActive(!isFusionOpening);
        runeFusionPanelObj.SetActive(isFusionOpening);

        try
        {
            if (fusionToggleBtnText != null) fusionToggleBtnText.text = isFusionOpening ? "Fusion Mode" : "Equip Mode";
        }
        catch {}

        if (RuneFusionUI.Instance != null) RuneFusionUI.Instance.ClearFusionSlots();
        if (!isFusionOpening && RuneEquipUI.Instance != null) RuneEquipUI.Instance.RefreshEquipUI();
    }
}