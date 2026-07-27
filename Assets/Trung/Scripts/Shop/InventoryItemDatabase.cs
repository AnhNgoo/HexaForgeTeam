using System.Collections.Generic;
using UnityEngine;

public class InventoryItemDatabase : MonoBehaviour
{
    public static InventoryItemDatabase Instance;

    [Header("All Items Database Configuration")]
    [SerializeField] private List<ShopItemSO> allGameItems = new List<ShopItemSO>();

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
        }
    }

    public ShopItemSO GetItemSO(string itemID)
    {
        if (allGameItems == null) return null;

        for (int i = 0; i < allGameItems.Count; i++)
        {
            if (allGameItems[i] != null && allGameItems[i].itemID == itemID)
            {
                return allGameItems[i];
            }
        }
        return null;
    }
}