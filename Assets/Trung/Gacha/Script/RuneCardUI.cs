using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RuneCardUI :
    MonoBehaviour,
    IPointerClickHandler
{
    [Header("UI")]
    [SerializeField] private Image cardImage;

    [SerializeField] private Image runeShapeImage;


    [SerializeField] private TMP_Text colorText;

    [SerializeField] private TMP_Text affixText;
    [SerializeField] private TMP_Text runeNameText;
    [SerializeField] private TMP_Text runeLoreText;

    [Header("Equip UI")]

[SerializeField] private TMP_Text slotText;
[SerializeField] private Image slotFrameImage;

    [Header("Tooltip")]
    [SerializeField] private GameObject tooltipPanel;

    [SerializeField] private TMP_Text tooltipText;

    [Header("Action Panel")]
    [SerializeField] private GameObject actionPanel;

    [Header("Buttons")]
    [SerializeField] private Button useButton;

    [SerializeField] private TMP_Text useButtonText;

    [SerializeField] private Button deleteButton;
[SerializeField] private GameObject backUI;

private bool isRevealed;
private bool isAnimating;
private bool canRevealAnimation;


    [Header("Card Sprite")]
    [SerializeField] private Sprite commonSprite;

    [SerializeField] private Sprite rareSprite;

    [SerializeField] private Sprite epicSprite;

    [SerializeField] private Sprite legendarySprite;

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

    private RuneData currentRuneData;

    private bool isOpened;
    private bool isSelected;

[Header("Select")]
[SerializeField] private GameObject selectedHighlight;


    private void Awake()
    {
        transform.rotation =
            Quaternion.identity;


        ClosePanels();

        if (useButton != null)
        {
            useButton.onClick.AddListener(
                OnUseButton);
        }

        if (deleteButton != null)
        {
            deleteButton.onClick.AddListener(
                OnDeleteButton);
        }
    }

    private void Update()
    {
        if (!isOpened)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (!IsPointerOverThisCard())
            {
                ClosePanels();
            }
        }
    }

    public void Setup(
        RuneData runeData,
        bool playAnimation = true)
    {
        currentRuneData = runeData;
        isSelected = false;

if (selectedHighlight != null)
{
    selectedHighlight.SetActive(false);
}

SetupCardImage(
    runeData.runeRarity);

SetupRuneShape(
    runeData.runeColor,
    runeData.runeRarity);

SetupColorText(
    runeData.runeColor);

SetupRuneName(
    runeData);

SetupRuneLore(
    runeData);

SetupAffixText(
    runeData);

SetupTooltip(
    runeData);

UpdateEquipUI();
canRevealAnimation =
    playAnimation;

isAnimating = false;

if (canRevealAnimation)
{
    HideFrontUI();

    if (backUI != null)
    {
        backUI.SetActive(true);
    }

    isRevealed = false;
}
else
{
    ShowFrontUI();

    if (backUI != null)
    {
        backUI.SetActive(false);
    }

    isRevealed = true;
}
    }

    public void OnPointerClick(
    PointerEventData eventData)
{
    if (isAnimating)
{
    return;
}

if (canRevealAnimation &&
    !isRevealed)
{
    StartCoroutine(
        RevealAnimation());

    return;
}
    if (InventoryUI.Instance != null &&
        InventoryUI.Instance.IsDeleteMode())
    {
        isSelected = !isSelected;

        if (selectedHighlight != null)
        {
            selectedHighlight.SetActive(
                isSelected);
                
        }

        return;
    }

    isOpened = !isOpened;

    if (tooltipPanel != null)
    {
        tooltipPanel.SetActive(
            isOpened);
    }

    if (actionPanel != null)
    {
        actionPanel.SetActive(
            isOpened);
    }
}

    private void ClosePanels()
    {
        isOpened = false;

        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }

        if (actionPanel != null)
        {
            actionPanel.SetActive(false);
        }
    }

    private bool IsPointerOverThisCard()
    {
        return RectTransformUtility
            .RectangleContainsScreenPoint(
                transform as RectTransform,
                Input.mousePosition,
                null);
    }

    private RuneData GetInventoryRuneData()
    {
        if (RuneInventory.Instance == null)
        {
            return null;
        }

        for (int i = 0;
            i < RuneInventory.Instance.runes.Count;
            i++)
        {
            if (RuneInventory.Instance
                .runes[i]
                .runeID ==
                currentRuneData.runeID)
            {
                return RuneInventory.Instance
                    .runes[i];
            }
        }

        return null;
    }

    #region Tooltip

    private void SetupTooltip(
        RuneData runeData)
    {
        if (tooltipText == null)
        {
            return;
        }


        for (int i = 0;
            i < runeData.affixes.Count;
            i++)
        {
            RuneAffixData affix =
                runeData.affixes[i];

            bool isPercent =
                IsPercentStat(
                    affix.statType);

            if (isPercent)
            {
                tooltipText.text +=
                    $"• +{affix.value:F1}% ";
            }
            else
            {
                tooltipText.text +=
                    $"• +{affix.value:F0} ";
            }

            tooltipText.text +=
                $"{GetFullStatName(affix.statType)}\n";
        }
    }

    #endregion

    #region Button

    private void OnUseButton()
    {
        if (RuneInventory.Instance == null)
        {
            return;
        }

        RuneData inventoryRune =
            GetInventoryRuneData();

        if (inventoryRune == null)
        {
            return;
        }

        currentRuneData =
            inventoryRune;

        if (!inventoryRune.isEquipped)
        {
            bool equipped =
                RuneInventory.Instance
                .EquipRune(
                    inventoryRune);

            if (equipped)
            {
                Debug.Log(
                    "Equipped Rune");
            }
            else
            {
                Debug.Log(
                    "All Slots Full");
            }
        }
        else
        {
            RuneInventory.Instance
                .UnequipRune(
                    inventoryRune);

            Debug.Log(
                "Unequipped Rune");
        }

        currentRuneData =
            inventoryRune;

        Setup(
            currentRuneData,
            false);

        UpdateEquipUI();

        ClosePanels();

        if (InventoryUI.Instance != null)
        {
            InventoryUI.Instance
                .RefreshInventory();
        }
    }

    private void OnDeleteButton()
{
    int refundGem =
        GetRefundGemByRarity(
            currentRuneData.runeRarity);

    GemManager.Instance
        .AddGem(refundGem);

    if (RuneInventory.Instance != null)
    {
        RuneInventory.Instance
            .RemoveRune(
                currentRuneData.runeID);
    }

    Destroy(gameObject);
}

    #endregion

    #region Card Frame

    private void SetupCardImage(
        RuneRarity runeRarity)
    {
        switch (runeRarity)
        {
            case RuneRarity.Common:

                cardImage.sprite =
                    commonSprite;

                break;

            case RuneRarity.Rare:

                cardImage.sprite =
                    rareSprite;

                break;

            case RuneRarity.Epic:

                cardImage.sprite =
                    epicSprite;

                break;

            case RuneRarity.Legendary:

                cardImage.sprite =
                    legendarySprite;

                break;
        }
    }

    #endregion

    #region Rune Shape

    private void SetupRuneShape(
        RuneColor runeColor,
        RuneRarity runeRarity)
    {
        switch (runeColor)
        {
            case RuneColor.Red:

                SetupRedShape(
                    runeRarity);

                break;

            case RuneColor.Green:

                SetupGreenShape(
                    runeRarity);

                break;

            case RuneColor.Blue:

                SetupBlueShape(
                    runeRarity);

                break;
        }
    }

    private void SetupRedShape(
        RuneRarity runeRarity)
    {
        switch (runeRarity)
        {
            case RuneRarity.Common:

                runeShapeImage.sprite =
                    redCommonSprite;

                break;

            case RuneRarity.Rare:

                runeShapeImage.sprite =
                    redRareSprite;

                break;

            case RuneRarity.Epic:

                runeShapeImage.sprite =
                    redEpicSprite;

                break;

            case RuneRarity.Legendary:

                runeShapeImage.sprite =
                    redLegendarySprite;

                break;
        }
    }

    private void SetupGreenShape(
        RuneRarity runeRarity)
    {
        switch (runeRarity)
        {
            case RuneRarity.Common:

                runeShapeImage.sprite =
                    greenCommonSprite;

                break;

            case RuneRarity.Rare:

                runeShapeImage.sprite =
                    greenRareSprite;

                break;

            case RuneRarity.Epic:

                runeShapeImage.sprite =
                    greenEpicSprite;

                break;

            case RuneRarity.Legendary:

                runeShapeImage.sprite =
                    greenLegendarySprite;

                break;
        }
    }

    private void SetupBlueShape(
        RuneRarity runeRarity)
    {
        switch (runeRarity)
        {
            case RuneRarity.Common:

                runeShapeImage.sprite =
                    blueCommonSprite;

                break;

            case RuneRarity.Rare:

                runeShapeImage.sprite =
                    blueRareSprite;

                break;

            case RuneRarity.Epic:

                runeShapeImage.sprite =
                    blueEpicSprite;

                break;

            case RuneRarity.Legendary:

                runeShapeImage.sprite =
                    blueLegendarySprite;

                break;
        }
    }

    #endregion

    #region Equip UI

    private void UpdateEquipUI()
    {
        RuneData inventoryRune =
            GetInventoryRuneData();

        if (inventoryRune != null)
        {
            currentRuneData =
                inventoryRune;
        }


        if (slotText != null)
        {
            slotText.gameObject.SetActive(
                currentRuneData.isEquipped);

            if (currentRuneData.isEquipped)
            {
                slotText.text =
                    $"{currentRuneData.equippedSlotIndex + 1}";
            }
        }
        if (slotFrameImage != null)
{
    slotFrameImage.gameObject.SetActive(
        currentRuneData.isEquipped);
}

        if (useButtonText != null)
        {
            if (currentRuneData.isEquipped)
            {
                useButtonText.text =
                    "Unequip";
            }
            else
            {
                useButtonText.text =
                    "Use";
            }
        }
    }

    #endregion

    #region Text

    private void SetupColorText(
        RuneColor runeColor)
    {
        switch (runeColor)
        {
            case RuneColor.Red:

                colorText.text = "Red";

                colorText.color = Color.red;

                break;

            case RuneColor.Green:

                colorText.text = "Green";

                colorText.color = Color.green;

                break;

            case RuneColor.Blue:

                colorText.text = "Blue";

                colorText.color = Color.cyan;

                break;
        }
    }



    private void SetupAffixText(
        RuneData runeData)
    {
        if (affixText == null)
        {
            return;
        }

        affixText.text = "";

        for (int i = 0;
            i < runeData.affixes.Count;
            i++)
        {
            RuneAffixData affix =
                runeData.affixes[i];

            affixText.text +=
                GetShortStatName(
                    affix.statType);

            if (i <
                runeData.affixes.Count - 1)
            {
                affixText.text += "\n";
            }
        }
    }

   private string GetShortStatName(
    RuneStatType statType)
{
    switch (statType)
    {
        case RuneStatType.HP:
            return "HP";

        case RuneStatType.HPPercent:
            return "HP%";

        case RuneStatType.MP:
            return "MP";

        case RuneStatType.MPPercent:
            return "MP%";

        case RuneStatType.Stamina:
            return "STA";

        case RuneStatType.StaminaPercent:
            return "STA%";

        case RuneStatType.ATK:
            return "ATK";

        case RuneStatType.ATKPercent:
            return "ATK%";

        case RuneStatType.MATK:
            return "MATK";

        case RuneStatType.MATKPercent:
            return "MATK%";

        case RuneStatType.DEF:
            return "DEF";

        case RuneStatType.DEFPercent:
            return "DEF%";

        case RuneStatType.AttackSpeed:
            return "ASPD";

        case RuneStatType.CritChance:
            return "CRIT";

        case RuneStatType.CritDamage:
            return "CRIT DMG";

        case RuneStatType.ArmorPenetration:
            return "ARM PEN";

        case RuneStatType.StaminaRegen:
            return "STA REG";

    }

    return "UNKNOWN";
}

    private string GetFullStatName(
    RuneStatType statType)
{
    switch (statType)
    {
        case RuneStatType.HP:
            return "HP";

        case RuneStatType.HPPercent:
            return "HP";

        case RuneStatType.MP:
            return "MP";

        case RuneStatType.MPPercent:
            return "MP";

        case RuneStatType.Stamina:
            return "Stamina";

        case RuneStatType.StaminaPercent:
            return "Stamina";

        case RuneStatType.ATK:
            return "Attack";

        case RuneStatType.ATKPercent:
            return "Attack";

        case RuneStatType.MATK:
            return "Magic Attack";

        case RuneStatType.MATKPercent:
            return "Magic Attack";

        case RuneStatType.DEF:
            return "Defense";

        case RuneStatType.DEFPercent:
            return "Defense";

        case RuneStatType.AttackSpeed:
            return "Attack Speed";

        case RuneStatType.CritChance:
            return "Critical Chance";

        case RuneStatType.CritDamage:
            return "Critical Damage";

        case RuneStatType.ArmorPenetration:
            return "Armor Penetration";

        case RuneStatType.StaminaRegen:
            return "Stamina Regeneration";

    }

    return "Unknown";
}
    private bool IsPercentStat(
    RuneStatType statType)
{
    switch (statType)
    {
        case RuneStatType.HPPercent:
        case RuneStatType.MPPercent:
        case RuneStatType.StaminaPercent:

        case RuneStatType.ATKPercent:
        case RuneStatType.MATKPercent:
        case RuneStatType.DEFPercent:

        case RuneStatType.AttackSpeed:
        case RuneStatType.CritChance:
        case RuneStatType.CritDamage:
        case RuneStatType.ArmorPenetration:
        case RuneStatType.StaminaRegen:


            return true;
    }

    return false;
}
    private string GetColorName(
        RuneColor runeColor)
    {
        switch (runeColor)
        {
            case RuneColor.Red:
                return "Red";

            case RuneColor.Green:
                return "Green";

            case RuneColor.Blue:
                return "Blue";
        }

        return "Unknown";
    }

    #endregion

       public bool IsSelected()
{
    return isSelected;
}

