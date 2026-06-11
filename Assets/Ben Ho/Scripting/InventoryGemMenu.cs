using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryGemMenu : MenuBase
{
    public override MenuType menuType => MenuType.InventoryGemMenu;

    [Header("Buttons")]
    [SerializeField] private Button btn_Back;
    [SerializeField] private Button btn_Use;

    [Header("Display")]
    [SerializeField] private Image img_DisplayGem;
    [SerializeField] private TextMeshProUGUI txt_GemName;
    [SerializeField] private TextMeshProUGUI txt_GemAmount;
    [SerializeField] private TextMeshProUGUI txt_TotalGem;

    // [Header("Slots")]
    // [SerializeField] private List<InventoryGemSlotUI> gemSlots = new List<InventoryGemSlotUI>();

    [Header("Back Menu")]
    [SerializeField] private MenuType backMenu = MenuType.InventoryMenu;

    // private InventoryGemItem selectedGem;

    protected override void LoadComponent()
    {

    }

    protected override void LoadComponentRuntime()
    {

    }

    public override void Open(object data = null)
    {
        base.Open(data);

        if (btn_Back != null)
        {
            btn_Back.onClick.RemoveListener(OnBackButtonClicked);
            btn_Back.onClick.AddListener(OnBackButtonClicked);
        }

        if (btn_Use != null)
        {
            btn_Use.onClick.RemoveListener(OnUseButtonClicked);
            btn_Use.onClick.AddListener(OnUseButtonClicked);
        }

        // RefreshGemUI();
    }

    public override void Close()
    {
        if (btn_Back != null)
            btn_Back.onClick.RemoveListener(OnBackButtonClicked);

        if (btn_Use != null)
            btn_Use.onClick.RemoveListener(OnUseButtonClicked);

        // foreach (InventoryGemSlotUI slot in gemSlots)
        // {
        //     if (slot != null)
        //         slot.RemoveListener();
        // }

        base.Close();
    }

    // public void SelectGem(InventoryGemItem gem)
    // {
    //     selectedGem = gem;

    //     if (selectedGem == null || selectedGem.gemData == null) return;

    //     if (img_DisplayGem != null)
    //     {
    //         img_DisplayGem.sprite = selectedGem.gemData.gemIcon;
    //         img_DisplayGem.enabled = true;
    //     }

    //     if (txt_GemName != null)
    //         txt_GemName.text = selectedGem.gemData.gemName;

    //     if (txt_GemAmount != null)
    //         txt_GemAmount.text = "x" + selectedGem.amount;

    //     Debug.Log("Selected Gem: " + selectedGem.gemData.gemName);
    // }

    private void OnUseButtonClicked()
    {
        // if (selectedGem == null || selectedGem.gemData == null)
        // {
        //     Debug.Log("Chưa chọn gem.");
        //     return;
        // }

        // if (InventoryGem.Instance == null)
        // {
        //     Debug.LogWarning("Không tìm thấy InventoryGem.");
        //     return;
        // }

        // InventoryGem.Instance.UseGem(selectedGem.gemData, 1);

        // selectedGem = null;
        // RefreshGemUI();

        Debug.Log("Use Gem button clicked");
    }

    private void OnBackButtonClicked()
    {
        Debug.Log("Inventory Gem Back button clicked");

        UIManager.Instance.ChangeMenu(backMenu);
    }

    // private void RefreshGemUI()
    // {
    //     if (InventoryGem.Instance == null)
    //     {
    //         Debug.LogWarning("Không tìm thấy InventoryGem trong scene.");
    //         ClearDisplay();
    //         return;
    //     }

    //     List<InventoryGemItem> gems = InventoryGem.Instance.Gems;

    //     for (int i = 0; i < gemSlots.Count; i++)
    //     {
    //         if (gemSlots[i] == null) continue;

    //         if (i < gems.Count)
    //             gemSlots[i].Init(this, gems[i]);
    //         else
    //             gemSlots[i].Clear();
    //     }

    //     if (gems.Count > 0)
    //         SelectGem(gems[0]);
    //     else
    //         ClearDisplay();

    //     if (txt_TotalGem != null)
    //         txt_TotalGem.text = InventoryGem.Instance.GetTotalGemAmount().ToString();
    // }

    private void ClearDisplay()
    {
        // selectedGem = null;

        if (img_DisplayGem != null)
        {
            img_DisplayGem.sprite = null;
            img_DisplayGem.enabled = false;
        }

        if (txt_GemName != null)
            txt_GemName.text = "";

        if (txt_GemAmount != null)
            txt_GemAmount.text = "";

        if (txt_TotalGem != null)
            txt_TotalGem.text = "0";
    }
}