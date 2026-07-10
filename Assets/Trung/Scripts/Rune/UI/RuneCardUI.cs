using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class RuneCardUI : MonoBehaviour, IPointerClickHandler
{
    [Header("UI Fields")]
    [SerializeField] private Image cardImage;
    [SerializeField] private Image runeShapeImage;
    [SerializeField] private TMP_Text colorText;
    [SerializeField] private TMP_Text affixText;
    [SerializeField] private TMP_Text runeNameText;
    [SerializeField] private TMP_Text runeLoreText;

    [Header("Equip UI Layout")]
    [SerializeField] private TMP_Text slotText;
    [SerializeField] private Image slotFrameImage;

    [Header("Tooltip Panel Config")]
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TMP_Text tooltipText;

    [Header("Action Context Panel")]
    [SerializeField] private GameObject actionPanel;

    [Header("NEW: Select Mode Highlights")]
    [SerializeField] private GameObject selectedHighlight; 
    [SerializeField] private Toggle selectToggle;          
    private bool isSelected = false;                        
    private bool isDeleteMode = false;                      

    [Header("Action Buttons Click")]
    [SerializeField] private Button useButton;
    [SerializeField] private TMP_Text useButtonText;
    [SerializeField] private Button deleteButton;
    [SerializeField] private Button smallRerollButton;
    [SerializeField] private GameObject backUI;
    [SerializeField] private Sprite originRuneSprite;

    private bool isRevealed;
    private bool isAnimating;
    private bool canRevealAnimation;

    [Header("Card Sprite Theme")]
    [SerializeField] private Sprite commonSprite;
    [SerializeField] private Sprite rareSprite;
    [SerializeField] private Sprite epicSprite;
    [SerializeField] private Sprite legendarySprite;

    [Header("Red Shape Resource")]
    [SerializeField] private Sprite redCommonSprite;
    [SerializeField] private Sprite redRareSprite;
    [SerializeField] private Sprite redEpicSprite;
    [SerializeField] private Sprite redLegendarySprite;

    [Header("Green Shape Resource")]
    [SerializeField] private Sprite greenCommonSprite;
    [SerializeField] private Sprite greenRareSprite;
    [SerializeField] private Sprite greenEpicSprite;
    [SerializeField] private Sprite greenLegendarySprite;

    [Header("Blue Shape Resource")]
    [SerializeField] private Sprite blueCommonSprite;
    [SerializeField] private Sprite blueRareSprite;
    [SerializeField] private Sprite blueEpicSprite;
    [SerializeField] private Sprite blueLegendarySprite;

    private RuneData currentRuneData;
    private bool isOpened;

    private void Awake()
    {
        transform.rotation = Quaternion.identity;
        ClosePanels();

        if (useButton != null) useButton.onClick.AddListener(OnUseButton);
        if (deleteButton != null) deleteButton.onClick.AddListener(OnDeleteButton);
        if (smallRerollButton != null) smallRerollButton.onClick.AddListener(() => {
    if (currentRuneData != null && RuneRerollUI.Instance != null) {
        RuneRerollUI.Instance.OpenPanel(currentRuneData); // ĐỔI THÀNH OpenPanel theo đúng file RuneRerollUI.cs mới
        ClosePanels(); 
    }
});

        if (selectToggle != null)
        {
            selectToggle.onValueChanged.AddListener((isOn) => {
                isSelected = isOn;
                if (selectedHighlight != null) selectedHighlight.SetActive(isSelected);
            });
        }
    }

    private void Update()
    {
        if (!isOpened) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (!IsPointerOverThisCard())
            {
                ClosePanels();
            }
        }
    }

    public void Setup(RuneData runeData, bool playAnimation = true)
    {
        currentRuneData = runeData;

        SetupCardImage(runeData.runeRarity);
        SetupRuneShape(runeData.runeColor, runeData.runeRarity);
        
        // BẢO VỆ CHỐNG LỖI FONT TOÀN DIỆN CHO THẺ BÀI NGỌC
        try
        {
            SetupColorText(runeData.runeColor);
            SetupRuneName(runeData);
            SetupRuneLore(runeData);
            SetupAffixText(runeData);
            SetupTooltip(runeData);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[RuneCardUI Protect] Chặn sập ghim do lỗi ký tự Font 1683: {e.Message}");
        }

        UpdateEquipUI();

        canRevealAnimation = playAnimation;
        isAnimating = false;

        if (canRevealAnimation)
        {
            HideFrontUI();
            if (backUI != null) backUI.SetActive(true);
            isRevealed = false;
        }
        else
        {
            ShowFrontUI();
            if (backUI != null) backUI.SetActive(false);
            isRevealed = true;
        }

        SetSelected(false); 
    }

    public void UpdateSelectModeVisual()
    {
        if (selectToggle != null)
        {
            selectToggle.gameObject.SetActive(isDeleteMode); 
        }
        if (!isDeleteMode)
        {
            SetSelected(false); 
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isAnimating) return;

        if (canRevealAnimation && !isRevealed)
        {
            if (IsLegendary() && GachaManager.Instance != null)
            {
                canRevealAnimation = false;
                GachaManager.Instance.TriggerLegendaryRevealAction(this);
            }
            else
            {
                StartInternalReveal();
            }
            return;
        }
        
        if (isDeleteMode)
        {
            SetSelected(!isSelected);
            return; 
        }

        if (RuneFusionUI.Instance != null && RuneFusionUI.Instance.gameObject.activeInHierarchy)
        {
            RuneFusionUI.Instance.AddRuneToFusion(currentRuneData);
            return; 
        }

        if (RuneDetailInfoPanel.Instance != null)
        {
            RuneDetailInfoPanel.Instance.DisplayRuneInfo(currentRuneData);
        }

        isOpened = !isOpened;
        if (tooltipPanel != null) tooltipPanel.SetActive(isOpened); 
        if (actionPanel != null) actionPanel.SetActive(isOpened);   
    }

    public void SetSelected(bool value)
    {
        isSelected = value;
        if (selectedHighlight != null) selectedHighlight.SetActive(isSelected);
        if (selectToggle != null)
        {
            selectToggle.onValueChanged.RemoveAllListeners(); 
            selectToggle.isOn = isSelected;
            selectToggle.onValueChanged.AddListener((isOn) => {
                isSelected = isOn;
                if (selectedHighlight != null) selectedHighlight.SetActive(isSelected);
            });
        }
    }

    private void ClosePanels()
    {
        isOpened = false;
        if (tooltipPanel != null) tooltipPanel.SetActive(false);
        if (actionPanel != null) actionPanel.SetActive(false);
    }

    private bool IsPointerOverThisCard()
    {
        return RectTransformUtility.RectangleContainsScreenPoint(transform as RectTransform, Input.mousePosition, null);
    }

    private RuneData GetInventoryRuneData()
    {
        if (RuneInventoryManager.Instance == null) return null;
        for (int i = 0; i < RuneInventoryManager.Instance.runes.Count; i++)
        {
            if (RuneInventoryManager.Instance.runes[i].runeID == currentRuneData.runeID) return RuneInventoryManager.Instance.runes[i];
        }
        return null;
    }

    private void SetupTooltip(RuneData runeData)
    {
        if (tooltipText == null) return;
        tooltipText.text = ""; 

        for (int i = 0; i < runeData.affixes.Count; i++)
        {
            RuneAffixData affix = runeData.affixes[i];
            bool isPercent = IsPercentStat(affix.statType);
            if (isPercent) tooltipText.text += $"• +{affix.value:F1}% ";
            else tooltipText.text += $"• +{affix.value:F0} ";

            tooltipText.text += $"{GetFullStatName(affix.statType)}\n";
        }
    }

    private void OnUseButton()
    {
        if (RuneInventoryManager.Instance == null) return;

        RuneData inventoryRune = GetInventoryRuneData();
        if (inventoryRune == null) return;

        currentRuneData = inventoryRune;
        bool isEquippedByCurrentChar = false;
        
        CharacterType currentType = (RuneEquipUI.Instance != null) ? RuneEquipUI.Instance.GetViewingCharacter() : CharacterManager.Instance.GetSelectedCharacter();
        CharacterRuneEquip build = CharacterManager.Instance.GetCharacterRuneBuild(currentType);

        if (build != null)
        {
            for (int i = 0; i < build.equippedRuneIDs.Length; i++)
            {
                if (build.equippedRuneIDs[i] == currentRuneData.runeID) { isEquippedByCurrentChar = true; break; }
            }
        }

        if (!isEquippedByCurrentChar)
        {
            int targetSlotIndex = -1;
            if (build != null && RuneEquipUI.Instance != null)
            {
                if (IsUltimateRune())
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
                        if (string.IsNullOrEmpty(build.equippedRuneIDs[i]))
                        {
                            RuneColor requiredColor = RuneEquipUI.Instance.GetSlotRequiredColor(currentType, i);
                            if (inventoryRune.runeColor == requiredColor) { targetSlotIndex = i; break; }
                        }
                    }
                }
            }

            if (targetSlotIndex == -1)
            {
                Debug.LogWarning($"<color=#FF3333>[TRANG BỊ] Không tìm thấy ô trống phù hợp hệ màu {inventoryRune.runeColor} trên nhân vật này.</color>");
                if (LobbyNotifyManager.Instance != null) LobbyNotifyManager.Instance.ShowNotify("No matching empty slot found for this rune element!", Color.yellow);
                return; 
            }

            bool equipped = RuneInventoryManager.Instance.EquipRune(inventoryRune, currentType);
            if (equipped)
            {
                Debug.Log($"<color=#00FFCC><b>[TRANG BỊ]</b> Đã lắp thành công viên ngọc {inventoryRune.runeName} vào ô {targetSlotIndex + 1}</color>");
                if (LobbyNotifyManager.Instance != null) LobbyNotifyManager.Instance.ShowNotify($"Rune equipped successfully into slot {targetSlotIndex + 1}!", Color.green);
            }
            else
            {
                Debug.Log("<color=#FF3333>[TRANG BỊ] Lắp ngọc thất bại. Toàn bộ các slot đã bị lấp đầy.</color>");
                if (LobbyNotifyManager.Instance != null) LobbyNotifyManager.Instance.ShowNotify("Equip failed. All rune slots are currently full!", Color.red);
            }
        }
        else
        {
            RuneInventoryManager.Instance.UnequipRune(inventoryRune, currentType);
            Debug.Log($"<color=#FFFF66><b>[THÁO NGỌC NHANH]</b> Đã gỡ viên {inventoryRune.runeName} khỏi bảng trang bị.</color>");
            if (LobbyNotifyManager.Instance != null) LobbyNotifyManager.Instance.ShowNotify($"Rune unequipped from character layout.", Color.white);
        }

        currentRuneData = inventoryRune;
        Setup(currentRuneData, false);
        UpdateEquipUI();
        ClosePanels();

        if (InventoryUI.Instance != null) InventoryUI.Instance.RefreshInventory();
    }

    private void OnDeleteButton()
    {
        if (currentRuneData == null) return;

        CharacterType[] allChars = (CharacterType[])System.Enum.GetValues(typeof(CharacterType));
        foreach (CharacterType charType in allChars)
        {
            var build = CharacterManager.Instance.GetCharacterRuneBuild(charType);
            if (build != null)
            {
                for (int slot = 0; slot < build.equippedRuneIDs.Length; slot++)
                {
                    if (build.equippedRuneIDs[slot] == currentRuneData.runeID) build.equippedRuneIDs[slot] = ""; 
                }
            }
        }

        // FIXED CHUẨN CHỈ: Tính toán trực tiếp tiền hoàn trả theo độ hiếm ngọc để dập tắt hoàn toàn lỗi CS0103
        int refundGem = 0;
        switch (currentRuneData.runeRarity)
        {
            case RuneRarity.Common: refundGem = 50; break;
            case RuneRarity.Rare: refundGem = 120; break;
            case RuneRarity.Epic: refundGem = 300; break;
            case RuneRarity.Legendary: refundGem = 800; break;
        }

        GemManager.Instance.AddGem(refundGem);

        if (RuneInventoryManager.Instance != null) RuneInventoryManager.Instance.RemoveRune(currentRuneData.runeID);

        Debug.Log($"<color=#FFFF66><b>[PHÂN TÁCH NGỌC]</b> Đã giải phóng slot và hủy viên ngọc {currentRuneData.runeName} thành công.</color>");
        if (LobbyNotifyManager.Instance != null)
            LobbyNotifyManager.Instance.ShowNotify($"Rune dismantled! Gained +{refundGem} Crystals.", Color.green);

        if (RuneEquipUI.Instance != null) RuneEquipUI.Instance.RefreshEquipUI();
        if (LobbyStatManager.Instance != null) LobbyStatManager.Instance.RecalculateStats();

        Destroy(gameObject);
    }

    private void SetupCardImage(RuneRarity runeRarity)
    {
        switch (runeRarity)
        {
            case RuneRarity.Common: cardImage.sprite = commonSprite; break;
            case RuneRarity.Rare: cardImage.sprite = rareSprite; break;
            case RuneRarity.Epic: cardImage.sprite = epicSprite; break;
            case RuneRarity.Legendary: cardImage.sprite = legendarySprite; break;
        }
    }

    private void SetupRuneShape(RuneColor runeColor, RuneRarity runeRarity)
    {
        if (IsUltimateRune()) { runeShapeImage.sprite = originRuneSprite; return; }
        switch (runeColor)
        {
            case RuneColor.Red: SetupRedShape(runeRarity); break;
            case RuneColor.Green: SetupGreenShape(runeRarity); break;
            case RuneColor.Blue: SetupBlueShape(runeRarity); break;
        }
    }

    private void SetupRedShape(RuneRarity runeRarity)
    {
        switch (runeRarity)
        {
            case RuneRarity.Common: runeShapeImage.sprite = redCommonSprite; break;
            case RuneRarity.Rare: runeShapeImage.sprite = redRareSprite; break;
            case RuneRarity.Epic: runeShapeImage.sprite = redEpicSprite; break;
            case RuneRarity.Legendary: runeShapeImage.sprite = redLegendarySprite; break;
        }
    }

    private void SetupGreenShape(RuneRarity runeRarity)
    {
        switch (runeRarity)
        {
            case RuneRarity.Common: runeShapeImage.sprite = greenCommonSprite; break;
            case RuneRarity.Rare: runeShapeImage.sprite = greenRareSprite; break;
            case RuneRarity.Epic: runeShapeImage.sprite = greenEpicSprite; break;
            case RuneRarity.Legendary: runeShapeImage.sprite = greenLegendarySprite; break;
        }
    }

    private void SetupBlueShape(RuneRarity runeRarity)
    {
        switch (runeRarity)
        {
            case RuneRarity.Common: runeShapeImage.sprite = blueCommonSprite; break;
            case RuneRarity.Rare: runeShapeImage.sprite = blueRareSprite; break;
            case RuneRarity.Epic: runeShapeImage.sprite = blueEpicSprite; break;
            case RuneRarity.Legendary: runeShapeImage.sprite = blueLegendarySprite; break;
        }
    }

    private void UpdateEquipUI()
    {
        RuneData inventoryRune = GetInventoryRuneData();
        if (inventoryRune != null) currentRuneData = inventoryRune;

        bool isEquippedByCurrentChar = false;
        int slotIndex = -1;

        CharacterType currentType = (RuneEquipUI.Instance != null) ? RuneEquipUI.Instance.GetViewingCharacter() : CharacterManager.Instance.GetSelectedCharacter();
        CharacterRuneEquip build = CharacterManager.Instance.GetCharacterRuneBuild(currentType);

        if (build != null && currentRuneData != null)
        {
            for (int i = 0; i < build.equippedRuneIDs.Length; i++)
            {
                if (build.equippedRuneIDs[i] == currentRuneData.runeID) { isEquippedByCurrentChar = true; slotIndex = i; break; }
            }
        }

        // BẢO VỆ CHỐNG LỖI FONT 1683
        try
        {
            if (slotText != null)
            {
                slotText.gameObject.SetActive(isEquippedByCurrentChar);
                if (isEquippedByCurrentChar) slotText.text = $"{slotIndex + 1}";
            }
            if (useButtonText != null)
            {
                useButtonText.text = isEquippedByCurrentChar ? "Unequip" : "Use";
            }
        }
        catch {}

        if (slotFrameImage != null) slotFrameImage.gameObject.SetActive(isEquippedByCurrentChar);
    }

    private void SetupColorText(RuneColor runeColor)
    {
        if (IsUltimateRune()) { colorText.text = "Origin"; colorText.color = new Color(1f, 0.84f, 0f); return; }
        switch (runeColor)
        {
            case RuneColor.Red: colorText.text = "Red"; colorText.color = Color.red; break;
            case RuneColor.Green: colorText.text = "Green"; colorText.color = Color.green; break;
            case RuneColor.Blue: colorText.text = "Blue"; colorText.color = Color.cyan; break;
        }
    }

    private void SetupAffixText(RuneData runeData)
    {
        if (affixText == null) return;
        affixText.text = "";

        for (int i = 0; i < runeData.affixes.Count; i++)
        {
            RuneAffixData affix = runeData.affixes[i];
            affixText.text += GetShortStatName(affix.statType);
            if (i < runeData.affixes.Count - 1) affixText.text += "\n";
        }
    }

    private string GetShortStatName(RuneStatType statType)
    {
        switch (statType)
        {
            case RuneStatType.HP: return "HP";
            case RuneStatType.HPPercent: return "HP%";
            case RuneStatType.MP: return "MP";
            case RuneStatType.MPPercent: return "MP%";
            case RuneStatType.Stamina: return "STA";
            case RuneStatType.StaminaPercent: return "STA%";
            case RuneStatType.ATK: return "ATK";
            case RuneStatType.ATKPercent: return "ATK%";
            case RuneStatType.DEF: return "DEF";
            case RuneStatType.DEFPercent: return "DEF%";
            case RuneStatType.CritChance: return "CRIT";
            case RuneStatType.CritDamage: return "CRIT DMG";
            case RuneStatType.ArmorPenetration: return "ARM PEN";
            case RuneStatType.StaminaRegen: return "STA REG";
            case RuneStatType.AllStats: return "ALL STAT";
        }
        return "UNKNOWN";
    }

    private string GetFullStatName(RuneStatType statType)
    {
        switch (statType)
        {
            case RuneStatType.HP: return "HP";
            case RuneStatType.HPPercent: return "HP";
            case RuneStatType.MP: return "MP";
            case RuneStatType.MPPercent: return "MP";
            case RuneStatType.Stamina: return "Stamina";
            case RuneStatType.StaminaPercent: return "Stamina";
            case RuneStatType.ATK: return "Attack";
            case RuneStatType.ATKPercent: return "Attack";
            case RuneStatType.DEF: return "Defense";
            case RuneStatType.DEFPercent: return "Defense";
            case RuneStatType.CritChance: return "Critical Chance";
            case RuneStatType.CritDamage: return "Critical Damage";
            case RuneStatType.ArmorPenetration: return "Armor Penetration";
            case RuneStatType.StaminaRegen: return "Stamina Regeneration";
            case RuneStatType.AllStats: return "All Stats";
        }
        return "Unknown";
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

    public RuneData GetRuneData() => currentRuneData;

    public bool IsSelected() => isSelected;

    public bool IsLegendary() => currentRuneData != null && currentRuneData.runeRarity == RuneRarity.Legendary;
    public bool IsRevealed() => isRevealed;

    public void ForceReveal()
    {
        if (isAnimating || isRevealed) return;
        StartDOTweenRevealAnimation();
    }

    private void SetupRuneName(RuneData runeData)
    {
        if (runeNameText == null) return;
        runeNameText.text = runeData.runeName;
        if (IsUltimateRune()) runeNameText.color = new Color(1f, 0.84f, 0f);
    }

    private void SetupRuneLore(RuneData runeData)
    {
        if (runeLoreText == null) return;
        runeLoreText.text = $"\"{runeData.runeLore}\""; 
    }

    private void HideFrontUI()
    {
        if (runeShapeImage != null) runeShapeImage.gameObject.SetActive(false);
        if (colorText != null) colorText.gameObject.SetActive(false);
        if (affixText != null) affixText.gameObject.SetActive(false);
        if (runeNameText != null) runeNameText.gameObject.SetActive(false);
        if (runeLoreText != null) runeLoreText.gameObject.SetActive(false);
        if (slotText != null) slotText.gameObject.SetActive(false);
        if (slotFrameImage != null) slotFrameImage.gameObject.SetActive(false);
    }

    private void ShowFrontUI()
    {
        if (runeShapeImage != null) runeShapeImage.gameObject.SetActive(true);
        if (colorText != null) colorText.gameObject.SetActive(true);
        if (affixText != null) affixText.gameObject.SetActive(true);
        if (runeNameText != null) runeNameText.gameObject.SetActive(true);
        if (runeLoreText != null) runeLoreText.gameObject.SetActive(true);
        UpdateEquipUI();
    }

    private void StartDOTweenRevealAnimation()
    {
        isAnimating = true;
        RectTransform rect = transform as RectTransform;
        Vector3 originalScale = rect.localScale;
        Vector3 originalPos = rect.localPosition;

        Sequence flipSequence = DOTween.Sequence();
        flipSequence.Append(rect.DOScaleX(0f, 0.15f).SetEase(Ease.InQuad));
        flipSequence.AppendCallback(() => {
            if (backUI != null) backUI.SetActive(false);
            ShowFrontUI();
        });
        flipSequence.Append(rect.DOScaleX(originalScale.x, 0.15f).SetEase(Ease.OutQuad));
        flipSequence.Append(rect.DOScale(originalScale * 1.18f, 0.04f).SetEase(Ease.OutQuad));
        flipSequence.Append(rect.DOScale(originalScale * 0.82f, 0.04f).SetEase(Ease.InQuad));
        flipSequence.Append(rect.DOShakePosition(0.1f, new Vector3(4f, 4f, 0f), 30));
        flipSequence.OnComplete(() => {
            rect.localScale = originalScale;
            rect.localPosition = originalPos;
            isRevealed = true;
            if (GachaManager.Instance != null) GachaManager.Instance.NotifyCardRevealed();
            isAnimating = false;
        });
    }

    private bool IsUltimateRune()
    {
        if (currentRuneData == null) return false;
        for (int i = 0; i < currentRuneData.affixes.Count; i++)
        {
            if (currentRuneData.affixes[i].statType == RuneStatType.AllStats) return true;
        }
        return false;
    }

    public void StartInternalReveal()
    {
        if (!isRevealed && !isAnimating) StartDOTweenRevealAnimation();
    }
}