using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FIMSpace.FTools;
using UnityEngine;

public class TestRandom : MonoBehaviour
{
    public Transform target;
    public Transform[] spawnPoint;
    List<int> indexes;
    public GameObject prefab;
    private int randomBonusHealth;
    // Start is called before the first frame update
    void Start()
    {
        randomBonusHealth = Random.Range(0 , 3);
        indexes = Enumerable.Range(0, spawnPoint.Length).ToList();

        for (int i = 0; i < indexes.Count; i++)
        {
            int randomCount = Random.Range(0, indexes.Count);
            (indexes[i], indexes[randomCount]) = (indexes[randomCount], indexes[i]); 
        }
        for (int i = 0; i <= randomBonusHealth; i++)
        {
            int spawnIndex = indexes[i];

            Transform point = spawnPoint[spawnIndex];
            Ray ray = new Ray(point.position + Vector3.up * 40f, Vector3.down);

            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                GameObject bonus = Instantiate(prefab, hit.point, Quaternion.identity);
                Vector3 direction = target.position - bonus.transform.position;
                direction.y = 0;

                bonus.transform.rotation = Quaternion.LookRotation(-direction);
            }
            else
            {
                Debug.Log("Miss Raycast");
            }
        }
    }
}
