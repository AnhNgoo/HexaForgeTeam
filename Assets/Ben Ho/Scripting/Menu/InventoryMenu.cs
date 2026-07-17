using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;

public class InventoryMenu : MenuBase
{
    public override MenuType menuType => MenuType.InventoryMenu;

    [Header("Buttons")]
    [SerializeField] private Button btn_Back;
    [SerializeField] private Button btn_Use;
    [SerializeField] private Button btn_Discard;

    [Header("Display")]
    [SerializeField] private Image img_DisplayItem;

    [Header("Counter")]
    [SerializeField] private TextMeshProUGUI txt_CurrentAmount;
    [SerializeField] private TextMeshProUGUI txt_MaxAmount;
    [SerializeField] private int maxAmount = 30;

    [Header("Slots")]
    [SerializeField] private List<InventorySlotUI> slots = new List<InventorySlotUI>();

    [Header("Weapon Inventory")]
    [SerializeField] private GameObject weaponContents;
    [SerializeField] private List<ItemSlot> weaponSlots = new List<ItemSlot>();

    private InventorySlotUI selectedSlot;

    protected override void LoadComponent()
    {
        GetweaponSlots();
        if (btn_Discard == null)
            btn_Discard = transform.Find("Bottom-Bar/Controller_Button/Btn_Discard")?.GetComponent<Button>();
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

        if (btn_Discard != null)
        {
            btn_Discard.gameObject.SetActive(false);
        }
        foreach (InventorySlotUI slot in slots)
        {
            if (slot != null)
                slot.Init(this);
        }

        SelectFirstItem();
        UpdateCounter();
        DisableSelectedImages();
        EventManager.Subscribe(GameEvent.OnSelectItemInInventory, OnShowDiscardButton);
        EventManager.Subscribe(GameEvent.OnDeselectItemInInventory, OnHideDiscardButton);

    }

    public override void Close()
    {
        base.Close();
        if (btn_Back != null)
            btn_Back.onClick.RemoveListener(OnBackClicked);

        if (btn_Use != null)
            btn_Use.onClick.RemoveListener(OnUseClicked);

        foreach (InventorySlotUI slot in slots)
        {
            if (slot != null)
                slot.RemoveListener();
        }

        EventManager.Unsubscribe(GameEvent.OnSelectItemInInventory, OnShowDiscardButton);
        EventManager.Unsubscribe(GameEvent.OnDeselectItemInInventory, OnHideDiscardButton);
    }

    protected override void Awake()
    {
        base.Awake();
        EventManager.Subscribe(GameEvent.OnAddWeaponToInventory, OnAddWeaponToWeaponSlots);
        EventManager.Subscribe(GameEvent.OnDiscardItemInInventory, OnDiscardWeaponFromWeaponSlots);
    }

    protected void OnDestroy()
    {
        EventManager.Unsubscribe(GameEvent.OnAddWeaponToInventory, OnAddWeaponToWeaponSlots);
        EventManager.Unsubscribe(GameEvent.OnDiscardItemInInventory, OnDiscardWeaponFromWeaponSlots);
    }

    private void Update()
    {
        if (InputManager.InputActions.Keyboard.Escape.triggered)
        {
            UIManager.Instance.ChangeMenu(MenuType.GameplayMenu);
        }
    }

    private void OnHideDiscardButton(object obj)
    {
        if (btn_Discard != null)
            btn_Discard.gameObject.SetActive(false);
    }

    private void OnShowDiscardButton(object obj)
    {
        if (btn_Discard != null && btn_Discard.gameObject.activeSelf == false)
            btn_Discard.gameObject.SetActive(true);
    }
    #region Weapon Inventory
    private void GetweaponSlots()
    {
        if (weaponContents == null)
            weaponContents = transform.Find("main/Left-Content/Content-Items/Scroll View/Viewport/Content/WeaponContents")?.gameObject;

        if (weaponContents == null)
            return;

        if (weaponSlots.Count < weaponContents.transform.childCount)
        {
            weaponSlots.Clear();
            for (int i = 0; i < weaponContents.transform.childCount; i++)
            {
                ItemSlot slot = weaponContents.transform.GetChild(i).GetComponent<ItemSlot>();
                if (slot != null)
                    weaponSlots.Add(slot);
            }
        }
    }

    public bool CheckWeaponSlots()
    {
        foreach (ItemSlot slot in weaponSlots)
        {
            if (slot != null && slot.isEmpty)
                return true;
        }
        return false;
    }
    private void OnAddWeaponToWeaponSlots(object obj)
    {
        if (obj is not ItemSLotData itemSLotData)
        {
            Debug.LogWarning("Invalid item slot data.");
            return;
        }

        foreach (ItemSlot slot in weaponSlots)
        {
            if (slot != null && slot.isEmpty)
            {
                slot.AddItemIntoSlot(itemSLotData);
                break;
            }
        }
    }

    private void OnDiscardWeaponFromWeaponSlots(object obj)
    {
        if (obj is not int weaponIndex)
        {
            Debug.LogWarning("Invalid weapon index.");
            return;
        }

        foreach (ItemSlot slot in weaponSlots)
        {
            if (slot != null && !slot.isEmpty && slot.Index == weaponIndex)
            {
                slot.DiscardItemFromSlot();
                break;
            }
        }
    }

    // Ẩn tất cả các selectedImage của các slot vũ khí
    private void DisableSelectedImages()
    {
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
    #endregion
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