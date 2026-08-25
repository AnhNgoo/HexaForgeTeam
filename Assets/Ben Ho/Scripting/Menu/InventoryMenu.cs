using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
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

    [Header("Rune Inventory")]
    [SerializeField] private GameObject runeContents;
    [SerializeField] private List<ItemSlot> runeSlots = new List<ItemSlot>();

    [Header("Rune Visual Sprites Config (12 Ngọc)")]
    [SerializeField] private List<Sprite> runeSprites = new List<Sprite>();
    [SerializeField] private Sprite originRuneSprite;

    private InventorySlotUI selectedSlot;

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

    protected override void LoadComponent()
    {
        GetWeaponSlots();
        GetRuneSlots();

        if (btn_Discard == null)
            btn_Discard = transform.Find("Bottom-Bar/Controller_Button/Btn_Discard")?.GetComponent<Button>();
    }

    protected override void LoadComponentRuntime() { }

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

        // TỰ ĐỘNG LÀM MỚI VÀ ĐỒNG BỘ RUNE BUILD HIỆN TẠI
        RefreshEquippedRunes();

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

        if (UITooltipPanel.Instance != null)
        {
            UITooltipPanel.Instance.HideTooltip();
        }
    }

    #region Rune Inventory Logic
    private void GetRuneSlots()
    {
        if (runeContents == null)
            runeContents = transform.Find("main/Left-Content/Content-Items/Scroll View/Viewport/Content/RuneContents")?.gameObject;

        if (runeContents == null) return;

        runeSlots.Clear();
        for (int i = 0; i < runeContents.transform.childCount; i++)
        {
            ItemSlot slot = runeContents.transform.GetChild(i).GetComponent<ItemSlot>();
            if (slot != null)
            {
                runeSlots.Add(slot);
            }
        }
    }

    public void RefreshEquippedRunes()
    {
        GetRuneSlots();

        for (int i = 0; i < runeSlots.Count; i++)
        {
            if (runeSlots[i] != null)
            {
                runeSlots[i].DiscardItemFromSlot();
            }
        }

        if (CharacterManager.Instance == null) return;

        CharacterType deployedChar = CharacterManager.Instance.GetSelectedCharacter();
        CharacterRuneEquip runeBuild = CharacterManager.Instance.GetCharacterRuneBuild(deployedChar);

        if (runeBuild == null || runeBuild.equippedRuneIDs == null) return;

        for (int i = 0; i < runeSlots.Count && i < runeBuild.equippedRuneIDs.Length; i++)
        {
            string runeID = runeBuild.equippedRuneIDs[i];
            if (string.IsNullOrEmpty(runeID)) continue;

            RuneData runeData = null;
            if (RuneInventoryManager.Instance != null)
            {
                runeData = RuneInventoryManager.Instance.runes.Find(r => r.runeID == runeID);
            }

            if (runeData != null)
            {
                Sprite runeSprite = GetRuneSprite(runeData);
                string title = $"<color={GetRarityHexColor(runeData.runeRarity)}>{runeData.runeName.ToUpper()}</color>";
                string details = $"<b>Rarity:</b> {runeData.runeRarity} | <b>Element:</b> {runeData.runeColor}\n\n";

                if (runeData.affixes != null)
                {
                    for (int k = 0; k < runeData.affixes.Count; k++)
                    {
                        var affix = runeData.affixes[k];
                        string sign = affix.value >= 0 ? "+" : "";
                        details += $"- {affix.statType}: <color=#00FFCC>{sign}{affix.value:F1}</color>\n";
                    }
                }

                if (!string.IsNullOrEmpty(runeData.runeLore))
                {
                    details += $"\n<i>\"{runeData.runeLore}\"</i>";
                }

                // Gán trực tiếp dữ liệu Rune vào Slot
                runeSlots[i].SetRuneDirectly(runeSprite, title, details, i);
            }
        }
    }

    private Sprite GetRuneSprite(RuneData rune)
    {
        if (rune == null) return null;

        if (rune.affixes != null)
        {
            for (int i = 0; i < rune.affixes.Count; i++)
            {
                if (rune.affixes[i].statType == RuneStatType.AllStats)
                {
                    return originRuneSprite != null ? originRuneSprite : (runeSprites.Count > 0 ? runeSprites[runeSprites.Count - 1] : null);
                }
            }
        }

        int colorOffset = (rune.runeColor == RuneColor.Red) ? 0 : (rune.runeColor == RuneColor.Green) ? 1 : 2;
        int targetIndex = ((int)rune.runeRarity * 3) + colorOffset;

        if (runeSprites != null && targetIndex >= 0 && targetIndex < runeSprites.Count)
        {
            return runeSprites[targetIndex];
        }

        return null;
    }

    private string GetRarityHexColor(RuneRarity rarity)
    {
        switch (rarity)
        {
            case RuneRarity.Common: return "#FFFFFF";
            case RuneRarity.Rare: return "#3399FF";
            case RuneRarity.Epic: return "#B266FF";
            case RuneRarity.Legendary: return "#FF9900";
        }
        return "#FFFFFF";
    }
    #endregion

    #region Weapon Inventory Logic
    private void GetWeaponSlots()
    {
        if (weaponContents == null)
            weaponContents = transform.Find("main/Left-Content/Content-Items/Scroll View/Viewport/Content/WeaponContents")?.gameObject;

        if (weaponContents == null)
            return;

        weaponSlots.Clear();
        for (int i = 0; i < weaponContents.transform.childCount; i++)
        {
            ItemSlot slot = weaponContents.transform.GetChild(i).GetComponent<ItemSlot>();
            if (slot != null)
                weaponSlots.Add(slot);
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

    private void DisableSelectedImages()
    {
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    private void OnHideDiscardButton(object obj)
    {
        if (btn_Discard != null)
            btn_Discard.gameObject.SetActive(false);
    }

    private void OnShowDiscardButton(object obj)
    {
        if (btn_Discard != null && !btn_Discard.gameObject.activeSelf)
            btn_Discard.gameObject.SetActive(true);
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
    }

    private void OnUseClicked()
    {
        if (selectedSlot == null || !selectedSlot.HasItem)
        {
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
        if (GameManager.Instance != null && GameManager.Instance.MapType == MapType.Lobby)
        {
            UIManager.Instance.ChangeMenu(MenuType.DefaultLobbyInputMenu);
            LobbyHUDTopBar.Instance?.ShowFullHUD();
        }
        else
        {
            UIManager.Instance.ChangeMenu(MenuType.GameplayMenu);
        }
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