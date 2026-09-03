using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ItemSLotData
{
    public ItemDataBase itemData;
    public int index;
}

public class ItemSlot : LoadComponents, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private ItemDataBase itemData;
    public ItemDataBase ItemData => itemData;

    [SerializeField] private Image itemImage;
    [SerializeField] private GameObject selectedImage;

    // Biến lưu trữ thông tin nếu slot này chứa Rune
    private string customTooltipTitle = "";
    private string customTooltipDetails = "";
    private Sprite customTooltipIcon = null;
    private bool isRuneSlot = false;

    public bool isEmpty { get; private set; } = true;
    public int Index { get; set; } = -1;

    protected override void LoadComponent()
    {
        if (itemImage == null)
            itemImage = transform.Find("Icon")?.GetComponent<Image>();
        if (selectedImage == null)
            selectedImage = transform.Find("Selected")?.gameObject;
    }

    protected override void LoadComponentRuntime() { }

    private void Start()
    {
        if (selectedImage != null)
            selectedImage.SetActive(false);
    }

    private void OnEnable()
    {
        DeselectItem();
        DisableSelectedImage();
    }

    public void AddItemIntoSlot(ItemSLotData itemSLotData)
    {
        if (itemSLotData == null || itemSLotData.itemData == null) return;

        isRuneSlot = false;
        itemData = itemSLotData.itemData;
        Index = itemSLotData.index;
        isEmpty = false;

        if (itemImage != null)
        {
            itemImage.gameObject.SetActive(true);
            itemImage.sprite = itemSLotData.itemData.itemIcon;
            itemImage.color = Color.white;
        }
    }

    public void SetRuneDirectly(Sprite runeSprite, string title, string details, int index)
    {
        isRuneSlot = true;
        Index = index;
        isEmpty = false;
        customTooltipTitle = title;
        customTooltipDetails = details;
        customTooltipIcon = runeSprite;

        if (itemImage != null)
        {
            itemImage.gameObject.SetActive(true);
            itemImage.sprite = runeSprite;
            itemImage.color = Color.white;
        }
    }

    public void DiscardItemFromSlot()
    {
        itemData = null;
        isEmpty = true;
        isRuneSlot = false;
        customTooltipTitle = "";
        customTooltipDetails = "";
        customTooltipIcon = null;

        if (itemImage != null)
        {
            itemImage.gameObject.SetActive(false);
            itemImage.sprite = null;
        }

        if (selectedImage != null)
            selectedImage.SetActive(false);

        EventManager.Notify(GameEvent.OnDeselectItemInInventory);
    }

    public void SelectItem()
    {
        if (itemData == null && !isRuneSlot) return;
        EventManager.Notify(GameEvent.OnSelectItemInInventory, Index);
    }

    public void DeselectItem()
    {
        EventManager.Notify(GameEvent.OnDeselectItemInInventory);
    }

    public void DisableSelectedImage()
    {
        if (selectedImage != null)
            selectedImage.SetActive(false);
    }

    #region Tooltip Pointer Events
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (UITooltipPanel.Instance == null || isEmpty) return;

        if (isRuneSlot)
        {
            UITooltipPanel.Instance.ShowTooltip(customTooltipTitle, customTooltipDetails, customTooltipIcon);
        }
        else if (itemData != null) //  Vũ khí
        {
            string rarity = $"<color={GetRarityHexColor(itemData.rarity)}>{itemData.rarity}</color>";
            string title = rarity + " - " + itemData.itemName;
            UITooltipPanel.Instance.ShowTooltip(title, itemData.itemDescription, itemData.itemIcon);
        }
    }

    private string GetRarityHexColor(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common: return "#FFFFFF";
            case ItemRarity.Uncommon: return "#dec714";
            case ItemRarity.Rare: return "#ff5555";
            case ItemRarity.Legendary: return "#bf00ff";
            default: return "#FFFFFF";
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (UITooltipPanel.Instance != null)
        {
            UITooltipPanel.Instance.HideTooltip();
        }
    }

    private void OnDisable()
    {
        if (UITooltipPanel.Instance != null)
        {
            UITooltipPanel.Instance.HideTooltip();
        }
    }
    #endregion
}