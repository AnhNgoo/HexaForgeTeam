using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RandomBonusHealth : MonoBehaviour
{
    public Transform target;
    public Transform[] spawnPoint;
    List<int> indexes;
    public GameObject bonusHealthPrefab;
    private int randomBonusHealth;
    void Start()
    {
        randomBonusHealth = UnityEngine.Random.Range(0, 3);
        indexes = Enumerable.Range(0, spawnPoint.Length).ToList();

        for (int i = 0; i < indexes.Count; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, indexes.Count);
            (indexes[i], indexes[randomIndex]) = (indexes[randomIndex], indexes[i]);
        }

        for (int i = 0; i <= randomBonusHealth; i++)
        {
            int spawnIndex = indexes[i];

            GameObject bonus = Instantiate(bonusHealthPrefab, spawnPoint[spawnIndex].position, Quaternion.identity);
            Vector3 direction = target.position - bonus.transform.position;
            direction.y = 0;
            bonus.transform.rotation = Quaternion.LookRotation(-direction);
        }
    }
}
