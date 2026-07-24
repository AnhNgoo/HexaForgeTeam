using UnityEngine;

public enum ShopCurrencyType
{
    Gem,
    RuneShard
}

[CreateAssetMenu(fileName = "NewShopItem", menuName = "Shop/Create Shop Item")]
public class ShopItemSO : ScriptableObject
{
    public string itemID;
    public string itemName;
    [TextArea] public string itemDescription;
    
    [Header("Price Configuration")]
    public ShopCurrencyType currencyType = ShopCurrencyType.Gem;
    public int costAmount = 100;

    [Header("Currency Exchange Feature")]
    public bool isCurrencyExchange = false; // Bật cờ này nếu đây là gói đổi tiền trực tiếp
    public ShopCurrencyType rewardCurrencyType = ShopCurrencyType.RuneShard; // Loại tiền nhận về

    [Header("Custom Rune Pack Feature")]
    public bool isCustomRunePack = false; // Bật cờ này nếu đây là gói bán Ngọc chọn Độ hiếm/Màu

    public Sprite itemIcon;
    public int purchaseQuantity = 1; // Số lượng nhận về (ví dụ: 100 Shards hoặc 50 Gems)
}