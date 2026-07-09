using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class Projectile : LoadComponents, IPoolable
{
    [SerializeField] private PoolType poolType;
    [SerializeField] private float lifeTime = 5f;
    [SerializeField] private float speed = 10f;
    [SerializeField] private Rigidbody rb;
    private Vector3 direction;
    private Cooldown cooldownLifeTime = new Cooldown();
    private PoolType hitEffect;
    private CharacterBase characterBase;

    public PoolType PoolType => poolType;

    protected override void LoadComponent()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();
    }

    protected override void LoadComponentRuntime()
    {

    }

    public void Init(Vector3 direction, CharacterBase characterBase, PoolType hitEffect = PoolType.None)
    {
        this.direction = direction.normalized;
        this.hitEffect = hitEffect;
        this.characterBase = characterBase;
        transform.rotation = Quaternion.LookRotation(direction);
        cooldownLifeTime.StartCooldown(lifeTime);
    }

    private void Update()
    {
        if (!cooldownLifeTime.IsOnCooldown)
        {
            ObjectPooling.Instance.ReturnToPool(poolType, gameObject);
        }
    }
    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out EnemyBase enemy))
        {
            float damage = characterBase.CharacterData.stats.damage;
            float poisonDamage = characterBase.CharacterData.stats.poisonDamage;
            if (enemy.DamageReceiver != null)
            {
                enemy.DamageReceiver.TakeHit(damage, poisonDamage, transform);
            }
        }

        if (hitEffect != PoolType.None)
        {
            ObjectPooling.Instance.SpawnFromPool(hitEffect, other.ClosestPoint(transform.position), Quaternion.identity);
        }
        cooldownLifeTime.Stop();
    }

    public void OnSpawnFromPool()
    {

    }

    public void OnReturnToPool()
    {

    }
}