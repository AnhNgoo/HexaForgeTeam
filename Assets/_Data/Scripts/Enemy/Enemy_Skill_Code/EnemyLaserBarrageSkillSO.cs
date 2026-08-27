using Cysharp.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(
    fileName = "EnemyLaserBarrageSkill",
    menuName = "Enemy/Skills/Dark Mage/Laser Barrage")]
public class EnemyLaserBarrageSkillSO : EnemyAttackSkillSO
{
    [Header("Pools")]
    [SerializeField] private PoolType telegraphPool;
    [SerializeField] private PoolType laserPool;

    [Header("Timing")]
    [SerializeField, Min(0f)] private float warningDuration = 0.65f;
    [SerializeField, Min(0.1f)] private float barrageDuration = 3f;

    [Header("Sweep")]
    [SerializeField, Range(0f, 360f)] private float sweepAngle = 160f;
    [SerializeField, Min(1)] private int sweepCount = 2;

    [Header("Placement")]
    [SerializeField] private float heightOffset = 0.8f;
    [SerializeField] private float forwardOffset = 0.5f;
    [SerializeField] private Vector3 rotationOffset;

    public override void OnAttackImpact(EnemyAttackContext context)
    {
        if (!IsValid(context))
            return;

        DarkMageBossBehaviour boss =
            context.Enemy.GetComponent<DarkMageBossBehaviour>();

        if (boss == null || !boss.BeginSpecialAction())
            return;

        RunBarrageAsync(context, boss).Forget();
    }

    private async UniTaskVoid RunBarrageAsync(
        EnemyAttackContext context,
        DarkMageBossBehaviour boss)
    {
        EnemyBase enemy = context.Enemy;
        GameObject telegraph = null;
        GameObject laser = null;
        EnemyHitbox laserHitbox = null;

        try
        {
            Transform anchor =
                enemy.Combat.ResolveProjectileAnchor(context.AttackData);

            Vector3 lockedDirection = GetAimDirection(context, anchor);

            telegraph = Spawn(
                telegraphPool,
                anchor.position,
                lockedDirection
            );

            float warningElapsed = 0f;

            while (warningElapsed < warningDuration)
            {
                if (!CanContinue(enemy))
                    return;

                warningElapsed += Time.deltaTime;

                anchor = enemy.Combat.ResolveProjectileAnchor(
                    context.AttackData
                );

                lockedDirection = GetAimDirection(context, anchor);

                PlaceEffect(
                    telegraph,
                    anchor,
                    lockedDirection
                );

                FaceDirection(enemy, lockedDirection);

                await UniTask.Yield(PlayerLoopTiming.Update);
            }

            ReturnEffect(telegraphPool, telegraph);
            telegraph = null;

            laser = Spawn(
                laserPool,
                anchor.position,
                lockedDirection
            );

            if (laser == null)
                return;

            laserHitbox =
                laser.GetComponentInChildren<EnemyHitbox>(true);

            if (laserHitbox != null)
            {
                laserHitbox.Initialize(
                    enemy,
                    context.AttackData,
                    context.RuntimeDamageMultiplier
                );

                laserHitbox.EnableHitBox();
            }

            float elapsed = 0f;

            while (elapsed < barrageDuration)
            {
                if (!CanContinue(enemy))
                    return;

                elapsed += Time.deltaTime;

                float normalized =
                    Mathf.Clamp01(elapsed / barrageDuration);

                float sweep =
                    Mathf.PingPong(
                        normalized * sweepCount,
                        1f
                    );

                float currentAngle = Mathf.Lerp(
                    -sweepAngle * 0.5f,
                    sweepAngle * 0.5f,
                    sweep
                );

                Vector3 sweepDirection =
                    Quaternion.AngleAxis(
                        currentAngle,
                        Vector3.up
                    ) * lockedDirection;

                anchor = enemy.Combat.ResolveProjectileAnchor(
                    context.AttackData
                );

                PlaceEffect(laser, anchor, sweepDirection);
                FaceDirection(enemy, sweepDirection);

                await UniTask.Yield(PlayerLoopTiming.Update);
            }
        }
        finally
        {
            if (laserHitbox != null)
            {
                laserHitbox.DisableHitBox();
                laserHitbox.Initialize(enemy);
            }

            ReturnEffect(telegraphPool, telegraph);
            ReturnEffect(laserPool, laser);

            if (enemy != null &&
                enemy.gameObject.activeInHierarchy &&
                enemy.Health.CurrentHealth > 0f)
            {
                enemy.AnimatorController.PlayAnimation(
                    enemy.AnimatorController.IdleHash,
                    0.08f
                );
            }

            boss?.EndSpecialAction();
        }
    }

    private GameObject Spawn(
        PoolType pool,
        Vector3 position,
        Vector3 direction)
    {
        if (pool == PoolType.None ||
            ObjectPooling.Instance == null)
        {
            return null;
        }

        Quaternion rotation =
            Quaternion.LookRotation(direction) *
            Quaternion.Euler(rotationOffset);

        return ObjectPooling.Instance.SpawnFromPool(
            pool,
            position,
            rotation
        );
    }

    private void PlaceEffect(
        GameObject effect,
        Transform anchor,
        Vector3 direction)
    {
        if (effect == null || anchor == null)
            return;

        Vector3 position =
            anchor.position +
            Vector3.up * heightOffset +
            direction * forwardOffset;

        Quaternion rotation =
            Quaternion.LookRotation(direction) *
            Quaternion.Euler(rotationOffset);

        effect.transform.SetPositionAndRotation(position, rotation);
    }

    private static Vector3 GetAimDirection(
        EnemyAttackContext context,
        Transform anchor)
    {
        Transform target =
            context.Enemy.Detection.CurrentTarget ??
            context.Target;

        if (target == null || anchor == null)
            return context.Enemy.MyTransform.forward;

        Vector3 direction = target.position - anchor.position;
        direction.y = 0f;

        return direction.sqrMagnitude > 0.01f
            ? direction.normalized
            : context.Enemy.MyTransform.forward;
    }

    private static void FaceDirection(
        EnemyBase enemy,
        Vector3 direction)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.01f)
        {
            enemy.MyTransform.rotation =
                Quaternion.LookRotation(direction);
        }
    }

    private static bool CanContinue(EnemyBase enemy)
    {
        return enemy != null &&
               enemy.gameObject.activeInHierarchy &&
               enemy.Health.CurrentHealth > 0f &&
               enemy.StateMachine.CurrentState ==
               enemy.StateMachine.EnemyAttackState;
    }

    private static bool IsValid(EnemyAttackContext context)
    {
        return context?.Enemy != null &&
               context.AttackData != null &&
               ObjectPooling.Instance != null;
    }

    private static void ReturnEffect(
        PoolType pool,
        GameObject effect)
    {
        if (pool == PoolType.None ||
            effect == null ||
            !effect.activeInHierarchy ||
            ObjectPooling.Instance == null)
        {
            return;
        }

        ObjectPooling.Instance.ReturnToPool(pool, effect);
    }
}