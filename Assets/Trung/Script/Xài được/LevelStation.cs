using UnityEngine;

public class LevelStation : MonoBehaviour
{
    [SerializeField] private LevelUpMenu levelUpMenu;

    [Header("Debug")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    private bool playerInRange = false;

    private void Update()
    {
        if (!playerInRange)
            return;

        if (Input.GetKeyDown(interactKey))
        {
            levelUpMenu.Open();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = true;

        Debug.Log("Player đã vào vùng Level Station");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = false;

        Debug.Log("Player đã rời vùng Level Station");
    }
}