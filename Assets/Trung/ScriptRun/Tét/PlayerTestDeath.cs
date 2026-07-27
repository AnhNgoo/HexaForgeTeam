using UnityEngine;

public class PlayerTestDeath : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private KeyCode deadKey = KeyCode.K;

    [Header("Respawn")]
    [SerializeField] private Transform respawnPoint;

    private PlayerLostGoldSpawner lostGoldSpawner;

    private void Awake()
    {
        lostGoldSpawner = GetComponent<PlayerLostGoldSpawner>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(deadKey))
        {
            Dead();
        }
    }

    private void Dead()
    {
        Debug.Log("Player đã chết");

        lostGoldSpawner.DropGold();

        Respawn();
    }

    private void Respawn()
    {
        if (respawnPoint == null)
            return;

        transform.position = respawnPoint.position;

        Debug.Log("Player hồi sinh");
    }
}