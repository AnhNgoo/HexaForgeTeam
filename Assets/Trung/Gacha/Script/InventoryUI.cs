using UnityEngine;

public class InventoryUI :
    MonoBehaviour
{
    public static InventoryUI Instance;
    [Header("Panel")]
    [SerializeField] private GameObject inventoryPanel;

    [Header("UI")]
    [SerializeField] private Transform contentParent;

    [Header("Card")]
    [SerializeField] private RuneCardUI cardPrefab;

private void Awake()
{
    if (Instance == null)
    {
        Instance = this;
    }
}
    private void Start()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }
    }

    private void OnEnable()
    {
        RefreshInventory();
    }

    public void OpenInventory()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(true);
        }

        RefreshInventory();
    }

    public void CloseInventory()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }
    }

    public void RefreshInventory()
    {
        ClearInventory();

        if (RuneInventory.Instance == null)
        {
            return;
        }

        for (int i = 0;
            i < RuneInventory.Instance.runes.Count;
            i++)
        {
            SpawnCard(
                RuneInventory.Instance
                .runes[i]);
        }
    }

    private void SpawnCard(
        RuneData runeData)
    {
        RuneCardUI card =
            Instantiate(
                cardPrefab,
                contentParent);

        card.Setup(
            runeData,
            false);
    }

    private void ClearInventory()
    {
        for (int i =
            contentParent.childCount - 1;
            i >= 0;
            i--)
        {
            Destroy(
                contentParent
                .GetChild(i)
                .gameObject);
        }
    }
}