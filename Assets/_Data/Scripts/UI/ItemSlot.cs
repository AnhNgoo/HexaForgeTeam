using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemSLotData
{
    public ItemDataBase itemData;
    public int index;
}

public class ItemSlot : LoadComponents
{
    [SerializeField] private ItemDataBase itemData;
    public ItemDataBase ItemData => itemData;
    [SerializeField] private Image itemImage;
    [SerializeField] private GameObject selectedImage;
    public bool isEmpty { get; private set; } = true;
    public int Index { get; set; } = -1;
    protected override void LoadComponent()
    {
        if (itemImage == null)
            itemImage = transform.Find("Icon")?.GetComponent<Image>();
        if (selectedImage == null)
            selectedImage = transform.Find("Selected")?.gameObject;
    }

    protected override void LoadComponentRuntime()
    {

    }

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
        if (itemSLotData == null) return;

        itemData = itemSLotData.itemData;
        Index = itemSLotData.index;
        isEmpty = false;
        itemImage.gameObject.SetActive(true);
        itemImage.sprite = itemSLotData.itemData.itemIcon;
    }

    public void DiscardItemFromSlot()
    {
        itemData = null;
        isEmpty = true;
        itemImage.gameObject.SetActive(false);
        itemImage.sprite = null;
        selectedImage.SetActive(false);
        EventManager.Notify(GameEvent.OnDeselectItemInInventory);
    }

    // Khi chọn sẽ gửi index hiện tại của slot và thông báo cho InventoryMenu biết để hiển thị nút Discard
    public void SelectItem()
    {
        if (itemData == null) return;
        EventManager.Notify(GameEvent.OnSelectItemInInventory, Index);
    }

    //  Khi bỏ chọn sẽ gửi thông báo cho InventoryMenu biết để ẩn nút Discard
    public void DeselectItem()
    {
        EventManager.Notify(GameEvent.OnDeselectItemInInventory);
    }

    public void DisableSelectedImage()
    {
        if (selectedImage != null)
            selectedImage.SetActive(false);
    }
}
