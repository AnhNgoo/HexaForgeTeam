using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StoreMenu : MenuBase
{
    public override MenuType menuType => MenuType.StoreMenu;

    [Header("Buttons")]
    [SerializeField] private Button btn_Back;

    [Header("Display Panel")]
    [SerializeField] private Image img_ProductIcon;
    [SerializeField] private TextMeshProUGUI txt_ProductName;
    [SerializeField] private TextMeshProUGUI txt_Description;
    [SerializeField] private TextMeshProUGUI txt_Price;
    [SerializeField] private TextMeshProUGUI txt_CoinInfo;

    [Header("Player Coin")]
    [SerializeField] private TextMeshProUGUI txt_PlayerCoin;
    [SerializeField] private int playerCoin = 0;

    [Header("Store Items")]
    [SerializeField] private List<StoreItemUI> storeItems = new List<StoreItemUI>();

    protected override void LoadComponent()
    {

    }

    protected override void LoadComponentRuntime()
    {

    }

    public override void Open(object data = null)
    {
        base.Open(data);

        if (btn_Back != null)
        {
            btn_Back.onClick.RemoveListener(OnBackButtonClicked);
            btn_Back.onClick.AddListener(OnBackButtonClicked);
        }

        for (int i = 0; i < storeItems.Count; i++)
        {
            if (storeItems[i] != null)
                storeItems[i].Init(this);
        }

        if (storeItems.Count > 0 && storeItems[0] != null)
            ShowItemInfo(storeItems[0]);

        UpdatePlayerCoinUI();
    }

    public override void Close()
    {
        if (btn_Back != null)
            btn_Back.onClick.RemoveListener(OnBackButtonClicked);

        for (int i = 0; i < storeItems.Count; i++)
        {
            if (storeItems[i] != null)
                storeItems[i].RemoveListener();
        }

        base.Close();
    }

    public void BuyItem(StoreItemUI item)
    {
        if (item == null) return;

        ShowItemInfo(item);

        int coinReceive = item.GetTotalCoin();
        playerCoin += coinReceive;

        UpdatePlayerCoinUI();

        Debug.Log("Bought: " + item.productName + " | +" + coinReceive + " coins");
    }

    private void ShowItemInfo(StoreItemUI item)
    {
        if (item == null) return;

        if (img_ProductIcon != null)
        {
            img_ProductIcon.sprite = item.productIcon;
            img_ProductIcon.enabled = item.productIcon != null;
        }

        if (txt_ProductName != null)
            txt_ProductName.text = item.productName;

        if (txt_Description != null)
            txt_Description.text = item.description;

        if (txt_Price != null)
            txt_Price.text = item.priceText;

        if (txt_CoinInfo != null)
        {
            if (item.bonusCoin > 0)
                txt_CoinInfo.text = item.coinAmount + " coins + " + item.bonusCoin + " bonus coins";
            else
                txt_CoinInfo.text = item.coinAmount + " coins";
        }
    }

    private void OnBackButtonClicked()
    {
        Debug.Log("Store Back button clicked");

        UIManager.Instance.ChangeMenu(MenuType.GameplayMenu);
    }

    private void UpdatePlayerCoinUI()
    {
        if (txt_PlayerCoin != null)
            txt_PlayerCoin.text = playerCoin.ToString();
    }
}