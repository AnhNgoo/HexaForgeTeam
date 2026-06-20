using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EarthBreakerSkill : MonoBehaviour
{
    [SerializeField] private float radius = 6f;
    [SerializeField] private LayerMask enemyLayer;
    public void Init(float damage, float poisonDamage)
    {
        Hitbox(damage, poisonDamage);
    }

    private void Hitbox(float damage, float poisonDamage)
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius, enemyLayer);
        foreach (Collider hitCollider in hitColliders)
        {
            EnemyBase enemy = hitCollider.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                if (enemy.DamageReceiver != null)
                {
                    enemy.DamageReceiver.TakeHit(damage, poisonDamage);
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
