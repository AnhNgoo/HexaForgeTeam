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

    [Header("Reward Weapon Replacement")]
    [SerializeField] private GameObject rewardReplacementHeader;
    [SerializeField] private Image rewardWeaponIcon;
    [SerializeField] private TMP_Text rewardWeaponName;
    [SerializeField] private TMP_Text useButtonLabel;

    private WeaponReplacementMenuData replacementData;
    private int selectedWeaponIndex = -1;
    private string defaultUseButtonText = "Use";

    private bool IsReplacementMode => replacementData != null;

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
        {
            btn_Discard = transform.Find("Bottom-Bar/Controller_Button/Btn_Discard")?.GetComponent<Button>();
        }

        if (useButtonLabel == null && btn_Use != null)
        {
            useButtonLabel = btn_Use.GetComponentInChildren<TMP_Text>(true);
        }

        if (btn_Back == null)
        {
            btn_Back = transform.Find("Bottom-Bar/Controller_Button/Btn_Back")?.GetComponent<Button>();
        }

        if (rewardReplacementHeader == null)
        {
            rewardReplacementHeader = transform.Find("RewardReplacementHeader")?.gameObject;
        }

        if (rewardReplacementHeader != null)
        {
            if (rewardWeaponIcon == null)
            {
                rewardWeaponIcon = rewardReplacementHeader.transform.Find("RewardIcon")?.GetComponent<Image>();
            }

            if (rewardWeaponName == null)
            {
                rewardWeaponName = rewardReplacementHeader.transform.Find("RewardName")?.GetComponent<TMP_Text>();
            }
        }
        if (btn_Back == null)
        {
            btn_Back = transform.Find("Bottom-Bar/Controller_Button/Btn_Back")?.GetComponent<Button>();
        }
    }

    protected override void LoadComponentRuntime()
    {
        LoadComponent();
    }

    public override void Open(object data = null)
    {
        base.Open(data);
        replacementData = data as WeaponReplacementMenuData;
        selectedWeaponIndex = -1;

        if (useButtonLabel != null && string.IsNullOrEmpty(defaultUseButtonText))
        {
            defaultUseButtonText = useButtonLabel.text;
        }

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
        ConfigureReplacementMode();
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

        WeaponInventorySystem.Instance?.SetRewardReplacementMode(false);
        rewardReplacementHeader?.SetActive(false);

        if (useButtonLabel != null) useButtonLabel.text = defaultUseButtonText;
        if (btn_Use != null) btn_Use.interactable = true;

        replacementData = null;
        selectedWeaponIndex = -1;
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

    private void OnHideDiscardButton(object obj)
    {
        selectedWeaponIndex = -1;
        if (IsReplacementMode && btn_Use != null)
        {
            btn_Use.interactable = false;
            return;
        }
        if (btn_Discard != null)
            btn_Discard.gameObject.SetActive(false);
    }

    private void OnShowDiscardButton(object obj)
    {
        if (obj is not int index)
            return;

        if (IsReplacementMode)
        {
            selectedWeaponIndex = index;
            if (btn_Use != null) btn_Use.interactable = true;
            if (btn_Discard != null) btn_Discard.gameObject.SetActive(false);
            return;
        }

        if (btn_Discard != null) btn_Discard.gameObject.SetActive(true);
    }

    private void ConfigureReplacementMode()
    {
        bool active = IsReplacementMode;
        if (useButtonLabel != null)
        {
            useButtonLabel.text =
                active ? "REPLACE" : defaultUseButtonText;
        }
        rewardReplacementHeader?.SetActive(active);
        WeaponInventorySystem.Instance?.SetRewardReplacementMode(active);

        if (!active)
            return;

        if (rewardWeaponIcon != null)
        {
            rewardWeaponIcon.sprite = replacementData.RewardWeapon.itemIcon;
            rewardWeaponIcon.enabled = replacementData.RewardWeapon.itemIcon != null;
        }

        if (rewardWeaponName != null)
        {
            rewardWeaponName.text = replacementData.RewardWeapon.itemName;
        }

        if (btn_Use != null)
        {
            btn_Use.gameObject.SetActive(true);
            btn_Use.interactable = false;
        }

        if (btn_Discard != null) btn_Discard.gameObject.SetActive(false);
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
        if (IsReplacementMode)
        {
            if (selectedWeaponIndex < 0)
                return;
            bool replaced = replacementData.OnConfirmed?.Invoke(selectedWeaponIndex) == true;

            if (!replaced)
            {
                NotifyUI notify = ObjectPooling.Instance?.SpawnFromPool(PoolType.NotifyUI)?.GetComponent<NotifyUI>();
                notify?.SetDescription("The weapon could not be replaced.");
            }

            return;
        }

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
        if (IsReplacementMode)
        {
            Action cancel = replacementData.OnCancelled;
            replacementData = null;
            WeaponInventorySystem.Instance?.SetRewardReplacementMode(false);
            cancel?.Invoke();
            return;
        }

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