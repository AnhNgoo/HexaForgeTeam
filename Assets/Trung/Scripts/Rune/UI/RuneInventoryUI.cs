using System.Collections.Generic;
using UnityEngine;
using TMPro;    
using UnityEngine.UI;

public class RuneInventoryUI : MonoBehaviour
{
    public static RuneInventoryUI Instance;
    
    [Header("Panel Root")]
    [SerializeField] private GameObject inventoryPanel;

    [Header("UI Grid Layout Content")]
    [SerializeField] private Transform contentParent;
    [SerializeField] private ScrollRect scrollRect;

    [Header("Card Prefab Template")]
    [SerializeField] private RuneCardUI cardPrefab;

    [Header("Select Mode Layout configuration")]
    [SerializeField] private GameObject selectAllButtonObj;         
    [SerializeField] private GameObject bulkDeleteButtonObj;       
    [SerializeField] private TMP_Text selectModeToggleButtonText;    
    private bool isSelectModeActive = false;                         

    [Header("Fusion Layout Button Config")]
    [SerializeField] private GameObject runeEquipPanelObj;  
    [SerializeField] private GameObject runeFusionPanelObj; 
    [SerializeField] private Button fusionModeButton;          
    [SerializeField] private TMP_Text fusionModeButtonText;      
    private bool isFusionActive = false;                         

    [Header("Inventory Capacity Settings")]
    [SerializeField] private TMP_Text capacityText; 
    [SerializeField] private int maxInventorySlots = 100; 

    [Header("Tab Switch Panel System")]
    [SerializeField] private GameObject runeMainPanelGroup; 
    [SerializeField] private GameObject itemMainPanelGroup; 
    [SerializeField] private Button tabRuneButton;          
    [SerializeField] private Button tabItemButton;

    private RuneCardUI lockedSelectedCardUI = null;
    private RuneData lockedSelectedRuneData = null;

    private HashSet<string> selectedRuneIDs = new HashSet<string>();

    private List<RuneCardUI> pooledCards = new List<RuneCardUI>();    

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        if (tabRuneButton != null)
        {
            tabRuneButton.onClick.RemoveAllListeners();
            tabRuneButton.onClick.AddListener(SwitchToRuneTab);
        }

        if (tabItemButton != null)
        {
            tabItemButton.onClick.RemoveAllListeners();
            tabItemButton.onClick.AddListener(SwitchToItemTab);
        }

        if (fusionModeButton != null)
        {
            fusionModeButton.onClick.RemoveAllListeners();
            fusionModeButton.onClick.AddListener(ToggleFusionMode);
        }

