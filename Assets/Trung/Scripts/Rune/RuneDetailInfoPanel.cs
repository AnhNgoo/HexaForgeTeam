using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RuneDetailInfoPanel : MonoBehaviour
{
    public static RuneDetailInfoPanel Instance;

    [Header("Panel Root")]
    [SerializeField] private GameObject panelRoot; 

    [Header("Text Fields")]
    [SerializeField] private TMP_Text runeNameText;
    [SerializeField] private TMP_Text rarityText;
    [SerializeField] private TMP_Text colorText;
    [SerializeField] private TMP_Text affixListText; 
    [SerializeField] private TMP_Text loreText;

    [Header("Buttons")]
    [SerializeField] private Button actionButton; // Nút Đeo / Tháo ngọc (Use/Unequip)
    [SerializeField] private TMP_Text actionButtonText;
    [SerializeField] private Button deleteButton; // Nút Xóa ngọc nhanh ngoài panel
    [SerializeField] private Button rerollPanelButton;

    [Header("Rune Visuals (12 Images Ngọc)")]
    [SerializeField] private List<GameObject> runeImages = new List<GameObject>();
    
    [Header("Special Asset (Ngọc Cổ Tự Ultimate - Nếu có)")]
    [SerializeField] private GameObject ultimateRuneImage;

    // Biến lưu trữ thông tin viên ngọc đang được chọn xem chi tiết
    private RuneData currentData;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (panelRoot != null) panelRoot.SetActive(false);

        // Đăng ký sự kiện lắng nghe cho các nút bấm ngoài Panel lớn
        if (actionButton != null) actionButton.onClick.AddListener(OnActionButtonClick);
        if (deleteButton != null) deleteButton.onClick.AddListener(OnDeleteButtonClick);
        if (rerollPanelButton != null) rerollPanelButton.onClick.AddListener(() => {
            if (rerollPanelButton != null) rerollPanelButton.onClick.AddListener(() => {
    if (currentData != null && RuneRerollUI.Instance != null) {
        RuneRerollUI.Instance.OpenPanel(currentData); // ĐỔI THÀNH OpenPanel theo đúng file RuneRerollUI.cs mới
    }
});
        });
    }

    private void Update()
    {
        // Cơ chế click chuột ra ngoài vùng Panel để tự động tắt bảng
        if (panelRoot == null || !panelRoot.activeSelf) return;

        if (Input.GetMouseButtonDown(0))
        {
            RectTransform rectTransform = panelRoot.transform as RectTransform;
            if (rectTransform != null)
            {
                // Kiểm tra xem con trỏ chuột có đang nằm bên trong phân tích hình học của Panel lớn hay không
                bool clickInsidePanel = RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition, null);
                
                // FIXED: Nếu click chuột nằm HOÀN TOÀN BÊN NGOÀI bảng thông tin lớn -> Tắt bảng Info ngay lập tức
                if (!clickInsidePanel)
                {
                    ClosePanel();
                }
            }
        }
    }

    public void DisplayRuneInfo(RuneData runeData)
    {
        if (runeData == null || panelRoot == null)
        {
            ClosePanel();
            return;
        }

        currentData = runeData;
        panelRoot.SetActive(true);

        Color rarityColor = GetRarityColor(runeData.runeRarity);
        
        if (runeNameText != null)
        {
            runeNameText.text = runeData.runeName.ToUpper();
            runeNameText.color = rarityColor;
        }

        if (rarityText != null)
        {
            rarityText.text = $"RARITY: {runeData.runeRarity.ToString().ToUpper()}";
            rarityText.color = rarityColor;
        }

        if (colorText != null)
        {
            if (IsUltimateRune(runeData))
            {
                colorText.text = "ELEMENT: ORIGIN POWER";
                colorText.color = new Color(1f, 0.84f, 0f);
            }
            else
            {
                colorText.text = $"ELEMENT: {runeData.runeColor.ToString().ToUpper()}";
                colorText.color = (runeData.runeColor == RuneColor.Red) ? Color.red : (runeData.runeColor == RuneColor.Green) ? Color.green : Color.cyan;
            }
        }

        if (affixListText != null)
        {
            affixListText.text = "";
            for (int i = 0; i < runeData.affixes.Count; i++)
            {
                RuneAffixData affix = runeData.affixes[i];
                bool isPercent = IsPercentStat(affix.statType);
                string valueSign = affix.value >= 0 ? "+" : "";
                string valueFormat = isPercent ? $"{affix.value:F1}%" : $"{affix.value:F0}";
                affixListText.text += $"✦ {GetFullStatName(affix.statType)} : <color=#00FFCC>{valueSign}{valueFormat}</color>\n\n";
            }
        }

        if (loreText != null)
        {
            loreText.text = string.IsNullOrEmpty(runeData.runeLore) ? "" : $"<i>\"{runeData.runeLore}\"</i>";
        }

        // Làm mới chữ hiển thị trên nút "Use" hoặc "Unequip" dựa vào việc viên ngọc này đã đeo chưa
        UpdateActionText();
        UpdateRuneImageVisual(runeData);
    }

    public void ClosePanel()
    {
        currentData = null;
        if (panelRoot != null) panelRoot.SetActive(false);
    }

    private void UpdateActionText()
    {
        if (actionButtonText == null || currentData == null) return;

        bool isEquippedByCurrentChar = false;
        CharacterType currentType = (RuneEquipUI.Instance != null) ? RuneEquipUI.Instance.GetViewingCharacter() : CharacterManager.Instance.GetSelectedCharacter();
        CharacterRuneEquip build = CharacterManager.Instance.GetCharacterRuneBuild(currentType);

        if (build != null)
        {
            for (int i = 0; i < build.equippedRuneIDs.Length; i++)
            {
                if (build.equippedRuneIDs[i] == currentData.runeID)
                {
                    isEquippedByCurrentChar = true;
                    break;
                }
            }
        }

        actionButtonText.text = isEquippedByCurrentChar ? "UNEQUIP" : "USE";
    }

    // XỬ LÝ SỰ KIỆN: Khi click nút Lắp/Tháo đồ trên bảng Info lớn ngoài sảnh
    private void OnActionButtonClick()
    {
        if (currentData == null || RuneInventoryManager.Instance == null) return;

        bool isEquippedByCurrentChar = false;
        CharacterType currentType = (RuneEquipUI.Instance != null) ? RuneEquipUI.Instance.GetViewingCharacter() : CharacterManager.Instance.GetSelectedCharacter();
        CharacterRuneEquip build = CharacterManager.Instance.GetCharacterRuneBuild(currentType);

        if (build != null)
        {
            for (int i = 0; i < build.equippedRuneIDs.Length; i++)
            {
                if (build.equippedRuneIDs[i] == currentData.runeID)
                {
                    isEquippedByCurrentChar = true;
                    break;
                }
            }
        }

        if (!isEquippedByCurrentChar)
        {
            // Thực hiện trang bị ngọc thông minh theo slot màu sắc
            int targetSlotIndex = -1;
            if (build != null && RuneEquipUI.Instance != null)
            {
                if (IsUltimateRune(currentData))
                {
                    for (int i = 0; i < build.equippedRuneIDs.Length; i++)
                    {
                        if (string.IsNullOrEmpty(build.equippedRuneIDs[i])) { targetSlotIndex = i; break; }
                    }
                }
                else
                {
                    for (int i = 0; i < build.equippedRuneIDs.Length; i++)
                    {
                        if (string.IsNullOrEmpty(build.equippedRuneIDs[i])) // Điều kiện tiên quyết: Ô phải trống!
                        {
                            RuneColor requiredColor = RuneEquipUI.Instance.GetSlotRequiredColor(currentType, i);
                            if (currentData.runeColor == requiredColor) 
                            { 
                                targetSlotIndex = i; 
                                break; 
                            }
                        }
                    }
                }
            }

            if (targetSlotIndex == -1)
            {
                Debug.LogWarning($"<color=#FF3333><b>[PANEL LẮP NGỌC]</b> Thao tác thất bại. Nhân vật không có ô trống Tiêu Chuẩn hệ {currentData.runeColor}.</color>");
                if (LobbyNotifyManager.Instance != null) 
                    LobbyNotifyManager.Instance.ShowNotify("Rune element mismatch or no vacant slots left!", Color.yellow);
                return;
            }

            RuneInventoryManager.Instance.EquipRune(currentData, currentType);
        }
        else
        {
            RuneInventoryManager.Instance.UnequipRune(currentData, currentType);
        }

        // Làm mới lại giao diện hòm đồ lưới và cập nhật lại trạng thái nút chữ trên bảng to
        if (InventoryUI.Instance != null) InventoryUI.Instance.RefreshInventory();
        UpdateActionText();
    }

    // XỬ LÝ SỰ KIỆN: Khi click nút Xóa ngọc trực tiếp ngoài bảng Info lớn
    private void OnDeleteButtonClick()
    {
        if (currentData == null || RuneInventoryManager.Instance == null) return;

        // FIXED: Trước khi xóa thực thể dữ liệu gốc khỏi túi đồ, ta phải ép nhân vật tháo viên này ra (nếu đang đeo) để tránh kẹt dữ liệu ma (Ghost Data)
        CharacterType[] allChars = (CharacterType[])System.Enum.GetValues(typeof(CharacterType));
        foreach (CharacterType charType in allChars)
        {
            var build = CharacterManager.Instance.GetCharacterRuneBuild(charType);
            if (build != null)
            {
                for (int slot = 0; slot < build.equippedRuneIDs.Length; slot++)
                {
                    if (build.equippedRuneIDs[slot] == currentData.runeID)
                    {
                        build.equippedRuneIDs[slot] = ""; // Tháo ngọc an toàn khỏi slot trang bị
                    }
                }
            }
        }

        int refundGem = GetRefundGemByRarity(currentData.runeRarity);
        GemManager.Instance.AddGem(refundGem);

        RuneInventoryManager.Instance.RemoveRune(currentData.runeID);

        Debug.Log($"<color=#FFFF66><b>[PHÂN TÁCH NGỌC]</b> Đã hủy viên ngọc {currentData.runeName}. Hoàn lại +{refundGem} Gems vào tài khoản.</color>");
        if (LobbyNotifyManager.Instance != null) 
            LobbyNotifyManager.Instance.ShowNotify($"Rune dismantled! Recycled +{refundGem} Crystals.", Color.green);

        // Làm mới giao diện hòm đồ lưới
        if (InventoryUI.Instance != null) InventoryUI.Instance.RefreshInventory();
        
        // FIXED: Ép đồng bộ và làm mới lại giao diện trang bị (Equip Slots) để cập nhật ô ngọc trống ngay lập tức
        if (RuneEquipUI.Instance != null) RuneEquipUI.Instance.RefreshEquipUI();
        
        // Làm mới tổng chỉ số Lobby
        if (LobbyStatManager.Instance != null) LobbyStatManager.Instance.RecalculateStats();

        ClosePanel();
    }

    private int GetRefundGemByRarity(RuneRarity rarity)
    {
        switch (rarity)
        {
            case RuneRarity.Common: return 50;
            case RuneRarity.Rare: return 120;
            case RuneRarity.Epic: return 300;
            case RuneRarity.Legendary: return 800;
        }
        return 0;
    }

    private void UpdateRuneImageVisual(RuneData runeData)
    {
        for (int i = 0; i < runeImages.Count; i++)
        {
            if (runeImages[i] != null) runeImages[i].SetActive(false);
        }
        if (ultimateRuneImage != null) ultimateRuneImage.SetActive(false);

        if (IsUltimateRune(runeData))
        {
            if (ultimateRuneImage != null) ultimateRuneImage.SetActive(true);
            return;
        }

        int targetIndex = ((int)runeData.runeRarity * 3) + (int)runeData.runeColor;
        if (targetIndex >= 0 && targetIndex < runeImages.Count && runeImages[targetIndex] != null)
        {
            runeImages[targetIndex].SetActive(true);
        }
    }

    #region Helpers
    private Color GetRarityColor(RuneRarity rarity)
    {
        switch (rarity)
        {
            case RuneRarity.Common: return Color.white;
            case RuneRarity.Rare: return new Color(0.2f, 0.6f, 1f);
            case RuneRarity.Epic: return new Color(0.7f, 0.2f, 1f);
            case RuneRarity.Legendary: return new Color(1f, 0.5f, 0f);
        }
        return Color.white;
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

    private string GetFullStatName(RuneStatType statType)
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

    private bool IsUltimateRune(RuneData rune)
    {
        if (rune == null) return false;
        for (int i = 0; i < rune.affixes.Count; i++)
        {
            if (rune.affixes[i].statType == RuneStatType.AllStats) return true;
        }
        return false;
    }
    #endregion

    public RuneData GetRuneDataHelper()
    {
        return currentData;
    }
}