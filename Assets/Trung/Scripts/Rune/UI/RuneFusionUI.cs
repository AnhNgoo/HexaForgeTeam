using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class RuneFusionUI : MonoBehaviour
{
    public static RuneFusionUI Instance;

    [Header("Panel Root")]
    [SerializeField] private GameObject fusionPanelRoot;

    [Header("Fusion Slots (3 Ô chứa GameObject Thẻ Ngọc)")]
    [SerializeField] private Transform[] ingredientSlots = new Transform[3];

    [Header("Center Point (Tâm gộp hiệu ứng)")]
    [SerializeField] private Transform fusionCenterPoint;

    [Header("Action Buttons")]
    [SerializeField] private Button fuseButton;
    [SerializeField] private Button clearAllButton;

    [Header("Status Text")]
    [SerializeField] private TMP_Text chanceText;
    [SerializeField] private TMP_Text resultText;

    [Header("Cost Display Prefab Manager")]
    [SerializeField] private CostDisplayUI costDisplayUI;

    [Header("Prefab Sample Display")]
    [SerializeField] private RuneCardUI cardPrefabSample;

    [Header("Protection Item Config")]
    [SerializeField] private string charmItemID = "FUSION_CHARM_01";
    [SerializeField] private Toggle useCharmToggle;

    private List<RuneData> selectedRunes = new List<RuneData>();
    private List<RuneCardUI> spawnedVisualCards = new List<RuneCardUI>();
    private bool isAnimating = false;

    private RuneCardUI rewardCardInstance = null;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        if (fuseButton != null) fuseButton.onClick.AddListener(OnFuseButtonClicked);
        if (clearAllButton != null) clearAllButton.onClick.AddListener(ClearFusionSlots);
        if (useCharmToggle != null) useCharmToggle.onValueChanged.AddListener((x) => UpdateFusionPanelVisual());

        ResetToggleState();
    }

    private void OnEnable()
    {
        ResetToggleState();
    }

    private void ResetToggleState()
    {
        if (useCharmToggle != null)
        {
            useCharmToggle.isOn = false;
        }
        UpdateFusionPanelVisual();
    }

    public void AddRuneToFusion(RuneData runeData)
    {
        if (isAnimating) return;

        ClearRewardCard();

        if (selectedRunes.Exists(r => r.runeID == runeData.runeID)) return;

        if (selectedRunes.Count >= 3)
        {
            if (resultText != null) resultText.SetTextSafe("<color=#FF4C4C>Ingredient slots are full!</color>");
            if (LobbyNotifyManager.Instance != null) LobbyNotifyManager.Instance.ShowNotify("Material slots are full!", Color.red);
            return;
        }

        if (selectedRunes.Count > 0 && selectedRunes[0].runeRarity != runeData.runeRarity)
        {
            if (resultText != null) resultText.SetTextSafe("<color=#FFFF66>Material rarity must be identical!</color>");
            if (LobbyNotifyManager.Instance != null) LobbyNotifyManager.Instance.ShowNotify("Runes must be of the same rarity!", Color.yellow);
            return;
        }

        if (runeData.runeRarity == RuneRarity.Legendary)
        {
            if (resultText != null) resultText.SetTextSafe("<color=#FFFF66>Legendary runes cannot be fused further!</color>");
            if (LobbyNotifyManager.Instance != null) LobbyNotifyManager.Instance.ShowNotify("Legendary tier has reached maximum level!", Color.yellow);
            return;
        }

        selectedRunes.Add(runeData);
        int currentSlotIndex = selectedRunes.Count - 1;

        if (cardPrefabSample != null && ingredientSlots[currentSlotIndex] != null)
        {
            RuneCardUI visualCard = Instantiate(cardPrefabSample, ingredientSlots[currentSlotIndex]);
            visualCard.Setup(runeData, false);

            if (visualCard.GetComponent<Collider2D>() != null) visualCard.GetComponent<Collider2D>().enabled = false;

            RectTransform rect = visualCard.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchoredPosition = Vector2.zero;
                rect.localPosition = Vector3.zero;
            }

            float targetScale = 0.5f;
            visualCard.transform.localScale = Vector3.zero;
            visualCard.transform.DOScale(new Vector3(targetScale, targetScale, targetScale), 0.25f).SetEase(Ease.OutBack);

            spawnedVisualCards.Add(visualCard);
        }

        UpdateFusionPanelVisual();
    }

    public void ClearFusionSlots()
    {
        if (isAnimating) return;

        selectedRunes.Clear();
        for (int i = spawnedVisualCards.Count - 1; i >= 0; i--)
        {
            if (spawnedVisualCards[i] != null) Destroy(spawnedVisualCards[i].gameObject);
        }
        spawnedVisualCards.Clear();

        ClearRewardCard();

        if (resultText != null) resultText.text = "Select 3 runes of the same rarity to begin fusion...";
        UpdateFusionPanelVisual();
    }

    private void ClearRewardCard()
    {
        if (rewardCardInstance != null)
        {
            Destroy(rewardCardInstance.gameObject);
            rewardCardInstance = null;
        }
    }

    private void UpdateFusionPanelVisual()
    {
        List<CostData> currentCosts = new List<CostData>();

        if (selectedRunes.Count == 0)
        {
            if (chanceText != null) chanceText.text = "Success Rate: --%";
            if (costDisplayUI != null) costDisplayUI.SetupCost(currentCosts);
            if (fuseButton != null) fuseButton.interactable = false;
            return;
        }

        RuneRarity currentRarity = selectedRunes[0].runeRarity;
        bool wantToUseCharm = useCharmToggle != null && useCharmToggle.isOn;
        bool hasCharm = InventoryItemManager.Instance != null && InventoryItemManager.Instance.GetItemQuantity(charmItemID) >= 1;
        bool isCharmActive = wantToUseCharm && hasCharm;

        if (chanceText != null)
        {
            if (isCharmActive)
            {
                chanceText.text = "Success Rate: <color=#00FFCC>100% (Charm Protected)</color>";
            }
            else
            {
                float rate = currentRarity == RuneRarity.Common ? 85f : currentRarity == RuneRarity.Rare ? 60f : 35f;
                chanceText.text = $"Success Rate: <color=green>{rate}%</color>";
            }
        }

        int shardCost = currentRarity == RuneRarity.Common ? 100 : currentRarity == RuneRarity.Rare ? 300 : 800;

        // Thêm Cost Mảnh Cổ Tự
        currentCosts.Add(new CostData("RUNE_SHARD", shardCost));

        // Nếu bật Toggle Bùa Bảo Vệ -> Thêm Cost Bùa
        if (wantToUseCharm)
        {
            currentCosts.Add(new CostData(charmItemID, 1));
        }

        // Truyền danh sách Cost sang Prefab quản lý hiển thị
        if (costDisplayUI != null)
        {
            costDisplayUI.SetupCost(currentCosts);
        }

        if (fuseButton != null)
        {
            fuseButton.interactable = (selectedRunes.Count == 3);
        }
    }

    public void AutoFillIngredients()
    {
        if (isAnimating || RuneInventoryManager.Instance == null) return;

        ClearFusionSlots();

        List<RuneData> allRunes = RuneInventoryManager.Instance.runes;
        List<RuneData> filteredRunes = new List<RuneData>();
        foreach (RuneData r in allRunes)
        {
            if (r == null || r.runeRarity == RuneRarity.Legendary) continue;

            if (RuneFilterPanel.Instance != null)
            {
                if (RuneFilterPanel.Instance.EvaluateRuneFilter(r)) filteredRunes.Add(r);
            }
            else
            {
                filteredRunes.Add(r);
            }
        }

        RuneRarity[] checkOrder = { RuneRarity.Common, RuneRarity.Rare, RuneRarity.Epic };
        RuneRarity selectedTargetRarity = RuneRarity.Common;
        bool foundValidGroup = false;

        foreach (RuneRarity targetRarity in checkOrder)
        {
            int matchCount = 0;
            foreach (RuneData r in filteredRunes)
            {
                if (r.runeRarity == targetRarity) matchCount++;
            }

            if (matchCount >= 3)
            {
                selectedTargetRarity = targetRarity;
                foundValidGroup = true;
                break;
            }
        }

        if (!foundValidGroup)
        {
            if (resultText != null) resultText.text = "<color=#FF4C4C>AutoFill failed: Need at least 3 matching runes!</color>";
            if (LobbyNotifyManager.Instance != null)
                LobbyNotifyManager.Instance.ShowNotify("Not enough matching material runes (minimum 3)!", Color.red);
            return;
        }

        int addedCount = 0;
        foreach (RuneData r in filteredRunes)
        {
            if (r.runeRarity == selectedTargetRarity)
            {
                AddRuneToFusion(r);
                addedCount++;
                if (addedCount >= 3) break;
            }
        }

        if (LobbyNotifyManager.Instance != null)
        {
            LobbyNotifyManager.Instance.ShowNotify($"Auto-filled 3 {selectedTargetRarity} material runes!", Color.green);
        }
    }

    private void OnFuseButtonClicked()
    {
        if (selectedRunes.Count != 3 || isAnimating) return;

        bool wantToUseCharm = useCharmToggle != null && useCharmToggle.isOn;
        if (wantToUseCharm && InventoryItemManager.Instance != null && InventoryItemManager.Instance.GetItemQuantity(charmItemID) < 1)
        {
            if (LobbyNotifyManager.Instance != null)
            {
                LobbyNotifyManager.Instance.ShowNotify("You do not possess any Protection Charms!", Color.red);
            }
            return;
        }

        List<string> ids = new List<string>();
        foreach (RuneData r in selectedRunes) ids.Add(r.runeID);

        StartFusionAnimationSequence(ids, wantToUseCharm);
    }

    private void StartFusionAnimationSequence(List<string> ingredientIDs, bool useProtection)
    {
        isAnimating = true;
        ClearRewardCard();

        if (resultText != null) resultText.text = "<color=#33FFFF>Channelling elemental particles...</color>";

        Sequence fusionSequence = DOTween.Sequence();

        for (int i = 0; i < spawnedVisualCards.Count; i++)
        {
            if (spawnedVisualCards[i] == null) continue;

            fusionSequence.Join(spawnedVisualCards[i].transform.DOMove(fusionCenterPoint.position, 0.6f).SetEase(Ease.InQuad));
            fusionSequence.Join(spawnedVisualCards[i].transform.DOScale(new Vector3(0.3f, 0.3f, 0.3f), 0.6f));
            fusionSequence.Join(spawnedVisualCards[i].transform.DORotate(new Vector3(0, 0, 360f), 0.6f, RotateMode.FastBeyond360));
        }

        fusionSequence.OnComplete(() =>
        {
            bool isSuccess;
            RuneData resultRune;

            bool execute = RuneFusionManager.Instance.TryFuseRunes(ingredientIDs, useProtection, out isSuccess, out resultRune);

            foreach (RuneCardUI visual in spawnedVisualCards) if (visual != null) Destroy(visual.gameObject);
            spawnedVisualCards.Clear();
            selectedRunes.Clear();

            if (execute)
            {
                if (isSuccess && resultRune != null)
                {
                    if (resultText != null) resultText.text = $"<color=#00FFCC>FUSION SUCCESSFUL!\nForged: {resultRune.runeName}</color>";
                    if (LobbyNotifyManager.Instance != null) LobbyNotifyManager.Instance.ShowNotify("Fusion successful! Higher tier rune acquired.", Color.green);

                    Camera.main.transform.DOShakePosition(0.3f, 15f, 20);

                    if (cardPrefabSample != null)
                    {
                        rewardCardInstance = Instantiate(cardPrefabSample, fusionCenterPoint);
                        rewardCardInstance.Setup(resultRune, false);

                        RectTransform rewardRect = rewardCardInstance.GetComponent<RectTransform>();
                        if (rewardRect != null)
                        {
                            rewardRect.anchoredPosition = Vector2.zero;
                            rewardRect.localPosition = Vector3.zero;
                        }

                        float rewardScale = 0.7f;
                        rewardCardInstance.transform.localScale = Vector3.zero;

                        rewardCardInstance.transform.DOScale(new Vector3(rewardScale * 1.3f, rewardScale * 1.3f, rewardScale * 1.3f), 0.4f).SetEase(Ease.OutElastic).OnComplete(() =>
                        {
                            if (rewardCardInstance != null)
                                rewardCardInstance.transform.DOScale(new Vector3(rewardScale, rewardScale, rewardScale), 0.15f);
                        });
                    }
                }
                else
                {
                    if (resultText != null) resultText.text = "<color=#FF4C4C>FUSION FAILED!\n20% Shards refunded.</color>";
                    if (LobbyNotifyManager.Instance != null) LobbyNotifyManager.Instance.ShowNotify("Fusion failed! Partial shards refunded.", Color.red);

                    fusionCenterPoint.DOShakePosition(0.4f, 25f, 30);
                }
            }

            isAnimating = false;
            ResetToggleState();
        });
    }
}