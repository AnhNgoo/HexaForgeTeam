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

    [Header("Affix Row Container")]
    [SerializeField] private Transform affixRowsContainer;
    [SerializeField] private Button affixRowButtonPrefab;

    [Header("Action Buttons")]
    [SerializeField] private Button rerollActionButton;
    [SerializeField] private Button closePanelButton;

    [Header("Status Texts & Cost UI")]
    [SerializeField] private TMP_Text statusNoticeText;
    [SerializeField] private CostDisplayUI costDisplayUI;

    [Header("Item Config")]
    [SerializeField] private string rerollItemID = "REROLL_SCROLL_01";
    [SerializeField] private string rerollItemName = "Reroll Scroll";

    [Header("Cost Settings")]
    [SerializeField] private int randomRerollShardCost = 150; 
    [SerializeField] private int targetRerollShardCost = 300; 

    [Header("Target Reroll Settings")]
    [SerializeField] private Toggle useTargetRerollToggle;
    [SerializeField] private TMP_Dropdown statTargetDropdown;

    private RuneData targetRuneData;
    private int selectedAffixIndex = -1;
    private bool isAnimating = false;

    // Danh sách lưu vết các StatType thực tế khớp với thứ tự các Option trong Dropdown
    private List<RuneStatType> availableDropdownStats = new List<RuneStatType>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (rerollPanelRoot != null) rerollPanelRoot.SetActive(false);

        if (rerollActionButton != null) rerollActionButton.onClick.AddListener(OnRerollActionButtonPressed);
        if (closePanelButton != null) closePanelButton.onClick.AddListener(ClosePanel);

        if (useTargetRerollToggle != null) useTargetRerollToggle.onValueChanged.AddListener((x) => UpdateCostVisual());
        if (statTargetDropdown != null) statTargetDropdown.onValueChanged.AddListener((x) => UpdateCostVisual());
    }

    public void OpenPanel(RuneData rune)
    {
        if (rune == null) return;
        if (isAnimating) return;

        targetRuneData = rune;
        selectedAffixIndex = -1;

        if (useTargetRerollToggle != null)
        {
            useTargetRerollToggle.isOn = false;
        }

        if (rerollPanelRoot != null) rerollPanelRoot.SetActive(true);

        ClearContainer(cardPreviewParent);
        if (cardPrefabSample != null)
        {
            RuneCardUI previewCard = Instantiate(cardPrefabSample, cardPreviewParent);
            previewCard.Setup(rune, false); 

            RectTransform rect = previewCard.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = Vector2.zero;
                rect.localPosition = Vector3.zero;
            }
            previewCard.transform.localScale = Vector3.one;

            if (previewCard.GetComponent<CanvasGroup>() != null) 
                previewCard.GetComponent<CanvasGroup>().blocksRaycasts = false;
        }

        if (statusNoticeText != null) statusNoticeText.SetTextSafe("Select an Affix line from below to reroll.");

        PopulateDropdownStats(); // THÊM MỚI: Gọi lọc Dropdown ngay khi mở viên ngọc
        RefreshAffixRows();
        UpdateCostVisual();
    }

    public void ClosePanel()
    {
        if (isAnimating) return;

        if (rerollPanelRoot != null) rerollPanelRoot.SetActive(false);
        targetRuneData = null;
        selectedAffixIndex = -1;

        if (RuneInventoryUI.Instance != null) RuneInventoryUI.Instance.RefreshInventory();
    }

    private void RefreshAffixRows()
    {
        ClearContainer(affixRowsContainer);
        if (targetRuneData == null || affixRowButtonPrefab == null) return;

        for (int i = 0; i < targetRuneData.affixes.Count; i++)
        {
            int index = i;
            RuneAffixData affix = targetRuneData.affixes[i];

            Button rowBtn = Instantiate(affixRowButtonPrefab, affixRowsContainer);
            TMP_Text btnText = rowBtn.GetComponentInChildren<TMP_Text>();

            string isPercent = IsPercentStat(affix.statType) ? "%" : "";
            if (btnText != null) btnText.SetTextSafe($"{GetStatName(affix.statType)}: +{affix.value}{isPercent}");

            Image rowImg = rowBtn.GetComponent<Image>();
            if (rowImg != null) rowImg.color = (selectedAffixIndex == index) ? Color.yellow : Color.white;

            rowBtn.onClick.AddListener(() =>
            {
                if (isAnimating) return;
                selectedAffixIndex = index;
                PopulateDropdownStats(); // Tải lại danh sách Dropdown lọc bỏ các dòng thuộc tính đã tồn tại
                RefreshAffixRows();
                UpdateCostVisual();
            });
        }
    }

    private void UpdateCostVisual()
    {
        bool isTargetMode = useTargetRerollToggle != null && useTargetRerollToggle.isOn;
        List<CostData> costs = new List<CostData>();

        if (selectedAffixIndex != -1)
        {
            if (isTargetMode)
            {
                costs.Add(new CostData(rerollItemID, 1));
                costs.Add(new CostData("RUNE_SHARD", targetRerollShardCost));
            }
            else
            {
                costs.Add(new CostData("RUNE_SHARD", randomRerollShardCost));
            }

            if (rerollActionButton != null) rerollActionButton.interactable = true;
        }
        else
        {
            if (rerollActionButton != null) rerollActionButton.interactable = false;
        }

        if (costDisplayUI != null)
        {
            costDisplayUI.SetupCost(costs);
        }

        if (statTargetDropdown != null) statTargetDropdown.gameObject.SetActive(isTargetMode);
    }

    private void OnRerollActionButtonPressed()
    {
        if (targetRuneData == null || selectedAffixIndex == -1 || isAnimating) return;

        bool isTargetMode = useTargetRerollToggle != null && useTargetRerollToggle.isOn;

        if (isTargetMode)
        {
            if (InventoryItemManager.Instance == null || InventoryItemManager.Instance.GetItemQuantity(rerollItemID) < 1)
            {
                if (LobbyNotifyManager.Instance != null)
                {
                    LobbyNotifyManager.Instance.ShowNotify($"Not enough {rerollItemName} to reroll affixes!", Color.red);
                }
                return;
            }

            if (RuneShardManager.Instance == null || RuneShardManager.Instance.GetCurrentShards() < targetRerollShardCost)
            {
                if (LobbyNotifyManager.Instance != null)
                {
                    LobbyNotifyManager.Instance.ShowNotify("Not enough Rune Shards!", Color.red);
                }
                return;
            }

            if (!InventoryItemManager.Instance.SpendItem(rerollItemID, 1))
            {
                return;
            }

            if (!RuneShardManager.Instance.SpendShards(targetRerollShardCost))
            {
                return;
            }
        }
        else
        {
            if (RuneShardManager.Instance == null || !RuneShardManager.Instance.SpendShards(randomRerollShardCost))
            {
                if (LobbyNotifyManager.Instance != null)
                {
                    LobbyNotifyManager.Instance.ShowNotify("Not enough Rune Shards to random roll!", Color.red);
                }
                return;
            }
        }

        RuneStatType finalTargetStat = RuneStatType.HP;
        if (isTargetMode && statTargetDropdown != null && availableDropdownStats.Count > 0)
        {
            int selectedIndex = Mathf.Clamp(statTargetDropdown.value, 0, availableDropdownStats.Count - 1);
            finalTargetStat = availableDropdownStats[selectedIndex];
        }

        StartCoroutine(RerollGachaRoutine(isTargetMode, finalTargetStat));
    }

    private System.Collections.IEnumerator RerollGachaRoutine(bool isTargetMode, RuneStatType targetStat)
    {
        isAnimating = true;
        if (rerollActionButton != null) rerollActionButton.interactable = false;
        if (closePanelButton != null) closePanelButton.interactable = false;

        Transform selectedRow = affixRowsContainer.GetChild(selectedAffixIndex);
        TMP_Text btnText = selectedRow.GetComponentInChildren<TMP_Text>();

        float duration = 1.8f;
        float elapsed = 0f;
        float delayTick = 0.06f;

        while (elapsed < duration)
        {
            elapsed += delayTick;
            RuneStatType randomStat = GetRandomStatPool();
            if (btnText != null)
            {
                string ModePrefix = isTargetMode ? "LOCKING" : "ROLLING";
                btnText.SetTextSafe($"<color=#FFD700>{ModePrefix} → </color>{GetStatName(randomStat)}");
            }
            yield return new WaitForSeconds(delayTick);
        }

        RuneAffixData activeAffix = targetRuneData.affixes[selectedAffixIndex];
        
        if (isTargetMode)
        {
            activeAffix.statType = targetStat;
        }
        else
        {
            activeAffix.statType = GetRandomStatPool();
        }

        activeAffix.value = GenerateNewValueByRarity(targetRuneData.runeRarity, activeAffix.statType);

        if (RuneInventoryManager.Instance != null)
        {
            RuneInventoryManager.Instance.SaveRunes();
        }

        ClearContainer(cardPreviewParent);
        if (cardPrefabSample != null)
        {
            RuneCardUI previewCard = Instantiate(cardPrefabSample, cardPreviewParent);
            previewCard.Setup(targetRuneData, false);

            RectTransform rect = previewCard.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = Vector2.zero;
                rect.localPosition = Vector3.zero;
            }
            previewCard.transform.localScale = Vector3.one;
        }

        if (statusNoticeText != null) statusNoticeText.SetTextSafe("<color=#00FFCC>Affix successfully transmuted!</color>");
        if (LobbyNotifyManager.Instance != null) LobbyNotifyManager.Instance.ShowNotify("Reroll Complete!", Color.green);

        if (LobbyStatManager.Instance != null) LobbyStatManager.Instance.RecalculateStats();

        isAnimating = false;
        if (closePanelButton != null) closePanelButton.interactable = true;
        PopulateDropdownStats();
        RefreshAffixRows();
        UpdateCostVisual();
    }

    private RuneStatType GetRandomStatPool()
    {
        // Lấy danh sách các dòng chưa có trên viên ngọc
        List<RuneStatType> validPool = new List<RuneStatType>();
        for (int i = 0; i < 14; i++)
        {
            RuneStatType type = (RuneStatType)i;
            bool isAlreadyOwned = false;

            if (targetRuneData != null)
            {
                for (int j = 0; j < targetRuneData.affixes.Count; j++)
                {
                    if (j == selectedAffixIndex) continue; // Bỏ qua chính dòng đang được reroll
                    if (targetRuneData.affixes[j].statType == type)
                    {
                        isAlreadyOwned = true;
                        break;
                    }
                }
            }

            if (!isAlreadyOwned)
            {
                validPool.Add(type);
            }
        }

        if (validPool.Count > 0)
        {
            return validPool[Random.Range(0, validPool.Count)];
        }

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

    /// <summary>
    /// Nạp danh sách thuộc tính vào Dropdown và TỰ ĐỘNG LỌC BỎ các thuộc tính viên ngọc ĐÃ SỞ HỮU ở các dòng khác
    /// </summary>
    private void PopulateDropdownStats()
    {
        if (statTargetDropdown == null) return;

        statTargetDropdown.options.Clear();
        availableDropdownStats.Clear();

        for (int i = 0; i < 14; i++)
        {
            RuneStatType candidateStat = (RuneStatType)i;
            bool isAlreadyOwnedOnRune = false;

            if (targetRuneData != null)
            {
                for (int j = 0; j < targetRuneData.affixes.Count; j++)
                {
                    // Kiểm tra: Chỉ cần thuộc tính này ĐÃ TỒN TẠI ở bất kỳ dòng nào trên ngọc -> Loại bỏ ngay!
                    if (targetRuneData.affixes[j].statType == candidateStat)
                    {
                        isAlreadyOwnedOnRune = true;
                        break;
                    }
                }
            }

            if (!isAlreadyOwnedOnRune)
            {
                availableDropdownStats.Add(candidateStat);
                statTargetDropdown.options.Add(new TMP_Dropdown.OptionData(GetStatName(candidateStat)));
            }
        }

        statTargetDropdown.value = 0;
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