using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class RuneCardUI : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler, IPointerEnterHandler, IPointerExitHandler
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

    [Header("NEW: Select Mode Highlights")]
    [SerializeField] private GameObject selectedHighlight; 
    [SerializeField] private Toggle selectToggle;          
    private bool isSelected = false;                        
    private bool isDeleteMode = false;                      

    [Header("Card Theme & Back Settings")]
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

    private Canvas parentCanvas;
    private CanvasGroup canvasGroup;
    private GameObject dragProxy;
    [Header("Audio SFX Clips")]
    [SerializeField] private AudioClip hoverSFX;
    [SerializeField] private AudioClip clickSFX;
    private static AudioSource sharedCardAudioSource;

    private void Awake()
    {
        transform.rotation = Quaternion.identity;

        parentCanvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        if (selectToggle != null)
        {
            selectToggle.onValueChanged.AddListener((isOn) => {
                isSelected = isOn;
                if (selectedHighlight != null) selectedHighlight.SetActive(isSelected);
            });
        }
    }

    public void Setup(RuneData runeData, bool playAnimation = true)
    {
        currentRuneData = runeData;

        SetupCardImage(runeData.runeRarity);
        SetupRuneShape(runeData.runeColor, runeData.runeRarity);
        
        try
        {
            SetupColorText(runeData.runeColor);
            SetupRuneName(runeData);
            SetupRuneLore(runeData);
            SetupAffixText(runeData);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[RuneCardUI Protect] Error: {e.Message}");
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

        if (RuneInventoryUI.Instance != null && RuneInventoryUI.Instance.GetSelectedRuneData() == currentRuneData)
        {
            SetSelected(true);
        }
        else
        {
            SetSelected(false);
        }
    }

    #region Drag and Drop Handlers

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (currentRuneData == null || !isRevealed || isAnimating || isDeleteMode) return;

        if (RuneInventoryUI.Instance != null)
        {
            RuneInventoryUI.Instance.OnRuneClicked(this, currentRuneData);
        }

        dragProxy = new GameObject("Rune_DragProxy");
        if (parentCanvas != null) dragProxy.transform.SetParent(parentCanvas.transform, false);
        dragProxy.transform.SetAsLastSibling();

        Image proxyImg = dragProxy.AddComponent<Image>();
        proxyImg.sprite = runeShapeImage != null && runeShapeImage.sprite != null ? runeShapeImage.sprite : cardImage.sprite;
        
        proxyImg.raycastTarget = false; 

        RectTransform proxyRect = dragProxy.GetComponent<RectTransform>();
        proxyRect.sizeDelta = new Vector2(80, 80);

        canvasGroup.alpha = 0.4f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragProxy != null)
        {
            dragProxy.transform.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1.0f;
        canvasGroup.blocksRaycasts = true;

        if (dragProxy != null)
        {
            Destroy(dragProxy);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentRuneData == null || !isRevealed || isAnimating || isDeleteMode) return;

        PlayCardSFX(hoverSFX, 0.5f);

        bool isInsideInventory = (RuneInventoryUI.Instance != null && RuneInventoryUI.Instance.gameObject.activeInHierarchy) ||
                                 (RuneDetailInfoPanel.Instance != null && RuneDetailInfoPanel.Instance.IsPanelActive());

        if (isInsideInventory)
        {
            if (RuneInventoryUI.Instance != null)
            {
                RuneInventoryUI.Instance.OnRuneHovered(currentRuneData);
            }
            else if (RuneDetailInfoPanel.Instance != null)
            {
                RuneDetailInfoPanel.Instance.DisplayRuneInfo(currentRuneData);
            }
            return;
        }

        if (UITooltipPanel.Instance != null)
        {
            string title = $"<color={GetRarityHexColor(currentRuneData.runeRarity)}>{currentRuneData.runeName.ToUpper()}</color>";
            string details = $"<b>Rarity:</b> {currentRuneData.runeRarity}\n<b>Element:</b> {currentRuneData.runeColor}\n\n";

            for (int i = 0; i < currentRuneData.affixes.Count; i++)
            {
                var affix = currentRuneData.affixes[i];
                string sign = affix.value >= 0 ? "+" : "";
                details += $"- {affix.statType}: <color=#00FFCC>{sign}{affix.value:F1}</color>\n";
            }

            if (!string.IsNullOrEmpty(currentRuneData.runeLore))
            {
                details += $"\n<i>\"{currentRuneData.runeLore}\"</i>";
            }

            UITooltipPanel.Instance.ShowTooltip(title, details, runeShapeImage != null ? runeShapeImage.sprite : null);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (UITooltipPanel.Instance != null)
        {
            UITooltipPanel.Instance.HideTooltip();
        }
    }

    private string GetRarityHexColor(RuneRarity rarity)
    {
        switch (rarity)
        {
            case RuneRarity.Common: return "#FFFFFF";
            case RuneRarity.Rare: return "#3399FF";
            case RuneRarity.Epic: return "#B266FF";
            case RuneRarity.Legendary: return "#FF9900";
        }
        return "#FFFFFF";
    }

    #endregion

    #region Scroll Handler

    public void OnScroll(PointerEventData eventData)
    {
        ScrollRect parentScroll = GetComponentInParent<ScrollRect>();
        if (parentScroll != null)
        {
            parentScroll.OnScroll(eventData);
        }
    }

    #endregion

    public void UpdateSelectModeVisual(bool isDeleteModeActive = false)
    {
        isDeleteMode = isDeleteModeActive;
        if (selectToggle != null)
        {
            selectToggle.gameObject.SetActive(isDeleteMode); 
        }
        if (!isDeleteMode)
        {
            bool isLocked = RuneInventoryUI.Instance != null && RuneInventoryUI.Instance.GetSelectedRuneData() == currentRuneData;
            SetSelected(isLocked); 
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isAnimating) return;

        PlayCardSFX(clickSFX, 0.8f);

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

        if (RuneInventoryUI.Instance != null)
        {
            RuneInventoryUI.Instance.OnRuneClicked(this, currentRuneData);
        }
        else if (RuneDetailInfoPanel.Instance != null)
        {
            RuneDetailInfoPanel.Instance.DisplayRuneInfo(currentRuneData);
        }
    }

    public void SetSelectedDirect(bool value)
    {
        isSelected = value;
        if (selectedHighlight != null) selectedHighlight.SetActive(isSelected);
        if (selectToggle != null)
        {
            selectToggle.SetIsOnWithoutNotify(isSelected);
        }
    }

    public void SetSelected(bool value)
    {
        SetSelectedDirect(value);
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
        bool isEquippedByOtherChar = false;
        string ownerCharName = "";
        int slotIndex = -1;

        CharacterType viewingType = (RuneEquipUI.Instance != null) ? RuneEquipUI.Instance.GetViewingCharacter() : CharacterManager.Instance.GetSelectedCharacter();

        if (CharacterManager.Instance != null && currentRuneData != null)
        {
            CharacterType[] allChars = (CharacterType[])System.Enum.GetValues(typeof(CharacterType));
            foreach (CharacterType charType in allChars)
            {
                var build = CharacterManager.Instance.GetCharacterRuneBuild(charType);
                if (build != null && build.equippedRuneIDs != null)
                {
                    for (int i = 0; i < build.equippedRuneIDs.Length; i++)
                    {
                        if (build.equippedRuneIDs[i] == currentRuneData.runeID)
                        {
                            if (charType == viewingType)
                            {
                                isEquippedByCurrentChar = true;
                                slotIndex = i;
                            }
                            else
                            {
                                isEquippedByOtherChar = true;
                                ownerCharName = charType.ToString().ToUpper();
                            }
                            break;
                        }
                    }
                }
            }
        }

        try
        {
            bool isEquippedAnywhere = isEquippedByCurrentChar || isEquippedByOtherChar;

            if (slotText != null)
            {
                slotText.gameObject.SetActive(isEquippedAnywhere);
                if (isEquippedByCurrentChar)
                {
                    slotText.SetTextSafe($"{slotIndex + 1}");
                }
                else if (isEquippedByOtherChar)
                {
                    slotText.SetTextSafe(ownerCharName);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[RuneCardUI Protect] Error font/render: {e.Message}");
        }

        if (slotFrameImage != null)
        {
            slotFrameImage.gameObject.SetActive(isEquippedByCurrentChar || isEquippedByOtherChar);
        }
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

    public RuneData GetRuneData() => currentRuneData;

    public bool IsSelected() => isSelected;

    public bool IsLegendary() => currentRuneData != null && currentRuneData.runeRarity == RuneRarity.Legendary;
    public bool IsRevealed() => isRevealed;

    public void InstantRevealWithoutAnimation()
    {
        transform.DOKill();
        isAnimating = false;
        isRevealed = true;
        canRevealAnimation = false;

        if (backUI != null) backUI.SetActive(false);
        ShowFrontUI();

        if (canvasGroup != null)
        {
            canvasGroup.DOKill();
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }
    }

    public void ForceReveal()
    {
        InstantRevealWithoutAnimation();
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
    private void PlayCardSFX(AudioClip clip, float volume = 0.8f)
    {
        if (clip == null) return;

        if (sharedCardAudioSource == null)
        {
            GameObject audioObj = new GameObject("Card_AudioSource_Shared");
            DontDestroyOnLoad(audioObj);
            sharedCardAudioSource = audioObj.AddComponent<AudioSource>();
            sharedCardAudioSource.playOnAwake = false;
        }

        sharedCardAudioSource.PlayOneShot(clip, volume);
    }
}