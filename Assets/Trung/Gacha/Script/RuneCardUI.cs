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

    [SerializeField] private TMP_Text rarityText;

    [SerializeField] private TMP_Text colorText;

    [SerializeField] private TMP_Text affixText;

    [Header("Equip UI")]
[SerializeField] private Image equippedImage;

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

    [Header("Animation")]
    [SerializeField] private float stampDuration = 0.38f;

    [SerializeField] private float cardShakeDuration = 0.16f;

    [SerializeField] private float cardShakeStrength = 6f;

    [SerializeField] private float rarityTextStartScale = 4.5f;

    private static readonly Color EpicColor =
        new Color(0.7f, 0.3f, 1f);

    private RuneData currentRuneData;

    private bool isOpened;

    private void Awake()
    {
        transform.rotation =
            Quaternion.identity;

        rarityText.transform.localScale =
            Vector3.one;

        rarityText.transform.rotation =
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

        rarityText.text =
            runeData.runeRarity.ToString();

        SetupCardImage(
            runeData.runeRarity);

        SetupRuneShape(
            runeData.runeColor,
            runeData.runeRarity);

        SetupColorText(
            runeData.runeColor);

        SetupRarityColor(
            runeData.runeRarity);

        SetupAffixText(
            runeData);

        SetupTooltip(
            runeData);

        UpdateEquipUI();

        if (!playAnimation)
        {
            return;
        }

        StopAllCoroutines();

        StartCoroutine(
            PlayStampAnimation());
    }

    public void OnPointerClick(
        PointerEventData eventData)
    {
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

        tooltipText.text = "";

        tooltipText.text +=
            $"<b>{runeData.runeRarity}</b>\n";

        tooltipText.text +=
            $"{GetColorName(runeData.runeColor)} Rune\n";

        tooltipText.text +=
            "────────────\n";

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

        if (equippedImage != null)
{
    equippedImage.gameObject.SetActive(
        currentRuneData.isEquipped);
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

    private void SetupRarityColor(
        RuneRarity runeRarity)
    {
        switch (runeRarity)
        {
            case RuneRarity.Common:

                rarityText.color =
                    Color.white;

                break;

            case RuneRarity.Rare:

                rarityText.color =
                    Color.cyan;

                break;

            case RuneRarity.Epic:

                rarityText.color =
                    EpicColor;

                break;

            case RuneRarity.Legendary:

                rarityText.color =
                    Color.yellow;

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

            case RuneStatType.MP:
                return "MP";

            case RuneStatType.Stamina:
                return "STA";

            case RuneStatType.ATK:
                return "ATK";

            case RuneStatType.MATK:
                return "MATK";

            case RuneStatType.DEF:
                return "DEF";

            case RuneStatType.AttackSpeed:
                return "ASPD";

            case RuneStatType.CooldownReduction:
                return "CDR";

            case RuneStatType.CritChance:
                return "CRIT";

            case RuneStatType.CritDamage:
                return "CRIT DMG";

            case RuneStatType.ArmorPenetration:
                return "ARM PEN";

            case RuneStatType.MoveSpeed:
                return "MOVE SPD";

            case RuneStatType.StaminaRegen:
                return "STA REGEN";
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

            case RuneStatType.MP:
                return "MP";

            case RuneStatType.Stamina:
                return "Stamina";

            case RuneStatType.ATK:
                return "Attack";

            case RuneStatType.MATK:
                return "Magic Attack";

            case RuneStatType.DEF:
                return "Defense";

            case RuneStatType.AttackSpeed:
                return "Attack Speed";

            case RuneStatType.CooldownReduction:
                return "Cooldown Reduction";

            case RuneStatType.CritChance:
                return "Critical Chance";

            case RuneStatType.CritDamage:
                return "Critical Damage";

            case RuneStatType.ArmorPenetration:
                return "Armor Penetration";

            case RuneStatType.MoveSpeed:
                return "Move Speed";

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
            case RuneStatType.AttackSpeed:
            case RuneStatType.CooldownReduction:
            case RuneStatType.CritChance:
            case RuneStatType.CritDamage:
            case RuneStatType.ArmorPenetration:
            case RuneStatType.MoveSpeed:
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

    #region Animation

    private IEnumerator PlayStampAnimation()
    {
        float time = 0f;

        Vector3 startScale =
            Vector3.one *
            rarityTextStartScale;

        Vector3 impactScale =
            Vector3.one * 0.72f;

        Vector3 finalScale =
            Vector3.one;

        float startRotation =
            Random.Range(-15f, 15f);

        rarityText.transform.localScale =
            startScale;

        rarityText.transform.rotation =
            Quaternion.Euler(
                0f,
                0f,
                startRotation);

        while (time < stampDuration)
        {
            time += Time.deltaTime;

            float t =
                time / stampDuration;

            if (t < 0.65f)
            {
                float lerpT =
                    t / 0.65f;

                rarityText.transform.localScale =
                    Vector3.Lerp(
                        startScale,
                        impactScale,
                        lerpT);
            }
            else
            {
                float lerpT =
                    (t - 0.65f) / 0.35f;

                rarityText.transform.localScale =
                    Vector3.Lerp(
                        impactScale,
                        finalScale,
                        lerpT);
            }

            float rotation =
                Mathf.Lerp(
                    startRotation,
                    0f,
                    t);

            rarityText.transform.rotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    rotation);

            yield return null;
        }

        rarityText.transform.localScale =
            Vector3.one;

        rarityText.transform.rotation =
            Quaternion.identity;

        yield return StartCoroutine(
            PlayCardShake());
    }

    private IEnumerator PlayCardShake()
    {
        float time = 0f;

        Quaternion originalRotation =
            transform.rotation;

        while (time < cardShakeDuration)
        {
            time += Time.deltaTime;

            float t =
                time / cardShakeDuration;

            float shake =
                Mathf.Sin(t * 45f) *
                (1f - t) *
                cardShakeStrength;

            transform.rotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    shake);

            yield return null;
        }

        transform.rotation =
            originalRotation;
    }

    #endregion
}