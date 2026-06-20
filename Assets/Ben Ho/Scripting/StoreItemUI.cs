using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StoreItemUI : MonoBehaviour
{
    [Header("Product Data")]
    public string productName;
    public string description;
    public Sprite productIcon;
    public int coinAmount;
    public int bonusCoin;
    public string priceText;

    [Header("UI")]
    [SerializeField] private Button btn_Buy;
    [SerializeField] private Image img_Icon;
    [SerializeField] private TextMeshProUGUI txt_Name;
    [SerializeField] private TextMeshProUGUI txt_Coin;
    [SerializeField] private TextMeshProUGUI txt_Price;

    private StoreMenu storeMenu;

    public void Init(StoreMenu menu)
    {
        storeMenu = menu;

        RefreshUI();

        if (btn_Buy != null)
        {
            btn_Buy.onClick.RemoveListener(OnBuyButtonClicked);
            btn_Buy.onClick.AddListener(OnBuyButtonClicked);
        }
    }

    public void RemoveListener()
    {
        if (btn_Buy != null)
            btn_Buy.onClick.RemoveListener(OnBuyButtonClicked);
    }

    private void OnBuyButtonClicked()
    {
        if (storeMenu == null) return;

        storeMenu.BuyItem(this);
    }

    private void RefreshUI()
    {
        if (img_Icon != null)
        {
            img_Icon.sprite = productIcon;
            img_Icon.enabled = productIcon != null;
        }

        if (txt_Name != null)
            txt_Name.text = productName;

        if (txt_Coin != null)
            txt_Coin.text = coinAmount.ToString();

        if (txt_Price != null)
            txt_Price.text = priceText;
    }

    public int GetTotalCoin()
    {
        return coinAmount + bonusCoin;
    }
}