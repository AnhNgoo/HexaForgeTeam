using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;
using System.Collections.Generic;

public class RuneEquipUI : MonoBehaviour
{
    [Header("Equip Slots")]
    [SerializeField] private Image slot1Image;
    [SerializeField] private Image slot2Image;
    [SerializeField] private Image slot3Image;
    [SerializeField] private TMP_Text slot1ConditionText;
    [SerializeField] private TMP_Text slot2ConditionText;
    [SerializeField] private TMP_Text slot3ConditionText;

    [Header("Character Buttons (Luôn hiển thị)")]
    [SerializeField] private GameObject kaelButtonObj;
    [SerializeField] private GameObject lyraButtonObj;
    [SerializeField] private GameObject aresButtonObj;
    [SerializeField] private GameObject elaraButtonObj;

    private CharacterType viewingCharacter;

    [Header("Stat UI")]
    [SerializeField] private TMP_Text totalStatText;

    [Header("Empty Sprite")]
    [SerializeField] private Sprite emptySprite;

    [Header("Red Shape")]
    [SerializeField] private Sprite redCommonSprite;
    [SerializeField] private Sprite redRareSprite;
    [SerializeField] private Sprite redEpicSprite;
    [SerializeField] private Sprite redLegendarySprite;

    [Header("Green Shape")]
    [SerializeField] private Sprite greenCommonSprite;
    [SerializeField] private Sprite greenRareSprite;
    [SerializeField] private Sprite greenEpicSprite;
    [SerializeField] private Sprite greenLegendarySprite;

    [Header("Blue Shape")]
    [SerializeField] private Sprite blueCommonSprite;
    [SerializeField] private Sprite blueRareSprite;
    [SerializeField] private Sprite blueEpicSprite;
    [SerializeField] private Sprite blueLegendarySprite;

    [Header("Origin Rune")]
    [SerializeField] private Sprite originRuneSprite;

