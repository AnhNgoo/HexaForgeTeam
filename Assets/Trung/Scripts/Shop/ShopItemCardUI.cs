using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItemCardUI : LoadComponents
{
    [Header("UI Fields")]
    [SerializeField] private TMP_Text txtItemName;
    [SerializeField] private TMP_Text txtDescription;
    [SerializeField] private TMP_Text txtCost;
    [SerializeField] private TMP_Text txtOwned;
    [SerializeField] private Image imgIcon;
    [SerializeField] private Button buyButton;

    private ShopItemSO itemData;

    public void SetupCard(ShopItemSO data)
    {
        itemData = data;
        if (itemData == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);
        RefreshCardUI();
    }

    public void RefreshCardUI()
    {
        if (itemData == null) return;

        if (txtItemName != null) txtItemName.SetTextSafe(itemData.itemName);
        if (txtDescription != null) txtDescription.SetTextSafe(itemData.itemDescription);
        if (txtCost != null) txtCost.SetTextSafe($"{itemData.gemCost} Gems");
        if (imgIcon != null && itemData.itemIcon != null) imgIcon.sprite = itemData.itemIcon;

        if (txtOwned != null && InventoryItemManager.Instance != null)
        {
            int ownedQty = InventoryItemManager.Instance.GetItemQuantity(itemData.itemID);
            txtOwned.SetTextSafe($"Owned: {ownedQty}");
        }

        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(OnBuyClicked);

            if (GemManager.Instance != null)
            {
                buyButton.interactable = GemManager.Instance.GetCurrentGem() >= itemData.gemCost;
            }
        }
    }

    private void OnBuyClicked()
    {
        if (itemData == null || GemManager.Instance == null) return;

        if (GemManager.Instance.GetCurrentGem() < itemData.gemCost)
        {
            if (LobbyNotifyManager.Instance != null)
            {
                LobbyNotifyManager.Instance.ShowNotify("Not enough Crystals!", Color.red);
            }
            return;
        }

        if (GemManager.Instance.SpendGem(itemData.gemCost))
        {
            if (InventoryItemManager.Instance != null)
            {
                InventoryItemManager.Instance.AddItem(itemData.itemID, itemData.itemName, itemData.purchaseQuantity);
            }

            if (LobbyNotifyManager.Instance != null)
            {
                LobbyNotifyManager.Instance.ShowNotify($"Purchased {itemData.itemName} x{itemData.purchaseQuantity}!", Color.green);
            }

            RefreshCardUI();

            if (LobbyShopUI.Instance != null)
            {
                LobbyShopUI.Instance.RefreshShopUI();
            }
        }
    }

    protected override void LoadComponent()
    {
        if (txtItemName == null) txtItemName = transform.Find("TxtItemName")?.GetComponent<TMP_Text>();
        if (txtDescription == null) txtDescription = transform.Find("TxtDescription")?.GetComponent<TMP_Text>();
        if (txtCost == null) txtCost = transform.Find("TxtCost")?.GetComponent<TMP_Text>();
        if (txtOwned == null) txtOwned = transform.Find("TxtOwned")?.GetComponent<TMP_Text>();
        if (imgIcon == null) imgIcon = transform.Find("ImgIcon")?.GetComponent<Image>();
        if (buyButton == null) buyButton = transform.Find("BuyButton")?.GetComponent<Button>();
    }

    protected override void LoadComponentRuntime()
    {
    }
}