using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class LyraSkill_1_Projectile : Projectile, IPoolable
{
    [Header("Skill Settings")]
    [SerializeField] private float radiusDetection = 10f;
    [SerializeField] private int maxBounces = 3; // Số lần tối đa để phát hiện kẻ địch mới
    private List<GameObject> enemyPrevious;
    private int bounces = 1; // Số lần đã phát hiện kẻ địch mới


    protected override void LoadComponent()
    {
        base.LoadComponent();
    }

    protected override void LoadComponentRuntime()
    {

    }

    public override void Init(Vector3 direction, CharacterBase characterBase, PoolType hitEffect = PoolType.None)
    {
        Init(direction, characterBase, hitEffect, null);
    }

    public void Init(Vector3 direction, CharacterBase characterBase, PoolType hitEffect = PoolType.None, List<GameObject> enemyPrevious = null, int bounces = 1)
    {
        _collider.enabled = false;
        base.Init(direction, characterBase, hitEffect);
        this.enemyPrevious = new List<GameObject>();
        this.bounces = bounces;
        if (enemyPrevious != null)
        {
            this.enemyPrevious.AddRange(enemyPrevious); // Lưu trữ kẻ địch trước đó để tránh va chạm lại
        }
        _collider.enabled = true;
    }
    protected override void Update()
    {
        if (!cooldownLifeTime.IsOnCooldown)
        {
            ObjectPooling.Instance.ReturnToPool(poolType, gameObject);
        }
    }
    protected override void FixedUpdate()
    {
        _rigidbody.MovePosition(_rigidbody.position + direction * speed * Time.fixedDeltaTime);
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (enemyPrevious != null && enemyPrevious.Contains(other.gameObject))
        {
            return; // Bỏ qua nếu va chạm với kẻ địch trước đó
        }

        if (other.TryGetComponent(out EnemyBase enemy))
        {
            float damage = characterBase.CharacterData.stats.damage;
            float poisonDamage = characterBase.CharacterData.stats.poisonDamage;
            if (enemy.DamageReceiver != null)
            {
                enemy.DamageReceiver.TakeHit(damage, poisonDamage, transform);
            }

            if (bounces <= maxBounces)
            {
                DetectEnemiesInRadius(other.transform);
            }
        }

        if (hitEffect != PoolType.None)
        {
            ObjectPooling.Instance.SpawnFromPool(hitEffect, other.ClosestPoint(transform.position), Quaternion.identity);
        }
        cooldownLifeTime.Stop();
    }

    private void DetectEnemiesInRadius(Transform currentHitEnemy) // enemy đã trúng đòn
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radiusDetection);

        GameObject closestEnemy = null;
        closestEnemy = FindClosestEnemy(hitColliders, currentHitEnemy.gameObject);

        if (closestEnemy != null)
        {
            SpawnProjectile(closestEnemy.transform, currentHitEnemy);
        }
    }

    private GameObject FindClosestEnemy(Collider[] hitColliders, GameObject currentHitEnemy = null)
    {
        GameObject closestEnemy = null;
        foreach (var hitCollider in hitColliders)
        {
            if (enemyPrevious != null && enemyPrevious.Contains(hitCollider.gameObject) ||
                (currentHitEnemy && hitCollider.gameObject == currentHitEnemy))
            {
                continue;
            }
            //Tìm Enemy gần nhất
            if (hitCollider.TryGetComponent(out EnemyBase enemy))
            {
                if (closestEnemy == null ||
                 Vector3.Distance(transform.position,
                                 hitCollider.transform.position) < Vector3.Distance(transform.position, closestEnemy.transform.position))
                {
                    closestEnemy = hitCollider.gameObject;
                }
            }
        }
        return closestEnemy;
    }

    private void SpawnProjectile(Transform closestEnemy, Transform currentHitEnemy)
    {
        GameObject projectileObj = ObjectPooling.Instance.SpawnFromPool(PoolType.LyraSkill_1_Projectile,
                                           currentHitEnemy.position,
                                           currentHitEnemy.rotation);

        if (projectileObj.TryGetComponent(out LyraSkill_1_Projectile lyraProjectile))
        {
            Vector3 directionToEnemy = (closestEnemy.transform.position - currentHitEnemy.position).normalized;

            List<GameObject> newEnemyPrevious = new List<GameObject>(this.enemyPrevious); // Tạo bản sao của danh sách kẻ địch trước đó
            newEnemyPrevious.Add(currentHitEnemy.gameObject); // Thêm kẻ địch vừa trúng đòn vào danh sách kẻ địch trước đó

            lyraProjectile.Init(directionToEnemy, characterBase, hitEffect, newEnemyPrevious, this.bounces + 1);
        }
    }

    public override void OnReturnToPool()
    {
        base.OnReturnToPool();
        enemyPrevious.Clear();
        bounces = 1;
    }
}