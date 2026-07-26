using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryItemCardUI : MonoBehaviour
{
    [Header("UI Fields")]
    [SerializeField] private TMP_Text txtItemName;
    [SerializeField] private TMP_Text txtQuantity;
    [SerializeField] private Image imgItemIcon;

    [Header("Visual Config (Optional Backup)")]
    [SerializeField] private Sprite defaultItemSprite;

    public void SetupCard(InventoryItemData data)
    {
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
}