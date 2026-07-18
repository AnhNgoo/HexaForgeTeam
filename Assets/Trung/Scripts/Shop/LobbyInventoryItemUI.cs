using System.Collections.Generic;
using UnityEngine;

public class LobbyInventoryItemUI : MonoBehaviour
{
    public static LobbyInventoryItemUI Instance;

    [Header("Panel Root")]
    [SerializeField] private GameObject itemInventoryPanelRoot;

    [Header("Grid Layout Container")]
    [SerializeField] private Transform contentParent;

    [Header("Prefab Template")]
    [SerializeField] private InventoryItemCardUI itemCardPrefab;

    private List<GameObject> activeCards = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void OnEnable()
    {
        RefreshItemInventory();
    }

    public void OpenPanel()
    {
        if (itemInventoryPanelRoot != null)
        {
            itemInventoryPanelRoot.SetActive(true);
        }

        RefreshItemInventory();
    }

    public void ClosePanel()
    {
        if (itemInventoryPanelRoot != null)
        {
            itemInventoryPanelRoot.SetActive(false);
        }
    }

    public void RefreshItemInventory()
    {
        ClearGrid();

        if (SaveLoadManager.Instance == null || SaveLoadManager.Instance.SaveData == null)
        {
            return;
        }

        List<InventoryItemData> savedItems = SaveLoadManager.Instance.SaveData.inventoryItems;
        if (savedItems == null || savedItems.Count == 0)
        {
            return;
        }

        for (int i = 0; i < savedItems.Count; i++)
        {
            if (savedItems[i].quantity <= 0)
            {
                continue;
            }

            InventoryItemCardUI card = Instantiate(itemCardPrefab, contentParent);
            card.SetupCard(savedItems[i]);
            activeCards.Add(card.gameObject);
        }
    }

    private void ClearGrid()
    {
        for (int i = activeCards.Count - 1; i >= 0; i--)
        {
            if (activeCards[i] != null)
            {
                Destroy(activeCards[i]);
            }
        }
        activeCards.Clear();

        if (contentParent != null)
        {
            for (int i = contentParent.childCount - 1; i >= 0; i--)
            {
                Destroy(contentParent.GetChild(i).gameObject);
            }
        }
    }
}