    public static RuneEquipUI Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        if (CharacterManager.Instance != null)
        {
            viewingCharacter = CharacterManager.Instance.GetSelectedCharacter();
        }
        RefreshEquipUI();
    }

    public void RefreshEquipUI()
    {
        ResetSlots();

        if (RuneInventoryManager.Instance == null)
        {
            return;
        }

        CharacterType currentType = viewingCharacter;
        CharacterRuneEquip build = CharacterManager.Instance.GetCharacterRuneBuild(currentType);
        
        // Cập nhật trạng thái Alpha và tương tác của toàn bộ nút bấm nhân vật mới
        UpdateCharacterButtonsState();

        TMP_Text[] conditionTexts = new TMP_Text[3] { slot1ConditionText, slot2ConditionText, slot3ConditionText };
        for (int i = 0; i < conditionTexts.Length; i++)
        {
            if (conditionTexts[i] == null) continue;
            RuneColor requiredColor = GetSlotRequiredColor(currentType, i);
            conditionTexts[i].text = requiredColor.ToString();
            conditionTexts[i].color = (requiredColor == RuneColor.Red) ? Color.red : (requiredColor == RuneColor.Green) ? Color.green : Color.cyan;
        }

        if (build == null)
        {
            return;
        }

        for (int i = 0; i < build.equippedRuneIDs.Length; i++)
        {
            string targetID = build.equippedRuneIDs[i];
            if (string.IsNullOrEmpty(targetID))
            {
                continue;
            }

            RuneData rune = null;
            for (int k = 0; k < RuneInventoryManager.Instance.runes.Count; k++)
            {
                if (RuneInventoryManager.Instance.runes[k].runeID == targetID)
                {
                    rune = RuneInventoryManager.Instance.runes[k];
                    break;
                }
            }

            if (rune == null)
            {
                continue;
            }

            Image targetSlot = GetSlotImage(i);
            if (targetSlot == null)
            {
                continue;
            }

            targetSlot.sprite = GetRuneSprite(rune);

            if (IsUltimateRune(rune))
            {
                targetSlot.color = new Color(1f, 0.9f, 0.5f);
            }
            else
            {
                targetSlot.color = Color.white;
            }
        }

        RefreshTotalStatText();
    }

    #region Total Stats

    private void RefreshTotalStatText()
    {
        if (totalStatText == null || RuneInventoryManager.Instance == null)
        {
            return;
        }

        Dictionary<RuneStatType, float> totalStats = RuneInventoryManager.Instance.GetStats(viewingCharacter);

        StringBuilder builder = new StringBuilder();

        foreach (var stat in totalStats)
        {
            if (stat.Key == RuneStatType.AllStats)
            {
                builder.AppendLine("<color=#FFD700>✦ ORIGIN POWER ✦\nAll Stats +" + $"{stat.Value:F0}</color>\n");
                continue;
            }
            
            bool isPercent = IsPercentStat(stat.Key);
            float rawValue = stat.Value;
            float cap = RuneInventoryManager.Instance.GetHardCap(stat.Key);
            bool reachedCap = rawValue >= cap;
            string color = reachedCap ? "#FF4C4C" : "#FFD966";
            float displayValue = Mathf.Min(rawValue, cap);

            if (isPercent)
            {
                builder.AppendLine($"<color={color}>{GetStatName(stat.Key)} +{displayValue:F1}% / {cap:F0}%</color>");
            }
            else
            {
                builder.AppendLine($"<color={color}>{GetStatName(stat.Key)} +{displayValue:F0} / {cap:F0}</color>");
            }
        }
        totalStatText.text = builder.ToString();
    }

    #endregion

    #region Button Alpha & Interactable States

    /// <summary>
    /// Đồng bộ trạng thái mờ/đậm và tương tác của toàn bộ nút chọn nhân vật trong hòm đồ
    /// </summary>
    private void UpdateCharacterButtonsState()
    {
        UpdateSingleButtonState(CharacterType.Kael, kaelButtonObj);
        UpdateSingleButtonState(CharacterType.Lyra, lyraButtonObj);
        UpdateSingleButtonState(CharacterType.Ares, aresButtonObj);
        UpdateSingleButtonState(CharacterType.Elara, elaraButtonObj);
    }

    private void UpdateSingleButtonState(CharacterType type, GameObject buttonObj)
    {
        if (buttonObj == null || CharacterManager.Instance == null) return;

        // Ép nút luôn hiển thị trên màn hình chứ không SetActive(false) như trước
        buttonObj.SetActive(true);

        Button btn = buttonObj.GetComponent<Button>();
        Image img = buttonObj.GetComponent<Image>();

        bool isUnlocked = CharacterManager.Instance.IsUnlocked(type);
        bool isSelected = (viewingCharacter == type);

        // Nếu chưa mở khóa nhân vật, khóa tương tác bấm nút để chống Bug xem giao diện trống
        if (btn != null)
        {
            btn.interactable = isUnlocked;
        }

        // Thay đổi tỷ lệ Alpha của ảnh nền Button dựa trên trạng thái thực tế
        if (img != null)
        {
            Color c = img.color;
            if (!isUnlocked)
            {
                c.a = 0.25f; // Nhân vật CHƯA CÓ: Mờ hẳn đi (Alpha 0.25)
            }
            else if (isSelected)
            {
                c.a = 0.5f;  // Nhân vật ĐANG ĐƯỢC CHỌN: Giảm nhẹ độ đậm (Alpha 0.5)
            }
            else
            {
                c.a = 1.0f;  // Nhân vật ĐÃ CÓ nhưng không chọn: Sáng rõ hoàn toàn (Alpha 1.0)
            }
            img.color = c;
        }
    }

    #endregion

    #region Slot

    private void ResetSlots()
    {
        ResetSlot(slot1Image);
        ResetSlot(slot2Image);
        ResetSlot(slot3Image);
    }

    private void ResetSlot(Image slotImage)
    {
        if (slotImage == null) return;

        slotImage.sprite = emptySprite;
        Color color = slotImage.color;
        color.a = 1f;
        slotImage.color = color;
    }

    private Image GetSlotImage(int slotIndex)
    {
        switch (slotIndex)
        {
            case 0: return slot1Image;
            case 1: return slot2Image;
            case 2: return slot3Image;
        }
        return null;
    }

    #endregion

    #region Unequip

    public void UnequipBySlot(int slotIndex)
    {
        if (RuneInventoryManager.Instance == null)
        {
            return;
        }

        CharacterType currentType = viewingCharacter; 
        CharacterRuneEquip build = CharacterManager.Instance.GetCharacterRuneBuild(currentType);

        if (build == null || slotIndex < 0 || slotIndex >= build.equippedRuneIDs.Length)
        {
            return;
        }

        string targetID = build.equippedRuneIDs[slotIndex];
        if (string.IsNullOrEmpty(targetID))
        {
            return;
        }

        RuneData rune = null;
        for (int i = 0; i < RuneInventoryManager.Instance.runes.Count; i++)
        {
            if (RuneInventoryManager.Instance.runes[i].runeID == targetID)
            {
                rune = RuneInventoryManager.Instance.runes[i];
                break;
            }
        }

        if (rune != null)
        {
            RuneInventoryManager.Instance.UnequipRune(rune, currentType);
            RefreshEquipUI();

            if (RuneInventoryUI.Instance != null)
            {
                RuneInventoryUI.Instance.RefreshInventory();
            }
        }
    }

    #endregion

    #region Rune Sprite

    private Sprite GetRuneSprite(RuneData rune)
    {
        if (IsUltimateRune(rune)) return originRuneSprite;

        switch (rune.runeColor)
        {
            case RuneColor.Red: return GetRedSprite(rune.runeRarity);
            case RuneColor.Green: return GetGreenSprite(rune.runeRarity);
            case RuneColor.Blue: return GetBlueSprite(rune.runeRarity);
        }
        return null;
    }

    private Sprite GetRedSprite(RuneRarity rarity)
    {
        switch (rarity)
        {
            case RuneRarity.Common: return redCommonSprite;
            case RuneRarity.Rare: return redRareSprite;
            case RuneRarity.Epic: return redEpicSprite;
            case RuneRarity.Legendary: return redLegendarySprite;
        }
        return null;
    }

    private Sprite GetGreenSprite(RuneRarity rarity)
    {
        switch (rarity)
        {
            case RuneRarity.Common: return greenCommonSprite;
            case RuneRarity.Rare: return greenRareSprite;
            case RuneRarity.Epic: return greenEpicSprite;
            case RuneRarity.Legendary: return greenLegendarySprite;
        }
        return null;
    }

    private Sprite GetBlueSprite(RuneRarity rarity)
    {
        switch (rarity)
        {
            case RuneRarity.Common: return blueCommonSprite;
            case RuneRarity.Rare: return blueRareSprite;
            case RuneRarity.Epic: return blueEpicSprite;
            case RuneRarity.Legendary: return blueLegendarySprite;
        }
        return null;
    }

    #endregion

    #region Stat Helper

    private bool IsPercentStat(RuneStatType statType)
    {
        switch (statType)
        {
            case RuneStatType.HPPercent:
            case RuneStatType.MPPercent:
            case RuneStatType.StaminaPercent:
            case RuneStatType.ATKPercent:
            case RuneStatType.DEFPercent:
            case RuneStatType.CritChance:
            case RuneStatType.CritDamage:
            case RuneStatType.ArmorPenetration:
            case RuneStatType.StaminaRegen:
                return true;
        }
        return false;
    }

    private string GetStatName(RuneStatType statType)
    {
        switch (statType)
        {
            case RuneStatType.HP: return "HP";
            case RuneStatType.HPPercent: return "HP";
            case RuneStatType.MP: return "MP";
            case RuneStatType.MPPercent: return "MP";
            case RuneStatType.Stamina: return "Stamina";
            case RuneStatType.StaminaPercent: return "Stamina";
            case RuneStatType.ATK: return "ATK";
            case RuneStatType.ATKPercent: return "ATK";
            case RuneStatType.DEF: return "DEF";
            case RuneStatType.DEFPercent: return "DEF";
            case RuneStatType.CritChance: return "Crit Chance";
            case RuneStatType.CritDamage: return "Crit Damage";
            case RuneStatType.ArmorPenetration: return "Armor Penetration";
            case RuneStatType.StaminaRegen: return "Stamina Regen";
            case RuneStatType.AllStats: return "All Stats";
        }
        return "Unknown";
    }

    #endregion

    private bool IsUltimateRune(RuneData rune)
    {
        if (rune == null) return false;
        for (int i = 0; i < rune.affixes.Count; i++)
        {
            if (rune.affixes[i].statType == RuneStatType.AllStats) return true;
        }
        return false;
    }

    public RuneColor GetSlotRequiredColor(CharacterType charType, int slotIndex)
    {
        if (charType == CharacterType.Kael) return (slotIndex == 2) ? RuneColor.Green : RuneColor.Red;
        if (charType == CharacterType.Lyra) return (slotIndex == 0) ? RuneColor.Red : RuneColor.Blue;
        if (charType == CharacterType.Ares) return (slotIndex == 2) ? RuneColor.Blue : RuneColor.Green;
        return (slotIndex == 0) ? RuneColor.Red : (slotIndex == 1) ? RuneColor.Green : RuneColor.Blue;
    }

    public void SwitchBuildToKael()
    {
        viewingCharacter = CharacterType.Kael;
        RefreshEquipUI();
        if (RuneInventoryUI.Instance != null) RuneInventoryUI.Instance.RefreshInventory();
        if (LobbyStatManager.Instance != null) LobbyStatManager.Instance.RecalculateStats();
        
        if (RuneDetailInfoPanel.Instance != null && RuneDetailInfoPanel.Instance.gameObject.activeInHierarchy)
        {
            RuneDetailInfoPanel.Instance.DisplayRuneInfo(RuneDetailInfoPanel.Instance.GetRuneDataHelper());
        }
    }

    public void SwitchBuildToLyra()
    {
        viewingCharacter = CharacterType.Lyra;
        RefreshEquipUI();
        if (RuneInventoryUI.Instance != null) RuneInventoryUI.Instance.RefreshInventory();
        if (LobbyStatManager.Instance != null) LobbyStatManager.Instance.RecalculateStats();

        if (RuneDetailInfoPanel.Instance != null && RuneDetailInfoPanel.Instance.gameObject.activeInHierarchy)
        {
            RuneDetailInfoPanel.Instance.DisplayRuneInfo(RuneDetailInfoPanel.Instance.GetRuneDataHelper());
        }
    }

    public void SwitchBuildToAres()
    {
        viewingCharacter = CharacterType.Ares;
        RefreshEquipUI();
        if (RuneInventoryUI.Instance != null) RuneInventoryUI.Instance.RefreshInventory();
        if (LobbyStatManager.Instance != null) LobbyStatManager.Instance.RecalculateStats();

        if (RuneDetailInfoPanel.Instance != null && RuneDetailInfoPanel.Instance.gameObject.activeInHierarchy)
        {
            RuneDetailInfoPanel.Instance.DisplayRuneInfo(RuneDetailInfoPanel.Instance.GetRuneDataHelper());
        }
    }

    public void SwitchBuildToElara()
    {
        viewingCharacter = CharacterType.Elara;
        RefreshEquipUI();
        if (RuneInventoryUI.Instance != null) RuneInventoryUI.Instance.RefreshInventory();
        if (LobbyStatManager.Instance != null) LobbyStatManager.Instance.RecalculateStats();

        if (RuneDetailInfoPanel.Instance != null && RuneDetailInfoPanel.Instance.gameObject.activeInHierarchy)
        {
            RuneDetailInfoPanel.Instance.DisplayRuneInfo(RuneDetailInfoPanel.Instance.GetRuneDataHelper());
        }
    }

    public CharacterType GetViewingCharacter()
    {
        return viewingCharacter;
    }
}