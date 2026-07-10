using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RuneFilterPanel : MonoBehaviour
{
    public static RuneFilterPanel Instance;

    [Header("Panel Root UI")]
    [SerializeField] private GameObject filterPanelRoot;

    [Header("Select Mode Button Container")]
    [SerializeField] private GameObject selectModeButtonObj;

    [Header("Filter Toggles")]
    [SerializeField] private Toggle toggleAll; 
    [SerializeField] private Toggle toggleCommon;
    [SerializeField] private Toggle toggleRare;
    [SerializeField] private Toggle toggleEpic;
    [SerializeField] private Toggle toggleLegendary;
    [SerializeField] private Toggle toggleRed;
    [SerializeField] private Toggle toggleGreen;
    [SerializeField] private Toggle toggleBlue;

    [Header("Color Settings")]
    [SerializeField] private Color activeColor = new Color(1f, 0.85f, 0.2f); 
    [SerializeField] private Color inactiveColor = Color.white;              

    private bool isIgnoreCallback = false; 

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (filterPanelRoot != null) filterPanelRoot.SetActive(false);
        if (selectModeButtonObj != null) selectModeButtonObj.SetActive(false);

        if (toggleAll != null) toggleAll.onValueChanged.AddListener(OnToggleAllChanged);
        if (toggleCommon != null) toggleCommon.onValueChanged.AddListener((isOn) => OnSingleFilterChanged(toggleCommon));
        if (toggleRare != null) toggleRare.onValueChanged.AddListener((isOn) => OnSingleFilterChanged(toggleRare));
        if (toggleEpic != null) toggleEpic.onValueChanged.AddListener((isOn) => OnSingleFilterChanged(toggleEpic));
        if (toggleLegendary != null) toggleLegendary.onValueChanged.AddListener((isOn) => OnSingleFilterChanged(toggleLegendary));
        if (toggleRed != null) toggleRed.onValueChanged.AddListener((isOn) => OnSingleFilterChanged(toggleRed));
        if (toggleGreen != null) toggleGreen.onValueChanged.AddListener((isOn) => OnSingleFilterChanged(toggleGreen));
        if (toggleBlue != null) toggleBlue.onValueChanged.AddListener((isOn) => OnSingleFilterChanged(toggleBlue));

        isIgnoreCallback = true;
        if (toggleAll != null) toggleAll.isOn = true;
        SetAllChildToggles(true);
        isIgnoreCallback = false;
        UpdateAllToggleVisuals();
    }

    private void Update()
    {
        // FIXED: CLICK CHUỘT RA NGOÀI VÙNG TRỐNG TỰ ĐỘNG ĐÓNG BẢNG LỌC
        if (filterPanelRoot == null || !filterPanelRoot.activeSelf) return;

        if (Input.GetMouseButtonDown(0))
        {
            RectTransform panelRect = filterPanelRoot.GetComponent<RectTransform>();
            if (panelRect != null)
            {
                // Kiểm tra va chạm hình học RectTransform
                bool clickInside = RectTransformUtility.RectangleContainsScreenPoint(panelRect, Input.mousePosition, null);
                
                if (!clickInside)
                {
                    CloseFilterPanel();
                }
            }
        }
    }

    public void OpenFilterPanel()
    {
        if (filterPanelRoot != null) filterPanelRoot.SetActive(true);
        if (selectModeButtonObj != null) selectModeButtonObj.SetActive(true);
        
        ResetFilterToDefault();
    }

    public void CloseFilterPanel()
    {
        if (filterPanelRoot != null) filterPanelRoot.SetActive(false);
        if (selectModeButtonObj != null) selectModeButtonObj.SetActive(false); 
    }

    public void ResetFilterToDefault()
    {
        isIgnoreCallback = true;
        if (toggleAll != null) toggleAll.isOn = true;
        SetAllChildToggles(true);
        isIgnoreCallback = false;
        
        UpdateAllToggleVisuals();
        NotifyInventoryRefresh(); // Ép hòm đồ lưới đồng bộ hiển thị lại toàn bộ ngọc ngay lập tức
    }

    private void OnToggleAllChanged(bool isOn)
    {
        if (isIgnoreCallback) return;

        isIgnoreCallback = true;
        SetAllChildToggles(isOn);
        isIgnoreCallback = false;

        UpdateAllToggleVisuals();
        NotifyInventoryRefresh();
    }

    private void OnSingleFilterChanged(Toggle targetToggle)
    {
        if (isIgnoreCallback) return;

        UpdateSingleToggleVisual(targetToggle);
        CheckAndUpdateToggleAllState();
        NotifyInventoryRefresh();
    }

    private void SetAllChildToggles(bool isOn)
    {
        if (toggleCommon != null) toggleCommon.isOn = isOn;
        if (toggleRare != null) toggleRare.isOn = isOn;
        if (toggleEpic != null) toggleEpic.isOn = isOn;
        if (toggleLegendary != null) toggleLegendary.isOn = isOn;
        if (toggleRed != null) toggleRed.isOn = isOn;
        if (toggleGreen != null) toggleGreen.isOn = isOn;
        if (toggleBlue != null) toggleBlue.isOn = isOn;
    }

    private void CheckAndUpdateToggleAllState()
    {
        if (toggleAll == null) return;

        bool allOn = (toggleCommon == null || toggleCommon.isOn) &&
                     (toggleRare == null || toggleRare.isOn) &&
                     (toggleEpic == null || toggleEpic.isOn) &&
                     (toggleLegendary == null || toggleLegendary.isOn) &&
                     (toggleRed == null || toggleRed.isOn) &&
                     (toggleGreen == null || toggleGreen.isOn) &&
                     (toggleBlue == null || toggleBlue.isOn);

        isIgnoreCallback = true;
        toggleAll.isOn = allOn;
        isIgnoreCallback = false;
        UpdateSingleToggleVisual(toggleAll);
    }

    public bool EvaluateRuneFilter(RuneData runeData)
    {
        if (runeData == null) return false;

        bool anyRarityFilterActive = (toggleCommon != null && toggleCommon.isOn) || 
                                     (toggleRare != null && toggleRare.isOn) || 
                                     (toggleEpic != null && toggleEpic.isOn) || 
                                     (toggleLegendary != null && toggleLegendary.isOn);

        bool anyColorFilterActive = (toggleRed != null && toggleRed.isOn) || 
                                    (toggleGreen != null && toggleGreen.isOn) || 
                                    (toggleBlue != null && toggleBlue.isOn);

        if (!anyRarityFilterActive && !anyColorFilterActive) return false;

        bool rarityMatch = !anyRarityFilterActive;
        if (anyRarityFilterActive)
        {
            switch (runeData.runeRarity)
            {
                case RuneRarity.Common: rarityMatch = toggleCommon != null && toggleCommon.isOn; break;
                case RuneRarity.Rare: rarityMatch = toggleRare != null && toggleRare.isOn; break;
                case RuneRarity.Epic: rarityMatch = toggleEpic != null && toggleEpic.isOn; break;
                case RuneRarity.Legendary: rarityMatch = toggleLegendary != null && toggleLegendary.isOn; break;
            }
        }

        bool colorMatch = !anyColorFilterActive;
        if (anyColorFilterActive)
        {
            switch (runeData.runeColor)
            {
                case RuneColor.Red: colorMatch = toggleRed != null && toggleRed.isOn; break;
                case RuneColor.Green: colorMatch = toggleGreen != null && toggleGreen.isOn; break;
                case RuneColor.Blue: colorMatch = toggleBlue != null && toggleBlue.isOn; break;
            }
        }

        return rarityMatch && colorMatch;
    }

    private void NotifyInventoryRefresh()
    {
        if (InventoryUI.Instance != null && InventoryUI.Instance.gameObject.activeInHierarchy)
        {
            InventoryUI.Instance.RefreshInventory();
        }
    }

    private void UpdateAllToggleVisuals()
    {
        UpdateSingleToggleVisual(toggleAll);
        UpdateSingleToggleVisual(toggleCommon);
        UpdateSingleToggleVisual(toggleRare);
        UpdateSingleToggleVisual(toggleEpic);
        UpdateSingleToggleVisual(toggleLegendary);
        UpdateSingleToggleVisual(toggleRed);
        UpdateSingleToggleVisual(toggleGreen);
        UpdateSingleToggleVisual(toggleBlue);
    }

    private void UpdateSingleToggleVisual(Toggle toggle)
    {
        if (toggle == null) return;
        Color targetColor = toggle.isOn ? activeColor : inactiveColor;

        try
        {
            TMP_Text txt = toggle.GetComponentInChildren<TMP_Text>();
            if (txt != null) txt.color = targetColor;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[FilterPanel Protect] Chặn lỗi Font chữ TMPro: {e.Message}");
        }

        Image img = toggle.GetComponent<Image>();
        if (img != null) img.color = targetColor;
    }
}