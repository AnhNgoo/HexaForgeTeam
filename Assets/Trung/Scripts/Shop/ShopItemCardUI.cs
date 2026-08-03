using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.EventSystems;

public class ShopItemCardUI : LoadComponents, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI Fields")]
    [SerializeField] private TMP_Text txtItemName;
    [SerializeField] private TMP_Text txtDescription;
    [SerializeField] private CostDisplayUI costDisplayUI;
    [SerializeField] private TMP_Text txtOwned;
    [SerializeField] private Image imgIcon;
    [SerializeField] private Button buyButton;

    private ShopItemSO itemData;
    private Vector3 originalScale = Vector3.one;

    protected override void Awake()
    {
        base.Awake();
        originalScale = transform.localScale;
    }

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
        if (imgIcon != null && itemData.itemIcon != null) imgIcon.sprite = itemData.itemIcon;

        if (costDisplayUI != null)
        {
            if (itemData.isCustomRunePack)
            {
                costDisplayUI.gameObject.SetActive(false);
            }
            else
            {
                costDisplayUI.gameObject.SetActive(true);
                string currencyID = itemData.currencyType == ShopCurrencyType.Gem ? "GEM" : "RUNE_SHARD";
                List<CostData> costs = new List<CostData> { new CostData(currencyID, itemData.costAmount) };
                costDisplayUI.SetupCost(costs);
            }
        }

        if (txtOwned != null)
        {
            if (itemData.isCurrencyExchange)
            {
                txtOwned.gameObject.SetActive(false);
            }
            else if (InventoryItemManager.Instance != null)
            {
                txtOwned.gameObject.SetActive(true);
                int ownedQty = InventoryItemManager.Instance.GetItemQuantity(itemData.itemID);
                txtOwned.SetTextSafe($"Owned: {ownedQty}");
            }
        }

        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(OnBuyClicked);

            bool canAfford = false;
            if (itemData.currencyType == ShopCurrencyType.Gem && GemManager.Instance != null)
            {
                canAfford = GemManager.Instance.GetCurrentGem() >= itemData.costAmount;
            }
            else if (itemData.currencyType == ShopCurrencyType.RuneShard && RuneShardManager.Instance != null)
            {
                canAfford = RuneShardManager.Instance.GetCurrentShards() >= itemData.costAmount;
            }

            buyButton.interactable = canAfford;
        }
    }

    private void OnBuyClicked()
    {
        if (itemData == null) return;

        transform.DOKill();
        transform.DOPunchScale(new Vector3(0.08f, 0.08f, 0f), 0.2f, 8, 0.5f).SetUpdate(true);

        if (itemData.isCustomRunePack)
        {
            if (ShopRuneSelectionPopupUI.Instance != null)
            {
                ShopRuneSelectionPopupUI.Instance.OpenPopup();
            }
        }
        else if (ShopQuantityPopupUI.Instance != null)
        {
            ShopQuantityPopupUI.Instance.OpenPopup(itemData);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.DOKill();
        transform.DOScale(originalScale * 1.05f, 0.2f).SetEase(Ease.OutQuad).SetUpdate(true);

        if (itemData != null && UITooltipPanel.Instance != null)
        {
            UITooltipPanel.Instance.ShowTooltip(itemData.itemName, itemData.itemDescription, imgIcon != null ? imgIcon.sprite : null);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOKill();
        transform.DOScale(originalScale, 0.2f).SetEase(Ease.InQuad).SetUpdate(true);

        if (UITooltipPanel.Instance != null)
        {
            UITooltipPanel.Instance.HideTooltip();
        }
    }

    protected override void LoadComponent()
    {
        if (txtItemName == null) txtItemName = transform.Find("TxtItemName")?.GetComponent<TMP_Text>();
        if (txtDescription == null) txtDescription = transform.Find("TxtDescription")?.GetComponent<TMP_Text>();
        if (costDisplayUI == null) costDisplayUI = transform.Find("CostDisplayUI")?.GetComponent<CostDisplayUI>();
        if (txtOwned == null) txtOwned = transform.Find("TxtOwned")?.GetComponent<TMP_Text>();
        if (imgIcon == null) imgIcon = transform.Find("ImgIcon")?.GetComponent<Image>();
        if (buyButton == null) buyButton = transform.Find("BuyButton")?.GetComponent<Button>();
    }

    protected override void LoadComponentRuntime()
    {
    }
}