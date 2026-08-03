using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class InventoryItemCardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI Fields")]
    [SerializeField] private TMP_Text txtItemName;
    [SerializeField] private TMP_Text txtQuantity;
    [SerializeField] private Image imgItemIcon;

    [Header("Visual Config (Optional Backup)")]
    [SerializeField] private Sprite defaultItemSprite;

    private InventoryItemData currentData;

    public void SetupCard(InventoryItemData data)
    {
        currentData = data;

        if (data == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        if (txtItemName != null)
        {
            txtItemName.SetTextSafe(data.itemName);
        }

        if (txtQuantity != null)
        {
            txtQuantity.SetTextSafe(data.quantity.ToString());
        }

        if (imgItemIcon != null)
        {
            Sprite itemIconSprite = null;

            if (InventoryItemDatabase.Instance != null)
            {
                itemIconSprite = InventoryItemDatabase.Instance.GetItemSprite(data.itemID);
            }

            if (itemIconSprite != null)
            {
                imgItemIcon.sprite = itemIconSprite;
            }
            else if (defaultItemSprite != null)
            {
                imgItemIcon.sprite = defaultItemSprite;
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentData == null || UITooltipPanel.Instance == null) return;

        ShopItemSO itemSO = InventoryItemDatabase.Instance != null ? InventoryItemDatabase.Instance.GetItemSO(currentData.itemID) : null;
        string desc = itemSO != null ? itemSO.itemDescription : "Special Item";

        UITooltipPanel.Instance.ShowTooltip(currentData.itemName, desc, imgItemIcon != null ? imgItemIcon.sprite : null);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (UITooltipPanel.Instance != null)
        {
            UITooltipPanel.Instance.HideTooltip();
        }
    }
}