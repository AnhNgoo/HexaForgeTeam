using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Rigidbody))]
public class Projectile : LoadComponents, IPoolable
{
    [Header("Projectile Properties")]
    [SerializeField] protected PoolType poolType;
    [SerializeField] protected float lifeTime = 5f;
    [SerializeField] protected float speed = 30f;
    [SerializeField] protected CapsuleCollider _collider;
    [SerializeField] protected Rigidbody _rigidbody;
    protected Vector3 direction;
    protected Cooldown cooldownLifeTime = new Cooldown();
    protected PoolType hitEffect;
    protected CharacterBase characterBase;


    public PoolType PoolType => poolType;

    protected override void LoadComponent()
    {
        _collider = GetComponent<CapsuleCollider>();
        _rigidbody = GetComponent<Rigidbody>();
    }

    protected override void LoadComponentRuntime()
    {

    }

    public virtual void Init(Vector3 direction, CharacterBase characterBase, PoolType hitEffect = PoolType.None)
    {
        _collider.enabled = false;
        this.direction = direction.normalized;
        this.hitEffect = hitEffect;
        this.characterBase = characterBase;
        transform.rotation = Quaternion.LookRotation(direction);
        cooldownLifeTime.StartCooldown(lifeTime);
        _collider.enabled = true;
    }

    protected virtual void Update()
    {
        if (!cooldownLifeTime.IsOnCooldown)
        {
            ObjectPooling.Instance.ReturnToPool(poolType, gameObject);
        }
    }
    protected virtual void FixedUpdate()
    {
        _rigidbody.MovePosition(_rigidbody.position + direction * speed * Time.fixedDeltaTime);
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out EnemyBase enemy))
        {
            float damage = characterBase.CharacterStat.finalStats.damage + characterBase.CharacterStat.GetWeaponDamage();
            float poisonDamage = characterBase.CharacterStat.finalStats.poisonDamage + characterBase.CharacterStat.GetWeaponPoisonDamage();
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

    public virtual void OnSpawnFromPool()
    {

    }

    public virtual void OnReturnToPool()
    {
        _collider.enabled = false;
    }
}