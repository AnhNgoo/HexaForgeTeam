using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class ShopQuantityPopupUI : MonoBehaviour
{
    public static ShopQuantityPopupUI Instance;

    [Header("Panel Root & Background Overlay")]
    [SerializeField] private GameObject popupRoot;
    [SerializeField] private RectTransform popupContainer; // Khung chứa chính của Popup
    [SerializeField] private CanvasGroup bgOverlayCanvasGroup; // Lớp nền mờ phía sau

    [Header("Item Info UI")]
    [SerializeField] private TMP_Text txtItemName;
    [SerializeField] private Image imgItemIcon;

    [Header("Quantity Controls")]
    [SerializeField] private TMP_Text txtQuantity;
    [SerializeField] private Button btnIncrease;
    [SerializeField] private Button btnDecrease;
    [SerializeField] private Button btnMax;

    [Header("Total Cost UI")]
    [SerializeField] private CostDisplayUI costDisplayUI;

    [Header("Action Buttons")]
    [SerializeField] private Button btnConfirmBuy;
    [SerializeField] private Button btnClose;

    private ShopItemSO currentItemData;
    private int currentQuantity = 1;
    private int maxBuyableQuantity = 1;
    private bool isAnimating = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        if (popupRoot != null)
        {
            popupRoot.SetActive(false);
        }
    }

    private void Start()
    {
        if (btnIncrease != null) btnIncrease.onClick.AddListener(OnIncreaseClicked);
        if (btnDecrease != null) btnDecrease.onClick.AddListener(OnDecreaseClicked);
        if (btnMax != null) btnMax.onClick.AddListener(OnMaxClicked);
        if (btnConfirmBuy != null) btnConfirmBuy.onClick.AddListener(OnConfirmBuyClicked);
        if (btnClose != null) btnClose.onClick.AddListener(HidePopup);
    }

    public void OpenPopup(ShopItemSO itemData)
    {
        if (itemData == null || popupRoot == null || isAnimating) return;

        currentItemData = itemData;
        currentQuantity = 1;

        CalculateMaxBuyable();

        if (txtItemName != null) txtItemName.SetTextSafe(itemData.itemName);
        if (imgItemIcon != null && itemData.itemIcon != null) imgItemIcon.sprite = itemData.itemIcon;

        popupRoot.SetActive(true);
        RefreshPopupUI();

        // HIỆU ỨNG MỞ POPUP (DOTWEEN)
        isAnimating = true;

        if (bgOverlayCanvasGroup != null)
        {
            bgOverlayCanvasGroup.alpha = 0f;
            bgOverlayCanvasGroup.DOFade(1f, 0.25f).SetUpdate(true);
        }

        if (popupContainer != null)
        {
            popupContainer.transform.localScale = Vector3.one * 0.7f;
            popupContainer.transform.DOScale(Vector3.one, 0.3f)
                .SetEase(Ease.OutBack)
                .SetUpdate(true)
                .OnComplete(() => isAnimating = false);
        }
        else
        {
            isAnimating = false;
        }
    }

    public void HidePopup()
    {
        if (popupRoot == null || !popupRoot.activeSelf || isAnimating) return;

        isAnimating = true;

        // HIỆU ỨNG ĐÓNG POPUP (DOTWEEN)
        if (bgOverlayCanvasGroup != null)
        {
            bgOverlayCanvasGroup.DOFade(0f, 0.2f).SetUpdate(true);
        }

        if (popupContainer != null)
        {
            popupContainer.transform.DOScale(Vector3.zero, 0.2f)
                .SetEase(Ease.InBack)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    popupRoot.SetActive(false);
                    currentItemData = null;
                    isAnimating = false;
                });
        }
        else
        {
            popupRoot.SetActive(false);
            currentItemData = null;
            isAnimating = false;
        }
    }

    private void CalculateMaxBuyable()
    {
        if (currentItemData == null)
        {
            maxBuyableQuantity = 1;
            return;
        }

        int availableCurrency = 0;
        if (currentItemData.currencyType == ShopCurrencyType.Gem && GemManager.Instance != null)
        {
            availableCurrency = GemManager.Instance.GetCurrentGem();
        }
        else if (currentItemData.currencyType == ShopCurrencyType.RuneShard && RuneShardManager.Instance != null)
        {
            availableCurrency = RuneShardManager.Instance.GetCurrentShards();
        }

        if (currentItemData.costAmount <= 0)
        {
            maxBuyableQuantity = 99;
        }
        else
        {
            maxBuyableQuantity = Mathf.Max(1, availableCurrency / currentItemData.costAmount);
        }
    }

    private void RefreshPopupUI()
    {
        if (currentItemData == null) return;

        currentQuantity = Mathf.Clamp(currentQuantity, 1, maxBuyableQuantity);

        if (txtQuantity != null)
        {
            txtQuantity.SetTextSafe(currentQuantity.ToString());
        }

        if (costDisplayUI != null)
        {
            string currencyID = currentItemData.currencyType == ShopCurrencyType.Gem ? "GEM" : "RUNE_SHARD";
            int totalCost = currentItemData.costAmount * currentQuantity;
            costDisplayUI.SetupCost(new List<CostData> { new CostData(currencyID, totalCost) });
        }

        if (btnDecrease != null) btnDecrease.interactable = (currentQuantity > 1);
        if (btnIncrease != null) btnIncrease.interactable = (currentQuantity < maxBuyableQuantity);

        if (btnConfirmBuy != null)
        {
            int totalCost = currentItemData.costAmount * currentQuantity;
            bool canAfford = false;

            if (currentItemData.currencyType == ShopCurrencyType.Gem && GemManager.Instance != null)
            {
                canAfford = GemManager.Instance.GetCurrentGem() >= totalCost;
            }
            else if (currentItemData.currencyType == ShopCurrencyType.RuneShard && RuneShardManager.Instance != null)
            {
                canAfford = RuneShardManager.Instance.GetCurrentShards() >= totalCost;
            }

            btnConfirmBuy.interactable = canAfford;
        }
    }

    private void OnIncreaseClicked()
    {
        if (currentQuantity < maxBuyableQuantity && !isAnimating)
        {
            currentQuantity++;
            AnimateQuantityText();
            RefreshPopupUI();
        }
    }

    private void OnDecreaseClicked()
    {
        if (currentQuantity > 1 && !isAnimating)
        {
            currentQuantity--;
            AnimateQuantityText();
            RefreshPopupUI();
        }
    }

    private void OnMaxClicked()
    {
        if (!isAnimating)
        {
            currentQuantity = maxBuyableQuantity;
            AnimateQuantityText();
            RefreshPopupUI();
        }
    }

    private void AnimateQuantityText()
    {
        if (txtQuantity != null)
        {
            txtQuantity.transform.DOKill(true);
            txtQuantity.transform.localScale = Vector3.one;
            txtQuantity.transform.DOPunchScale(new Vector3(0.25f, 0.25f, 0.25f), 0.18f, 8, 1f).SetUpdate(true);
        }
    }

    private void OnConfirmBuyClicked()
    {
        if (currentItemData == null || isAnimating) return;

        int totalCost = currentItemData.costAmount * currentQuantity;
        int totalRewardQuantity = currentItemData.purchaseQuantity * currentQuantity;
        bool purchaseSuccess = false;

        if (currentItemData.currencyType == ShopCurrencyType.Gem)
        {
            if (GemManager.Instance == null || GemManager.Instance.GetCurrentGem() < totalCost)
            {
                if (LobbyNotifyManager.Instance != null)
                {
                    LobbyNotifyManager.Instance.ShowNotify("Not enough Gems!", Color.red);
                }
                return;
            }

            purchaseSuccess = GemManager.Instance.SpendGem(totalCost);
        }
        else if (currentItemData.currencyType == ShopCurrencyType.RuneShard)
        {
            if (RuneShardManager.Instance == null || RuneShardManager.Instance.GetCurrentShards() < totalCost)
            {
                if (LobbyNotifyManager.Instance != null)
                {
                    LobbyNotifyManager.Instance.ShowNotify("Not enough Rune Shards!", Color.red);
                }
                return;
            }

            purchaseSuccess = RuneShardManager.Instance.SpendShards(totalCost);
        }

        if (purchaseSuccess)
        {
            // Hiệu ứng nhún nhẹ Popup khi mua thành công
            if (popupContainer != null)
            {
                popupContainer.DOPunchScale(new Vector3(0.08f, 0.08f, 0.08f), 0.15f).SetUpdate(true);
            }

            if (currentItemData.isCurrencyExchange)
            {
                if (currentItemData.rewardCurrencyType == ShopCurrencyType.RuneShard && RuneShardManager.Instance != null)
                {
                    RuneShardManager.Instance.AddShards(totalRewardQuantity);
                }
                else if (currentItemData.rewardCurrencyType == ShopCurrencyType.Gem && GemManager.Instance != null)
                {
                    GemManager.Instance.AddGem(totalRewardQuantity);
                }

                if (LobbyNotifyManager.Instance != null)
                {
                    LobbyNotifyManager.Instance.ShowNotify($"Exchanged! Received +{totalRewardQuantity} {currentItemData.rewardCurrencyType}s!", Color.green);
                }
            }
            else
            {
                if (InventoryItemManager.Instance != null)
                {
                    InventoryItemManager.Instance.AddItem(currentItemData.itemID, currentItemData.itemName, totalRewardQuantity);
                }

                if (LobbyNotifyManager.Instance != null)
                {
                    LobbyNotifyManager.Instance.ShowNotify($"Purchased {currentItemData.itemName} x{totalRewardQuantity}!", Color.green);
                }
            }

            HidePopup();

            if (LobbyShopUI.Instance != null)
            {
                LobbyShopUI.Instance.RefreshShopUI();
            }
        }
    }
    public bool IsPopupActive()
    {
        return popupRoot != null && popupRoot.activeInHierarchy;
    }
}