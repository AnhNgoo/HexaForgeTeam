using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveFire : MonoBehaviour
{
    [SerializeField] private GameObject firePrefabs;
    // Start is called before the first frame update
    private void Start()
    {
        firePrefabs.SetActive(false);
    }
    private void OnTriggerEnter(Collider player)
    {
        if (!player.CompareTag("Player")) return;

        firePrefabs.SetActive(true);
    }
}