public RuneData GetRuneData()
{
    return currentRuneData;
}
private int GetRefundGemByRarity(
    RuneRarity rarity)
{
    switch (rarity)
    {
        case RuneRarity.Common:
            return 50;

        case RuneRarity.Rare:
            return 120;

        case RuneRarity.Epic:
            return 300;

        case RuneRarity.Legendary:
            return 800;
    }

    return 0;
}
public bool IsLegendary()
{
    if (currentRuneData == null)
    {
        return false;
    }

    return
        currentRuneData.runeRarity ==
        RuneRarity.Legendary;
}

public bool IsRevealed()
{
    return isRevealed;
}

public void ForceReveal()
{
    if (isAnimating)
    {
        return;
    }

    if (isRevealed)
    {
        return;
    }

    StartCoroutine(
        RevealAnimation());
}
private void SetupRuneName(
    RuneData runeData)
{
    if (runeNameText == null)
    {
        return;
    }

    runeNameText.text =
        runeData.runeName;
}
private void SetupRuneLore(
    RuneData runeData)
{
    if (runeLoreText == null)
    {
        return;
    }

    Debug.Log(
        "Lore = " +
        runeData.runeLore);

    runeLoreText.text =
        $"\"{runeData.runeLore}\"";
}
public void SetSelected(
    bool value)
{
    isSelected = value;

    if (selectedHighlight != null)
    {
        selectedHighlight.SetActive(
            isSelected);
            if (InventoryUI.Instance != null)
{
    InventoryUI.Instance
        .UpdateSelectModeText();
}
    }
}
private void HideFrontUI()
{
    if (runeShapeImage != null)
    {
        runeShapeImage.gameObject.SetActive(false);
    }

    if (colorText != null)
    {
        colorText.gameObject.SetActive(false);
    }

    if (affixText != null)
    {
        affixText.gameObject.SetActive(false);
    }

    if (runeNameText != null)
    {
        runeNameText.gameObject.SetActive(false);
    }

    if (runeLoreText != null)
    {
        runeLoreText.gameObject.SetActive(false);
    }

    if (slotText != null)
    {
        slotText.gameObject.SetActive(false);
    }

    if (slotFrameImage != null)
    {
        slotFrameImage.gameObject.SetActive(false);
    }
}

