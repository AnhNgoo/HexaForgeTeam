using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Item Data")]
    [SerializeField] private ItemData itemData;

    [Header("Amount")]
    [SerializeField] private int amount = 1;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("Không tìm thấy InventoryManager trong scene.");
            return;
        }

        bool added = InventoryManager.Instance.AddItem(itemData, amount);

        if (added)
        {
            Debug.Log("Picked up: " + itemData.itemName + " x" + amount);
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("Inventory đã đầy, không thể nhặt item.");
        }
    }
}