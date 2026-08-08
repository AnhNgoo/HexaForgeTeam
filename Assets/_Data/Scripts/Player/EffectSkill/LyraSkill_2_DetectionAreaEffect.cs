using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LyraSkill_2_DetectionAreaEffect : MonoBehaviour, IPoolable
{
    [SerializeField] private List<Transform> enemies = new List<Transform>();
    public List<Transform> Enemies => enemies;

    private Transform characterTransform;
    public PoolType PoolType => PoolType.LyraSkill_2_DetectionAreaEffect;

    public void OnReturnToPool()
    {
        enemies.Clear();
    }

    public void OnSpawnFromPool()
    {

    }

    public void Init(Transform characterTransform, float radius)
    {
        this.characterTransform = characterTransform;
        transform.localScale = new Vector3(radius, radius, radius);
    }

    private void Update()
    {
        CheckObstacleFromCharacterToEnemy();
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
    /// <summary>
    /// Kiểm tra xem có vật cản giữa nhân vật và kẻ địch hay không. Nếu có, loại bỏ kẻ địch đó khỏi danh sách enemies. 
    /// Chạy ở Update để kiểm tra xem có vật cản giữa nhân vật và kẻ địch hay không
    /// </summary>

    private void CheckObstacleFromCharacterToEnemy()
    {
        if (characterTransform == null) return;

        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            Transform enemy = enemies[i];
            if (enemy == null)
            {
                enemies.RemoveAt(i);
                continue;
            }

            Vector3 directionToEnemy = enemy.position - characterTransform.position;
            float distanceToEnemy = directionToEnemy.magnitude;

            if (Physics.Raycast(characterTransform.position, directionToEnemy.normalized, out RaycastHit hit, distanceToEnemy))
            {
                if (!hit.collider.CompareTag("Enemy"))
                {
                    enemies.RemoveAt(i);
                }
            }
        }
    }
}
