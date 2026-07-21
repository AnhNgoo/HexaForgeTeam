using UnityEngine;

public class EnemyAttackContext
{
    public EnemyBase Enemy { get; }
    public AttackDataSO AttackData { get; }
    public Transform Target { get; }
    public float RuntimeDamageMultiplier { get; }

    public EnemyAttackContext(EnemyBase enemyBase, AttackDataSO attackData, Transform target, float runtimeDamageMultiplier)
    {
        Enemy = enemyBase;
        AttackData = attackData;
        Target = target;
        RuntimeDamageMultiplier = runtimeDamageMultiplier;
    }
}
