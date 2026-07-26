using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class ShopRuneSelectionPopupUI : MonoBehaviour
{
    public static ShopRuneSelectionPopupUI Instance;

    [Header("Panel Root & Overlay")]
    [SerializeField] private GameObject popupRoot;
    [SerializeField] private RectTransform popupContainer;
    [SerializeField] private CanvasGroup bgOverlayCanvasGroup;

    [Header("Rune Visual Preview (Prefab Thẻ Ngọc)")]
    [SerializeField] private Transform runePreviewParent;
    [SerializeField] private RuneCardUI cardPrefabSample;

    [Header("Color Options")]
    [SerializeField] private Toggle toggleRed;
    [SerializeField] private Toggle toggleGreen;
    [SerializeField] private Toggle toggleBlue;

    [Header("Rarity Options")]
    [SerializeField] private Toggle toggleCommon;
    [SerializeField] private Toggle toggleRare;
    [SerializeField] private Toggle toggleEpic;
    [SerializeField] private Toggle toggleLegendary;

    [Header("Price Configuration Per Rarity")]
    [SerializeField] private int commonPrice = 200;
    [SerializeField] private int rarePrice = 500;
    [SerializeField] private int epicPrice = 1200;
    [SerializeField] private int legendaryPrice = 3000;

    [Header("Total Cost UI & Action Button")]
    [SerializeField] private CostDisplayUI costDisplayUI;
    [SerializeField] private Button btnConfirmBuy;
    [SerializeField] private TMP_Text btnConfirmBuyText;
    [SerializeField] private Button btnClose;

    private RuneColor selectedColor = RuneColor.Red;
    private RuneRarity selectedRarity = RuneRarity.Common;
    private RuneCardUI previewCardInstance;
    private RuneData generatedRuneData;
    
    private bool isPurchased = false;
    private bool isAnimating = false;
    private bool isIgnoreCallback = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;

        if (popupRoot != null) popupRoot.SetActive(false);
    }

    private void Start()
    {
        if (toggleRed != null) toggleRed.onValueChanged.AddListener((isOn) => OnToggleColorChanged(RuneColor.Red, isOn));
        if (toggleGreen != null) toggleGreen.onValueChanged.AddListener((isOn) => OnToggleColorChanged(RuneColor.Green, isOn));
        if (toggleBlue != null) toggleBlue.onValueChanged.AddListener((isOn) => OnToggleColorChanged(RuneColor.Blue, isOn));

        if (toggleCommon != null) toggleCommon.onValueChanged.AddListener((isOn) => OnToggleRarityChanged(RuneRarity.Common, isOn));
        if (toggleRare != null) toggleRare.onValueChanged.AddListener((isOn) => OnToggleRarityChanged(RuneRarity.Rare, isOn));
        if (toggleEpic != null) toggleEpic.onValueChanged.AddListener((isOn) => OnToggleRarityChanged(RuneRarity.Epic, isOn));
        if (toggleLegendary != null) toggleLegendary.onValueChanged.AddListener((isOn) => OnToggleRarityChanged(RuneRarity.Legendary, isOn));

        if (btnConfirmBuy != null) btnConfirmBuy.onClick.AddListener(OnConfirmBuyClicked);
        if (btnClose != null) btnClose.onClick.AddListener(HidePopup);
    }

    public void OpenPopup()
    {
        if (popupRoot == null || isAnimating) return;

        isPurchased = false;
        SetTogglesInteractable(true);

        isIgnoreCallback = true;
        selectedColor = RuneColor.Red;
        selectedRarity = RuneRarity.Common;

        if (toggleRed != null) toggleRed.isOn = true;
        if (toggleGreen != null) toggleGreen.isOn = false;
        if (toggleBlue != null) toggleBlue.isOn = false;

        if (toggleCommon != null) toggleCommon.isOn = true;
        if (toggleRare != null) toggleRare.isOn = false;
        if (toggleEpic != null) toggleEpic.isOn = false;
        if (toggleLegendary != null) toggleLegendary.isOn = false;
        isIgnoreCallback = false;

        if (btnConfirmBuyText != null) btnConfirmBuyText.SetTextSafe("BUY");

        popupRoot.SetActive(true);
        RefreshPopupUI();

        isAnimating = true;

        if (bgOverlayCanvasGroup != null)
        {
            bgOverlayCanvasGroup.alpha = 0f;
            bgOverlayCanvasGroup.DOFade(1f, 0.25f).SetUpdate(true);
        }

        if (popupContainer != null)
        {
            popupContainer.transform.localScale = Vector3.one * 0.7f;
            popupContainer.transform.DOScale(Vector3.one, 0.3f)
                .SetEase(Ease.OutBack)
                .SetUpdate(true)
                .OnComplete(() => isAnimating = false);
        }
        else
        {
            isAnimating = false;
        }
    }

    public void HidePopup()
    {
        if (popupRoot == null || !popupRoot.activeSelf || isAnimating) return;

        isAnimating = true;

        if (bgOverlayCanvasGroup != null)
        {
            bgOverlayCanvasGroup.DOFade(0f, 0.2f).SetUpdate(true);
        }

        if (popupContainer != null)
        {
            popupContainer.transform.DOScale(Vector3.zero, 0.2f)
                .SetEase(Ease.InBack)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    popupRoot.SetActive(false);
                    ClearPreviewCard();
                    isAnimating = false;
                });
        }
        else
        {
            popupRoot.SetActive(false);
            ClearPreviewCard();
            isAnimating = false;
        }
    }

    private void OnToggleColorChanged(RuneColor color, bool isOn)
    {
        if (isIgnoreCallback || isPurchased) return;

        if (isOn)
        {
            selectedColor = color;
            RefreshPopupUI();
        }
    }

    private void OnToggleRarityChanged(RuneRarity rarity, bool isOn)
    {
        if (isIgnoreCallback || isPurchased) return;

        if (isOn)
        {
            selectedRarity = rarity;
            RefreshPopupUI();
        }
    }

    private int GetCurrentPrice()
    {
        switch (selectedRarity)
        {
            case RuneRarity.Common: return commonPrice;
            case RuneRarity.Rare: return rarePrice;
            case RuneRarity.Epic: return epicPrice;
            case RuneRarity.Legendary: return legendaryPrice;
        }
        return commonPrice;
    }

    private void RefreshPopupUI()
    {
        if (isPurchased) return;

        int price = GetCurrentPrice();

        if (costDisplayUI != null)
        {
            costDisplayUI.gameObject.SetActive(true);
            costDisplayUI.SetupCost(new List<CostData> { new CostData("GEM", price) });
        }

        if (btnConfirmBuy != null)
        {
            bool canAfford = GemManager.Instance != null && GemManager.Instance.GetCurrentGem() >= price;
            btnConfirmBuy.interactable = canAfford;
        }

        UpdateBlankPreviewCard();
    }

    private void UpdateBlankPreviewCard()
    {
        if (cardPrefabSample == null || runePreviewParent == null) return;

        RuneData blankRune = new RuneData(selectedColor, selectedRarity);
        AssignRuneLore(blankRune);
        blankRune.affixes.Clear();

        if (previewCardInstance == null)
        {
            previewCardInstance = Instantiate(cardPrefabSample, runePreviewParent);
            
            Collider2D col = previewCardInstance.GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            RectTransform rect = previewCardInstance.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchoredPosition = Vector2.zero;
                rect.localPosition = Vector3.zero;
            }
        }

        previewCardInstance.Setup(blankRune, false);
        previewCardInstance.gameObject.SetActive(true);
        previewCardInstance.transform.localScale = Vector3.one * 0.85f;
    }

    private void ClearPreviewCard()
    {
        generatedRuneData = null;

        if (previewCardInstance != null)
        {
            Destroy(previewCardInstance.gameObject);
            previewCardInstance = null;
        }

        if (runePreviewParent != null)
        {
            for (int i = runePreviewParent.childCount - 1; i >= 0; i--)
            {
                Destroy(runePreviewParent.GetChild(i).gameObject);
            }
        }
    }

    private void OnConfirmBuyClicked()
    {
        if (isAnimating) return;

        if (isPurchased)
        {
            HidePopup();
            return;
        }

        int price = GetCurrentPrice();

        if (GemManager.Instance == null || GemManager.Instance.GetCurrentGem() < price)
        {
            if (LobbyNotifyManager.Instance != null)
            {
                LobbyNotifyManager.Instance.ShowNotify("Not enough Gems!", Color.red);
            }
            return;
        }

        if (RuneInventoryManager.Instance != null && RuneInventoryManager.Instance.runes.Count >= 100)
        {
            if (LobbyNotifyManager.Instance != null)
            {
                LobbyNotifyManager.Instance.ShowNotify("Rune vault is full! Please dismantle some runes.", Color.red);
            }
            return;
        }

        if (GemManager.Instance.SpendGem(price))
        {
            generatedRuneData = GenerateCustomRune(selectedColor, selectedRarity);

            if (RuneInventoryManager.Instance != null)
            {
                RuneInventoryManager.Instance.AddRune(generatedRuneData);
            }

            if (LobbyNotifyManager.Instance != null)
            {
                LobbyNotifyManager.Instance.ShowNotify($"Forged Custom Rune: {generatedRuneData.runeName}!", Color.green);
            }

            isPurchased = true;
            SetTogglesInteractable(false);

            if (costDisplayUI != null) costDisplayUI.gameObject.SetActive(false);
            if (btnConfirmBuyText != null) btnConfirmBuyText.SetTextSafe("CONFIRM");

            PlayRuneRevealAnimation(generatedRuneData);

            if (LobbyShopUI.Instance != null)
            {
                LobbyShopUI.Instance.RefreshShopUI();
            }

            if (RuneInventoryUI.Instance != null && RuneInventoryUI.Instance.gameObject.activeInHierarchy)
            {
                RuneInventoryUI.Instance.RefreshInventory();
            }
        }
    }

    private void PlayRuneRevealAnimation(RuneData runeData)
    {
        if (previewCardInstance == null) return;

        isAnimating = true;
        RectTransform rect = previewCardInstance.transform as RectTransform;

        Sequence revealSeq = DOTween.Sequence();

        // Phase 1: Lắc dữ dội bùng nổ trong vài giây kèm phóng to nhẹ
        revealSeq.Append(rect.DOShakePosition(1.2f, strength: 35f, vibrato: 40, randomness: 90, snapping: false, fadeOut: false));
        revealSeq.Join(rect.DOShakeRotation(1.2f, strength: new Vector3(0, 0, 25f), vibrato: 40));
        revealSeq.Join(rect.DOScale(Vector3.one * 1.1f, 1.2f).SetEase(Ease.InQuad));

        // Phase 2: Thu hẹp trục X để lật thẻ bài
        revealSeq.Append(rect.DOScaleX(0f, 0.15f).SetEase(Ease.InQuad));
        revealSeq.AppendCallback(() =>
        {
            previewCardInstance.Setup(runeData, false);
        });

        // Phase 3: Bùng nổ hiện thẻ bài đã có đầy đủ dòng chỉ số (Affix)
        revealSeq.Append(rect.DOScaleX(0.85f, 0.15f).SetEase(Ease.OutQuad));
        revealSeq.Append(rect.DOScale(Vector3.one * 0.85f, 0.1f));
        revealSeq.Append(rect.DOPunchScale(new Vector3(0.35f, 0.35f, 0.35f), 0.35f, 12, 1f));

        revealSeq.OnComplete(() =>
        {
            isAnimating = false;
        });
    }

    private void SetTogglesInteractable(bool interactable)
    {
        if (toggleRed != null) toggleRed.interactable = interactable;
        if (toggleGreen != null) toggleGreen.interactable = interactable;
        if (toggleBlue != null) toggleBlue.interactable = interactable;

        if (toggleCommon != null) toggleCommon.interactable = interactable;
        if (toggleRare != null) toggleRare.interactable = interactable;
        if (toggleEpic != null) toggleEpic.interactable = interactable;
        if (toggleLegendary != null) toggleLegendary.interactable = interactable;
    }

    private RuneData GenerateCustomRune(RuneColor color, RuneRarity rarity)
    {
        RuneData rune = new RuneData(color, rarity);
        AssignRuneLore(rune);
        GenerateAffixes(rune);
        return rune;
    }

    private void GenerateAffixes(RuneData rune)
    {
        int affixCount = GetAffixCount(rune.runeRarity);
        List<RuneStatType> usedStats = new List<RuneStatType>();

        for (int i = 0; i < affixCount; i++)
        {
            RuneStatType statType = GetRandomStat(usedStats);
            usedStats.Add(statType);
            float value = GetRandomValue(statType, rune.runeRarity);
            rune.affixes.Add(new RuneAffixData(statType, value));
        }
    }

    private int GetAffixCount(RuneRarity rarity)
    {
        switch (rarity)
        {
            case RuneRarity.Common: return 1;
            case RuneRarity.Rare: return 2;
            case RuneRarity.Epic: return 3;
            case RuneRarity.Legendary: return 4;
        }
        return 1;
    }

    private RuneStatType GetRandomStat(List<RuneStatType> usedStats)
    {
        List<RuneStatType> pool = new List<RuneStatType>()
        {
            RuneStatType.HP, RuneStatType.HPPercent, RuneStatType.MP, RuneStatType.MPPercent,
            RuneStatType.Stamina, RuneStatType.StaminaPercent, RuneStatType.ATK, RuneStatType.ATKPercent,
            RuneStatType.DEF, RuneStatType.DEFPercent, RuneStatType.CritChance, RuneStatType.CritDamage,
            RuneStatType.ArmorPenetration, RuneStatType.StaminaRegen
        };

        for (int i = pool.Count - 1; i >= 0; i--)
        {
            if (usedStats.Contains(pool[i])) pool.RemoveAt(i);
        }
        return pool[Random.Range(0, pool.Count)];
    }

    private float GetRandomValue(RuneStatType statType, RuneRarity rarity)
    {
        switch (statType)
        {
            case RuneStatType.HP: return GetValueByRarity(rarity, 80f, 180f, 180f, 350f, 350f, 650f, 650f, 1200f);
            case RuneStatType.MP: return GetValueByRarity(rarity, 25f, 60f, 60f, 120f, 120f, 220f, 220f, 400f);
            case RuneStatType.Stamina: return GetValueByRarity(rarity, 15f, 40f, 40f, 80f, 80f, 140f, 140f, 250f);
            case RuneStatType.ATK: return GetValueByRarity(rarity, 3f, 8f, 8f, 18f, 18f, 35f, 35f, 60f);
            case RuneStatType.DEF: return GetValueByRarity(rarity, 2f, 6f, 6f, 14f, 14f, 28f, 28f, 50f);
            case RuneStatType.HPPercent: return GetValueByRarity(rarity, 2f, 4f, 4f, 7f, 7f, 12f, 12f, 20f);
            case RuneStatType.MPPercent: return GetValueByRarity(rarity, 2f, 4f, 4f, 7f, 7f, 12f, 12f, 18f);
            case RuneStatType.StaminaPercent: return GetValueByRarity(rarity, 3f, 5f, 5f, 9f, 9f, 15f, 15f, 25f);
            case RuneStatType.ATKPercent: return GetValueByRarity(rarity, 2f, 4f, 4f, 7f, 7f, 12f, 12f, 18f);
            case RuneStatType.DEFPercent: return GetValueByRarity(rarity, 2f, 4f, 4f, 7f, 7f, 12f, 12f, 18f);
            case RuneStatType.CritChance: return GetValueByRarity(rarity, 1f, 3f, 3f, 6f, 6f, 10f, 10f, 18f);
            case RuneStatType.CritDamage: return GetValueByRarity(rarity, 4f, 8f, 8f, 15f, 15f, 25f, 25f, 40f);
            case RuneStatType.ArmorPenetration: return GetValueByRarity(rarity, 2f, 5f, 5f, 9f, 9f, 15f, 15f, 25f);
            case RuneStatType.StaminaRegen: return GetValueByRarity(rarity, 3f, 6f, 6f, 10f, 10f, 18f, 18f, 30f);
        }
        return 1f;
    }

    private float GetValueByRarity(RuneRarity rarity, float cMin, float cMax, float rMin, float rMax, float eMin, float eMax, float lMin, float lMax)
    {
        switch (rarity)
        {
            case RuneRarity.Common: return Random.Range(cMin, cMax);
            case RuneRarity.Rare: return Random.Range(rMin, rMax);
            case RuneRarity.Epic: return Random.Range(eMin, eMax);
            case RuneRarity.Legendary: return Random.Range(lMin, lMax);
        }
        return 1f;
    }

    private void AssignRuneLore(RuneData rune)
    {
        switch (rune.runeColor)
        {
            case RuneColor.Red:
                switch (rune.runeRarity)
                {
                    case RuneRarity.Common: rune.runeName = "Ashfang"; rune.runeLore = "Its heat faded long ago, yet the scar remains."; break;
                    case RuneRarity.Rare: rune.runeName = "Blood Oath"; rune.runeLore = "The knight survived the battle. His comrades did not."; break;
                    case RuneRarity.Epic: rune.runeName = "Heart of Ruin"; rune.runeLore = "Every beat echoed like a war drum beneath the earth."; break;
                    case RuneRarity.Legendary: rune.runeName = "Crimson Crown"; rune.runeLore = "Kings burned kingdoms to wear it for a single night."; break;
                }
                break;
            case RuneColor.Green:
                switch (rune.runeRarity)
                {
                    case RuneRarity.Common: rune.runeName = "Wiltroot"; rune.runeLore = "It grew where no light should ever reach."; break;
                    case RuneRarity.Rare: rune.runeName = "Verdant Pulse"; rune.runeLore = "The forest whispered back when spoken to."; break;
                    case RuneRarity.Epic: rune.runeName = "Hollow Bloom"; rune.runeLore = "Flowers fed on the dead beneath the ruins."; break;
                    case RuneRarity.Legendary: rune.runeName = "Worldsap Core"; rune.runeLore = "Its roots once held an entire civilization together."; break;
                }
                break;
            case RuneColor.Blue:
                switch (rune.runeRarity)
                {
                    case RuneRarity.Common: rune.runeName = "Frost Vein"; rune.runeLore = "Cold enough to silence fear itself."; break;
                    case RuneRarity.Rare: rune.runeName = "Moon Shard"; rune.runeLore = "Fragments of a sky long forgotten."; break;
                    case RuneRarity.Epic: rune.runeName = "Deep Current"; rune.runeLore = "Something ancient moved beneath the tide."; break;
                    case RuneRarity.Legendary: rune.runeName = "Eye of Eternity"; rune.runeLore = "It watched the end before time understood death."; break;
                }
                break;
        }
    }
}