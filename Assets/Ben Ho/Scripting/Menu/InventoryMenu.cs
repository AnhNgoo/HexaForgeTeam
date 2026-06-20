using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryMenu : MenuBase
{
    public override MenuType menuType => MenuType.InventoryMenu;

    [Header("Buttons")]
    [SerializeField] private Button btn_Back;
    [SerializeField] private Button btn_Use;

    [Header("Display")]
    [SerializeField] private Image img_DisplayItem;

    [Header("Counter")]
    [SerializeField] private TextMeshProUGUI txt_CurrentAmount;
    [SerializeField] private TextMeshProUGUI txt_MaxAmount;
    [SerializeField] private int maxAmount = 30;

    [Header("Slots")]
    [SerializeField] private List<InventorySlotUI> slots = new List<InventorySlotUI>();

    private InventorySlotUI selectedSlot;

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
            btn_Back.onClick.RemoveListener(OnBackClicked);
            btn_Back.onClick.AddListener(OnBackClicked);
        }

        if (btn_Use != null)
        {
            btn_Use.onClick.RemoveListener(OnUseClicked);
            btn_Use.onClick.AddListener(OnUseClicked);
        }

        foreach (InventorySlotUI slot in slots)
        {
            if (slot != null)
                slot.Init(this);
        }

        SelectFirstItem();
        UpdateCounter();
    }

    public override void Close()
    {
        if (btn_Back != null)
            btn_Back.onClick.RemoveListener(OnBackClicked);

        if (btn_Use != null)
            btn_Use.onClick.RemoveListener(OnUseClicked);

        foreach (InventorySlotUI slot in slots)
        {
            if (slot != null)
                slot.RemoveListener();
        }

        base.Close();
    }

    public void SelectItem(InventorySlotUI slot)
    {
        selectedSlot = slot;

        if (img_DisplayItem != null)
        {
            img_DisplayItem.sprite = selectedSlot.GetIcon();
            img_DisplayItem.enabled = selectedSlot.HasItem;
        }

        Debug.Log("Selected item: " + selectedSlot.itemName);
    }

    private void OnUseClicked()
    {
        if (selectedSlot == null || !selectedSlot.HasItem)
        {
            Debug.Log("No item selected");
            return;
        }

        selectedSlot.Use();

        if (!selectedSlot.HasItem)
            SelectFirstItem();
        else
            SelectItem(selectedSlot);

        UpdateCounter();
    }

    private void OnBackClicked()
    {
        UIManager.Instance.ChangeMenu(MenuType.GameplayMenu);
    }

    private void SelectFirstItem()
    {
        selectedSlot = null;

        foreach (InventorySlotUI slot in slots)
        {
            if (slot != null && slot.HasItem)
            {
                SelectItem(slot);
                return;
            }
        }

        if (img_DisplayItem != null)
        {
            img_DisplayItem.sprite = null;
            img_DisplayItem.enabled = false;
        }
    }

    private void UpdateCounter()
    {
        int total = 0;

        foreach (InventorySlotUI slot in slots)
        {
            if (slot != null)
                total += slot.GetAmount();
        }

        if (txt_CurrentAmount != null)
            txt_CurrentAmount.text = total.ToString();

        if (txt_MaxAmount != null)
            txt_MaxAmount.text = maxAmount.ToString();
    }
}