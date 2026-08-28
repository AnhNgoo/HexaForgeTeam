using UnityEngine;

public enum ItemRarity
{
    Common,
    Uncommon,
    Rare,
    Legendary
}

[CreateAssetMenu(fileName = "New Item Data", menuName = "ScriptableObjects/ItemData", order = 1)]
public class ItemDataBase : ScriptableObject
{
    public string itemName;
    public Sprite itemIcon;
    [TextArea(3, 10)] public string itemDescription;
    public ItemRarity rarity = ItemRarity.Common;
}
