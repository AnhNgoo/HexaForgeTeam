using System;

[Serializable]
public class InventoryItemData
{
    public string itemID;
    public string itemName;
    public int quantity;

    public InventoryItemData(string id, string name, int qty)
    {
        itemID = id;
        itemName = name;
        quantity = qty;
    }
}