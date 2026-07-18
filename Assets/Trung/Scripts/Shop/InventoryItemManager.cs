using System.Collections.Generic;
using UnityEngine;

public class InventoryItemManager : MonoBehaviour
{
    public static InventoryItemManager Instance;

    private List<InventoryItemData> items = new List<InventoryItemData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        LoadInventory();
    }

    public void AddItem(string itemID, string itemName, int amount)
    {
        InventoryItemData existingItem = items.Find(x => x.itemID == itemID);

        if (existingItem != null)
        {
            existingItem.quantity += amount;
        }
        else
        {
            items.Add(new InventoryItemData(itemID, itemName, amount));
        }

        SaveInventory();
    }

    public int GetItemQuantity(string itemID)
    {
        InventoryItemData item = items.Find(x => x.itemID == itemID);
        return item != null ? item.quantity : 0;
    }

    public bool SpendItem(string itemID, int amount)
    {
        InventoryItemData existingItem = items.Find(x => x.itemID == itemID);

        if (existingItem == null || existingItem.quantity < amount)
        {
            return false;
        }

        existingItem.quantity -= amount;
        SaveInventory();
        return true;
    }

    private void LoadInventory()
    {
        if (SaveLoadManager.Instance != null && SaveLoadManager.Instance.SaveData != null)
        {
            items = SaveLoadManager.Instance.SaveData.inventoryItems;
            if (items == null)
            {
                items = new List<InventoryItemData>();
            }
        }
    }

    public void SaveInventory()
    {
        if (SaveLoadManager.Instance != null && SaveLoadManager.Instance.SaveData != null)
        {
            SaveLoadManager.Instance.SaveData.inventoryItems = items;
            SaveLoadManager.Instance.SaveGame();

            if (PlayFabDataManager.Instance != null)
            {
                PlayFabDataManager.Instance.MarkDirty();
            }
        }
    }
}