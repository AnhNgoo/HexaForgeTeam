using UnityEngine;

public class LostGoldPickup : MonoBehaviour
{
    [SerializeField] private KeyCode interactKey = KeyCode.F;

    private bool playerInRange = false;

    private LostGoldObject lostGoldObject;

    private void Awake()
    {
        lostGoldObject = GetComponent<LostGoldObject>();
    }

    private void Update()
    {
        if (!playerInRange)
            return;

        if (Input.GetKeyDown(interactKey))
        {
            lostGoldObject.CollectGold();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = true;

        Debug.Log("Có thể nhặt lại vàng");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = false;

        Debug.Log("Rời khỏi vùng nhặt vàng");
    }
}