        SwitchToRuneTab();
    }

    private void OnEnable()
    {
        DeselectLockedRune();
        ResetFusionState();
        if (RuneFilterPanel.Instance != null) RuneFilterPanel.Instance.ResetFilterToDefault();

        DisableSelectMode();
        RefreshInventory();
    }

    public RuneData GetSelectedRuneData() => lockedSelectedRuneData;
    public bool IsFusionActive() => isFusionActive;
    public bool IsSelectModeActive() => isSelectModeActive;
    public bool IsItemTabActive() => itemMainPanelGroup != null && itemMainPanelGroup.activeInHierarchy;
    public bool IsRuneSelectedForDelete(string runeID) => selectedRuneIDs.Contains(runeID);

    public void ToggleRuneSelectionForDelete(string runeID)
    {
        if (selectedRuneIDs.Contains(runeID))
        {
            selectedRuneIDs.Remove(runeID);
        }
        else
        {
            selectedRuneIDs.Add(runeID);
        }
        RefreshInventoryCardVisuals();
    }

    public void OnRuneHovered(RuneData hoverRune)
    {
        if (lockedSelectedRuneData != null) return;

        if (RuneDetailInfoPanel.Instance != null && hoverRune != null)
        {
            RuneDetailInfoPanel.Instance.DisplayRuneInfo(hoverRune);
        }
    }

    public void OnRuneClicked(RuneCardUI clickedCardUI, RuneData clickedRune)
    {
        if (clickedCardUI == null || clickedRune == null) return;

        if (isSelectModeActive)
        {
            ToggleRuneSelectionForDelete(clickedRune.runeID);
            return;
        }

        if (lockedSelectedCardUI == clickedCardUI)
        {
            DeselectLockedRune();
            return;
        }

        if (lockedSelectedCardUI != null)
        {
            lockedSelectedCardUI.SetSelected(false);
        }

        lockedSelectedCardUI = clickedCardUI;
        lockedSelectedRuneData = clickedRune;

        lockedSelectedCardUI.SetSelected(true);

        if (RuneDetailInfoPanel.Instance != null)
        {
            RuneDetailInfoPanel.Instance.DisplayRuneInfo(lockedSelectedRuneData);
        }
    }

    public void DeselectLockedRune()
    {
        if (lockedSelectedCardUI != null)
        {
            lockedSelectedCardUI.SetSelected(false);
        }

        lockedSelectedCardUI = null;
        lockedSelectedRuneData = null;
    }

    public void ResetFusionState()
    {
        isFusionActive = false;
        if (runeEquipPanelObj != null) runeEquipPanelObj.SetActive(true);
        if (runeFusionPanelObj != null) runeFusionPanelObj.SetActive(false);
        
        if (fusionModeButtonText != null)
        {
            fusionModeButtonText.SetTextSafe("Fusion Mode");
            fusionModeButtonText.color = Color.white;
        }

        if (RuneFusionUI.Instance != null)
        {
            RuneFusionUI.Instance.ClearFusionSlots();
        }
    }

    public void OpenInventory()
    {
        if (inventoryPanel != null) inventoryPanel.SetActive(true);
        RefreshInventory();
    }

    public void CloseInventory()
    {
        DeselectLockedRune();
        ResetFusionState();
        
        if (RuneFusionUI.Instance != null) RuneFusionUI.Instance.ClearFusionSlots();
        
        if (RuneFilterPanel.Instance != null) 
        {
            RuneFilterPanel.Instance.ResetFilterToDefault();
            RuneFilterPanel.Instance.CloseFilterPanel();
        }
        
        DisableSelectMode();

        if (inventoryPanel != null) inventoryPanel.SetActive(false);
    }

    public void ToggleFusionMode()
    {
        if (runeEquipPanelObj == null || runeFusionPanelObj == null) return;

        isFusionActive = !isFusionActive;

        runeEquipPanelObj.SetActive(!isFusionActive);
        runeFusionPanelObj.SetActive(isFusionActive);

        if (fusionModeButtonText != null)
        {
            if (isFusionActive)
            {
                fusionModeButtonText.SetTextSafe("Cancel Fusion Mode");
                fusionModeButtonText.color = new Color(1f, 0.4f, 0.4f);
            }
            else
            {
                fusionModeButtonText.SetTextSafe("Fusion Mode");
                fusionModeButtonText.color = Color.white;
            }
        }

        if (RuneFusionUI.Instance != null) RuneFusionUI.Instance.ClearFusionSlots();
        if (!isFusionActive && RuneEquipUI.Instance != null) RuneEquipUI.Instance.RefreshEquipUI();
    }

    public void ToggleSelectMode()
    {
        isSelectModeActive = !isSelectModeActive;
        selectedRuneIDs.Clear();

        if (selectAllButtonObj != null) 
            selectAllButtonObj.SetActive(isSelectModeActive);
            
        if (bulkDeleteButtonObj != null) 
            bulkDeleteButtonObj.SetActive(isSelectModeActive);

        try
        {
            if (selectModeToggleButtonText != null)
                selectModeToggleButtonText.SetTextSafe(isSelectModeActive ? "Cancel Select" : "Select Mode");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[InventoryUI Protect] Error Font: {e.Message}");
        }

        RefreshInventory();
    }

    public void DisableSelectMode()
    {
        isSelectModeActive = false;
        selectedRuneIDs.Clear();
        
        if (selectAllButtonObj != null) selectAllButtonObj.SetActive(false);
        if (bulkDeleteButtonObj != null) bulkDeleteButtonObj.SetActive(false);
        
        try
        {
            if (selectModeToggleButtonText != null) selectModeToggleButtonText.SetTextSafe("Select Mode");
        }
        catch {}
    }

    public void SmartSelectAllVisibleRunes()
    {
        if (!isSelectModeActive)
        {
            ToggleSelectMode();
        }

        if (RuneInventoryManager.Instance == null) return;

        selectedRuneIDs.Clear();
        List<RuneData> runes = RuneInventoryManager.Instance.runes;

        for (int i = 0; i < runes.Count; i++)
        {
            if (runes[i] == null) continue;

            if (RuneFilterPanel.Instance != null && !RuneFilterPanel.Instance.EvaluateRuneFilter(runes[i]))
            {
                continue;
            }

            selectedRuneIDs.Add(runes[i].runeID);
        }

        RefreshInventoryCardVisuals();

        if (LobbyNotifyManager.Instance != null)
        {
            LobbyNotifyManager.Instance.ShowNotify($"Selected all {selectedRuneIDs.Count} visible runes!", Color.yellow);
        }
    }

    public void DismantleSelectedRunesLoat()
    {
        List<RuneData> runesToRemove = new List<RuneData>();

        if (RuneInventoryManager.Instance != null)
        {
            List<RuneData> allRunes = RuneInventoryManager.Instance.runes;
            for (int i = 0; i < allRunes.Count; i++)
            {
                if (allRunes[i] != null && selectedRuneIDs.Contains(allRunes[i].runeID))
                {
                    runesToRemove.Add(allRunes[i]);
                }
            }
        }

        if (runesToRemove.Count == 0 && lockedSelectedRuneData != null)
        {
            runesToRemove.Add(lockedSelectedRuneData);
        }

        if (runesToRemove.Count == 0)
        {
            if (!isSelectModeActive)
            {
                ToggleSelectMode();
                if (LobbyNotifyManager.Instance != null)
                {
                    LobbyNotifyManager.Instance.ShowNotify("Select Mode Activated! Click runes to select.", Color.cyan);
                }
            }
            else
            {
                if (LobbyNotifyManager.Instance != null)
                {
                    LobbyNotifyManager.Instance.ShowNotify("No runes selected to dismantle!", Color.yellow);
                }
            }
            return;
        }

        int totalGemsRefund = 0;
        int totalShardsRefund = 0;

        for (int i = 0; i < runesToRemove.Count; i++)
        {
            RuneData runeData = runesToRemove[i];

            if (CharacterManager.Instance != null)
            {
                CharacterType[] allChars = (CharacterType[])System.Enum.GetValues(typeof(CharacterType));
                foreach (CharacterType charType in allChars)
                {
                    var build = CharacterManager.Instance.GetCharacterRuneBuild(charType);
                    if (build != null && build.equippedRuneIDs != null)
                    {
                        for (int slot = 0; slot < build.equippedRuneIDs.Length; slot++)
                        {
                            if (build.equippedRuneIDs[slot] == runeData.runeID)
                            {
                                build.equippedRuneIDs[slot] = "";
                            }
                        }
                    }
                }
            }

            int gemBack = 10;
            int shardBack = 50;
            switch (runeData.runeRarity)
            {
                case RuneRarity.Rare: gemBack = 25; shardBack = 150; break;
                case RuneRarity.Epic: gemBack = 60; shardBack = 400; break;
                case RuneRarity.Legendary: gemBack = 150; shardBack = 1000; break;
            }

            totalGemsRefund += gemBack;
            totalShardsRefund += shardBack;

            if (RuneInventoryManager.Instance != null)
            {
                RuneInventoryManager.Instance.runes.Remove(runeData);
            }
        }

        if (GemManager.Instance != null && totalGemsRefund > 0)
        {
            GemManager.Instance.AddGem(totalGemsRefund);
        }

        if (RuneShardManager.Instance != null && totalShardsRefund > 0)
        {
            RuneShardManager.Instance.AddShards(totalShardsRefund);
        }

        if (RuneInventoryManager.Instance != null)
        {
            RuneInventoryManager.Instance.SaveRunes();
        }

        DeselectLockedRune();
        DisableSelectMode();

        RefreshInventory();

        if (RuneEquipUI.Instance != null)
        {
            RuneEquipUI.Instance.RefreshEquipUI();
        }

        if (LobbyStatManager.Instance != null)
        {
            LobbyStatManager.Instance.RecalculateStats();
        }

        if (LobbyNotifyManager.Instance != null)
        {
            LobbyNotifyManager.Instance.ShowNotify($"Dismantled {runesToRemove.Count} runes! +{totalGemsRefund} Gems, +{totalShardsRefund} Shards.", Color.green);
        }
    }

    public void RefreshInventory()
    {
        if (capacityText != null && RuneInventoryManager.Instance != null)
        {
            int currentCount = RuneInventoryManager.Instance.runes.Count;
            
            try
            {
                capacityText.SetTextSafe($"Slots: {currentCount} / {maxInventorySlots}");
                capacityText.color = (currentCount >= maxInventorySlots * 0.9f) ? Color.red : Color.white;
            }
            catch {}
        }

        for (int i = 0; i < pooledCards.Count; i++)
        {
            if (pooledCards[i] != null) pooledCards[i].gameObject.SetActive(false);
        }

        if (RuneInventoryManager.Instance == null) return;

        List<RuneData> sortedRunes = new List<RuneData>(RuneInventoryManager.Instance.runes);
        sortedRunes.Sort((a, b) => b.runeRarity.CompareTo(a.runeRarity));

        int visibleCardCount = 0;

        try
        {
            for (int i = 0; i < sortedRunes.Count; i++)
            {
                if (RuneFilterPanel.Instance != null)
                {
                    if (!RuneFilterPanel.Instance.EvaluateRuneFilter(sortedRunes[i])) continue;
                }

                GetOrCreatePooledCard(sortedRunes[i], visibleCardCount);
                visibleCardCount++;
            }
            Canvas.ForceUpdateCanvases();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("[InventoryUI pooling] Render error ignored: " + e.Message);
        }

        if (scrollRect != null) scrollRect.verticalNormalizedPosition = 1f;
    }

    private void RefreshInventoryCardVisuals()
    {
        for (int i = 0; i < pooledCards.Count; i++)
        {
            if (pooledCards[i] != null && pooledCards[i].gameObject.activeInHierarchy)
            {
                RuneData data = pooledCards[i].GetRuneData();
                if (data != null)
                {
                    bool isSel = isSelectModeActive ? selectedRuneIDs.Contains(data.runeID) : (lockedSelectedRuneData != null && lockedSelectedRuneData.runeID == data.runeID);
                    pooledCards[i].SetSelectedDirect(isSel);
                    pooledCards[i].UpdateSelectModeVisual(isSelectModeActive);
                }
            }
        }
    }

    private void GetOrCreatePooledCard(RuneData runeData, int index)
    {
        RuneCardUI card;

        if (index < pooledCards.Count)
        {
            card = pooledCards[index];
            if (card == null) 
            {
                card = Instantiate(cardPrefab, contentParent);
                pooledCards[index] = card;
            }
        }
        else
        {
            card = Instantiate(cardPrefab, contentParent);
            pooledCards.Add(card);
        }

        card.gameObject.SetActive(true);
        card.Setup(runeData, false);
        card.SetDisableTooltip(true);

        bool isSel = isSelectModeActive ? selectedRuneIDs.Contains(runeData.runeID) : (lockedSelectedRuneData != null && lockedSelectedRuneData.runeID == runeData.runeID);
        card.SetSelectedDirect(isSel);
        card.UpdateSelectModeVisual(isSelectModeActive);
    }

    public void OpenFilterPanel()
    {
        if (RuneFilterPanel.Instance != null) RuneFilterPanel.Instance.OpenFilterPanel();
    }

    public void CloseFilterPanel()
    {
        if (RuneFilterPanel.Instance != null) RuneFilterPanel.Instance.CloseFilterPanel();
    }

    public void SwitchToRuneTab()
    {
        DisableSelectMode();

        if (runeMainPanelGroup != null) runeMainPanelGroup.SetActive(true);
        if (itemMainPanelGroup != null) itemMainPanelGroup.SetActive(false);

        RefreshInventory();
    }

    public void SwitchToItemTab()
    {
        DisableSelectMode();

        if (runeMainPanelGroup != null) runeMainPanelGroup.SetActive(false);
        if (itemMainPanelGroup != null) itemMainPanelGroup.SetActive(true);

        if (LobbyInventoryItemUI.Instance != null)
        {
            LobbyInventoryItemUI.Instance.RefreshItemInventory();
        }
    }
}