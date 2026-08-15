using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMeleeHitbox : MonoBehaviour
{
    [SerializeField] private bool debugMode = true;
    [SerializeField] private LayerMask enemyLayer;
    [Header("Attack Hitbox Settings")]
    [SerializeField] private float forwardAttackOffset = 1.5f;
    [SerializeField] private float yAttackOffset = 0f;
    [SerializeField] private float attackHitBoxRadius = 1f;

    private CharacterBase characterBase;
    private HitPauseEffect hitPauseEffect = new HitPauseEffect();
    private float tempForwardAttackOffset;
    private float tempYAttackOffset;
    private float tempAttackHitBoxRadius;

    public void Init(CharacterBase character)
    {
        characterBase = character;
        ResetHitBox();
    }

    public void SetHitBox(float forwardOffset, float yOffset, float radius)
    {
        tempForwardAttackOffset = forwardAttackOffset;
        tempYAttackOffset = yAttackOffset;
        tempAttackHitBoxRadius = attackHitBoxRadius;

        forwardAttackOffset = forwardOffset;
        yAttackOffset = yOffset;
        attackHitBoxRadius = radius;
    }

    public void ResetHitBox()
    {
        if (tempForwardAttackOffset == 0f && tempYAttackOffset == 0f && tempAttackHitBoxRadius == 0f)
            return;
        forwardAttackOffset = tempForwardAttackOffset;
        yAttackOffset = tempYAttackOffset;
        attackHitBoxRadius = tempAttackHitBoxRadius;
    }

    //Bật hixbox tấn công
    public void AttackHitBox(PoolType hitEffect = PoolType.None, bool hasHitPause = false)
    {
        Vector3 offset = transform.forward * forwardAttackOffset + transform.up * yAttackOffset;
        Collider[] hitColliders = Physics.OverlapSphere(transform.position + offset, attackHitBoxRadius, enemyLayer);

        if (hitColliders.Length == 0)
            return;

        if (hasHitPause)
            hitPauseEffect.PlayHitPause(0.5f, 0.3f); // Tạm dừng thời gian khi đòn tấn công trúng mục tiêu

        foreach (Collider hitCollider in hitColliders)
        {
            AttackHandler(hitCollider, hitEffect);
        }
    }

    // Xử lý logic khi đòn tấn công chạm trúng đối tượng
    private void AttackHandler(Collider other, PoolType hitEffect = PoolType.None)
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

        CameraShake.Instance.Shake();
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!debugMode)
            return;
        Gizmos.color = Color.red;

        Vector3 offset = transform.forward * forwardAttackOffset + transform.up * yAttackOffset;
        Gizmos.DrawWireSphere(transform.position + offset, attackHitBoxRadius);
    }
#endif
}
