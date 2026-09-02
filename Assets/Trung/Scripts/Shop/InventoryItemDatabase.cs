using System.Collections.Generic;
using UnityEngine;

public class InventoryItemDatabase : MonoBehaviour
{
    public static InventoryItemDatabase Instance;

    [Header("All Items Database Configuration")]
    [SerializeField] private List<ShopItemSO> allGameItems = new List<ShopItemSO>();

    [Header("Currency Icons Default")]
    [SerializeField] private Sprite gemIconSprite;
    [SerializeField] private Sprite runeShardIconSprite;
    [SerializeField] private Sprite originRuneIconSprite;
    [SerializeField] private Sprite expIconSprite; // Thêm trường icon cho EXP

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

    public Sprite GetItemSprite(string itemID)
    {
        if (itemID == "GEM" || itemID == "CURRENCY_GEM")
        {
            return gemIconSprite;
        }

        if (itemID == "RUNE_SHARD" || itemID == "CURRENCY_SHARD")
        {
            return runeShardIconSprite;
        }

        if (itemID == "ORIGIN_RUNE")
        {
            return originRuneIconSprite;
        }

        // Bổ sung nhận diện EXP hiển thị trên CostDisplayUI
        if (itemID == "EXP" || itemID == "ACCOUNT_EXP")
        {
            return expIconSprite;
        }

        ShopItemSO itemSO = GetItemSO(itemID);
        if (itemSO != null)
        {
            return itemSO.itemIcon;
        }

        return null;
    }
}