using UnityEngine;
using Sirenix.OdinInspector;

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
    [PreviewField(150, ObjectFieldAlignment.Left)]
    [AssetsOnly]
    public Sprite itemIcon;
    [TextArea(3, 10)] public string itemDescription;
    public ItemRarity rarity = ItemRarity.Common;
}
