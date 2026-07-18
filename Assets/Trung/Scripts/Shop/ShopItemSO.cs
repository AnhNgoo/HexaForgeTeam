using UnityEngine;

[CreateAssetMenu(fileName = "NewShopItem", menuName = "Shop/Create Shop Item")]
public class ShopItemSO : ScriptableObject
{
    public string itemID;
    public string itemName;
    [TextArea] public string itemDescription;
    public int gemCost;
    public Sprite itemIcon;
    public int purchaseQuantity = 1;
}