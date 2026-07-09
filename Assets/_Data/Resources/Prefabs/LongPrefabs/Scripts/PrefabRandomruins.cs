using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PrefabRandomruins : MonoBehaviour
{
    public SpawnData[] spawnDatas;
    public Transform target;    
    
    public void SpawnRuin()
    {   
        foreach (SpawnData data in spawnDatas)
        {
            if (data.prefabs == null) continue;

            int spawnCount = Mathf.Min(data.spawnPoints.Length, data.prefabs.Length);

            List<int> prefabIndexes = Enumerable.Range(0, data.prefabs.Length).ToList();
            for (int i = 0; i < prefabIndexes.Count; i++)
            {
                int randomIndex = Random.Range(i, prefabIndexes.Count);
                (prefabIndexes[i], prefabIndexes[randomIndex]) = (prefabIndexes[randomIndex], prefabIndexes[i]);
            }

            List<int> pointIndexes = Enumerable.Range(0, data.spawnPoints.Length).ToList();
            for (int i = 0; i < pointIndexes.Count; i++)
            {
                int randomIndex = Random.Range(i, pointIndexes.Count);
                (pointIndexes[i], pointIndexes[randomIndex]) = (pointIndexes[randomIndex], pointIndexes[i]);
            }

            for (int i = 0; i < spawnCount; i++)
            {
                GameObject prefabs = data.prefabs[prefabIndexes[i]];
                Transform point = data.spawnPoints[pointIndexes[i]];

                Ray ray = new Ray(point.position + Vector3.up * 40f, Vector3.down);
            
                if (Physics.Raycast(ray, out RaycastHit hit, 100f))
                {
                    GameObject bonus = Instantiate(prefabs, hit.point + Vector3.up * data.yOffset, Quaternion.identity);

                    if (target != null)
                    {
                        Vector3 direction = target.position - bonus.transform.position;
                        direction.y = 0;
                        bonus.transform.rotation = Quaternion.LookRotation(direction);
                    }
                }
            }
        }
    }
}