private void ShowFrontUI()
{
    if (runeShapeImage != null)
    {
        runeShapeImage.gameObject.SetActive(true);
    }

    if (colorText != null)
    {
        colorText.gameObject.SetActive(true);
    }

    if (affixText != null)
    {
        affixText.gameObject.SetActive(true);
    }

    if (runeNameText != null)
    {
        runeNameText.gameObject.SetActive(true);
    }

    if (runeLoreText != null)
    {
        runeLoreText.gameObject.SetActive(true);
    }

    UpdateEquipUI();
}
private IEnumerator RevealAnimation()
{
    isAnimating = true;

    RectTransform rect =
        transform as RectTransform;

    Vector3 originalScale =
        rect.localScale;

    Vector3 originalPos =
        rect.localPosition;

    float flipTime = 0.35f;

    float timer = 0f;

    bool revealed = false;

    while (timer < flipTime)
    {
        timer += Time.deltaTime;

        float t =
            timer / flipTime;

        float xScale;

        if (t < 0.5f)
        {
            xScale =
                Mathf.Lerp(
                    1f,
                    0f,
                    t * 2f);
        }
        else
        {
            if (!revealed)
            {
                revealed = true;

                if (backUI != null)
                {
                    backUI.SetActive(false);
                }

                ShowFrontUI();
            }

            xScale =
                Mathf.Lerp(
                    0f,
                    1f,
                    (t - 0.5f) * 2f);
        }

        rect.localScale =
            new Vector3(
                originalScale.x * xScale,
                originalScale.y,
                originalScale.z);

        yield return null;
    }

    rect.localScale =
        originalScale;

    float slamTime = 0.08f;

    timer = 0f;

    while (timer < slamTime)
    {
        timer += Time.deltaTime;

        float t =
            timer / slamTime;

        rect.localScale =
            Vector3.Lerp(
                originalScale * 1.18f,
                originalScale * 0.82f,
                t);

        yield return null;
    }

    timer = 0f;

    while (timer < 0.1f)
    {
        timer += Time.deltaTime;

        float shakeX =
            Random.Range(-4f, 4f);

        float shakeY =
            Random.Range(-4f, 4f);

        rect.localPosition =
            originalPos +
            new Vector3(
                shakeX,
                shakeY,
                0f);

        yield return null;
    }

    rect.localPosition =
        originalPos;

    rect.localScale =
        originalScale;

    isRevealed = true;
    if (GachaManager.Instance != null)
{
    GachaManager.Instance
        .NotifyCardRevealed();
}

    isAnimating = false;
}
}