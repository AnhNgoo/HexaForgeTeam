using UnityEngine;

public class ItemTest : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private string itemName = "Gold";

    public void OnInteract()
    {
        Debug.Log($"Đã tương tác với : {itemName}");

        Destroy(gameObject);
    }
}