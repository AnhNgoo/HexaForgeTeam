using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveFireBossPlace : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject[] fireBossPlace;

    [Header("Settings")]
    [SerializeField] private float firePerStep = 2f;
    [SerializeField] private float delayBetweenSteps = 0.5f;
    [SerializeField] private bool disableAllFireBossPlaceOnStart = true;

    private Coroutine activeRoutine;
    // Start is called before the first frame update
    void Start()
    {
        if (fireBossPlace == null || fireBossPlace.Length == 0)
        {
            Debug.LogError(
                "ActiveFireBossPlace không tìm thấy FireBossPlace nào!",
                this
            );

            return;
        }
        if (disableAllFireBossPlaceOnStart)
        {
            foreach (GameObject firePlace in fireBossPlace)
            {
                firePlace.SetActive(false);
            }
        }
    }
    private void StartActiveFireBossPlace()
    {
        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
        }

        activeRoutine = StartCoroutine(ActiveFireBossPlaceRoutine());
    }
    private IEnumerator ActiveFireBossPlaceRoutine()
    {
        int currentIndex = 0;

        while (currentIndex < fireBossPlace.Length)
        {
            int activeThisStep = 0;
            while (activeThisStep < firePerStep && currentIndex < fireBossPlace.Length)
            {
                fireBossPlace[currentIndex].SetActive(true);
                currentIndex++;
                activeThisStep++;
            }
            
            if (currentIndex < fireBossPlace.Length)
            {
                yield return new WaitForSeconds(delayBetweenSteps);
            }
        }

        activeRoutine = null;
    }
    private void OnTriggerEnter(Collider player)
    {
        if (player.CompareTag("Player"))
        {
            StartActiveFireBossPlace();
        }
    }
}
