using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LyraSkill_2_DetectionAreaEffect : MonoBehaviour, IPoolable
{
    [SerializeField] private List<Transform> enemies = new List<Transform>();
    public List<Transform> Enemies => enemies;

    public PoolType PoolType => PoolType.LyraSkill_2_DetectionAreaEffect;

    public void OnReturnToPool()
    {
        enemies.Clear();
    }

    public void OnSpawnFromPool()
    {

    }

    public void SetRadiusDetectionArea(float radius)
    {
        transform.localScale = new Vector3(radius, radius, radius);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (!enemies.Contains(other.transform))
            {
                enemies.Add(other.transform);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (enemies.Contains(other.transform))
            {
                enemies.Remove(other.transform);
            }
        }
    }

    public void ClearEnemy(Transform enemy)
    {
        if (enemies.Contains(enemy))
        {
            enemies.Remove(enemy);
        }
    }
}
