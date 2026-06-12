using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("Inventory Data")]
    public List<InventoryItem> items = new List<InventoryItem>();

    [Header("Setting")]
    public int maxItemAmount = 30;

    private void Awake()
    {
        Instance = this;
    }

    public bool AddItem(ItemData itemData, int amount)
    {
        if (itemData == null || amount <= 0) return false;

        int currentTotal = GetTotalItemAmount();

        if (currentTotal >= maxItemAmount)
        {
            return false;
        }

        int canAddAmount = Mathf.Min(amount, maxItemAmount - currentTotal);

        InventoryItem existingItem = items.Find(x => x.itemData == itemData);

        if (existingItem != null)
        {
            existingItem.amount += canAddAmount;
        }
        else
        {
            items.Add(new InventoryItem(itemData, canAddAmount));
        }

        return true;
    }

    public void UseItem(InventoryItem item)
    {
        if (item == null) return;

        item.amount--;

        if (item.amount <= 0)
        {
            items.Remove(item);
        }
    }

    public int GetTotalItemAmount()
    {
        int total = 0;

        foreach (InventoryItem item in items)
        {
            total += item.amount;
        }

        return total;
    }
}