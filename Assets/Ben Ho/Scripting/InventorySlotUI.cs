using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlotUI : MonoBehaviour
{
    [Header("Item Data")]
    public string itemName;
    public Sprite itemIcon;
    public int amount = 1;

    [Header("UI")]
    [SerializeField] private Button btn_Slot;
    [SerializeField] private Image img_Icon;
    [SerializeField] private TextMeshProUGUI txt_Amount;

    private InventoryMenu inventoryMenu;

    public bool HasItem => itemIcon != null && amount > 0;

    public void Init(InventoryMenu menu)
    {
        inventoryMenu = menu;

        if (btn_Slot != null)
        {
            btn_Slot.onClick.RemoveListener(OnClickSlot);
            btn_Slot.onClick.AddListener(OnClickSlot);
        }

        RefreshUI();
    }

    public void RemoveListener()
    {
        if (btn_Slot != null)
            btn_Slot.onClick.RemoveListener(OnClickSlot);
    }

    private void OnClickSlot()
    {
        if (!HasItem) return;

        inventoryMenu.SelectItem(this);
    }

    public void Use()
    {
        if (!HasItem) return;

        amount--;
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (img_Icon != null)
        {
            img_Icon.sprite = itemIcon;
            img_Icon.enabled = HasItem;
        }

        if (txt_Amount != null)
            txt_Amount.text = HasItem ? amount.ToString() : "";
    }

    public Sprite GetIcon()
    {
        return itemIcon;
    }

    public int GetAmount()
    {
        return amount;
    }
}