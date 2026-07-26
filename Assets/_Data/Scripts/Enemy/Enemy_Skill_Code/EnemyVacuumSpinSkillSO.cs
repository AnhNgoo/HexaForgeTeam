using Cysharp.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(
    fileName = "EnemyVacuumSpinSkill",
    menuName = "Enemy/Skills/Vacuum Spin")]
public class EnemyVacuumSpinSkillSO : EnemyAttackSkillSO
{
    [Header("Vacuum")]
    [SerializeField] private PoolType vacuumPool;
    [SerializeField] private float pullRadius = 7f;
    [SerializeField] private float pullStrength = 8f;
    [SerializeField] private float pullDuration = 1.4f;

    [Header("Damage")]
    [SerializeField] private float damageDelay = 1f;
    [SerializeField] private float damageWindow = 0.3f;

    [Header("Eclipse")]
    [SerializeField] private EnemyShadowRainSkillSO shadowRainSkill;
    [SerializeField] private float eclipseDuration = 4f;
    [SerializeField] private int eclipseRainWaves = 3;
    [SerializeField] private float eclipseWaveInterval = 0.2f;

    public override void OnAttackImpact(EnemyAttackContext context)
    {
        if (context?.Enemy == null ||
            context.AttackData == null)
        {
            return;
        }

        NightStalkerBossBehaviour behaviour =
            context.Enemy.GetComponent<NightStalkerBossBehaviour>();

        if (behaviour == null ||
            !behaviour.TryBeginVacuum(out bool isEclipse))
        {
            return;
        }

        RunVacuumAsync(
            context,
            behaviour,
            isEclipse
        ).Forget();
    }

    private async UniTaskVoid RunVacuumAsync(
        EnemyAttackContext context,
        NightStalkerBossBehaviour behaviour,
        bool isEclipse)
    {
        EnemyBase enemy = context.Enemy;
        GameObject vacuum = null;
        EnemyHitbox hitbox = null;

        try
        {
            vacuum = ObjectPooling.Instance.SpawnFromPool(
                vacuumPool,
                enemy.MyTransform.position,
                Quaternion.identity,
                enemy.MyTransform
            );

            if (vacuum == null)
            {
                Debug.LogWarning(
                    "[Vacuum Spin] Không spawn được Vacuum pool."
                );
                return;
            }

            hitbox =
                vacuum.GetComponentInChildren<EnemyHitbox>(true);

            hitbox?.Initialize(
                enemy,
                context.AttackData,
                context.RuntimeDamageMultiplier
            );

            float radius =
                pullRadius * behaviour.VacuumRadiusMultiplier;

            float strength =
                pullStrength * behaviour.VacuumPullMultiplier;

            float activeDuration =
                isEclipse ? eclipseDuration : pullDuration;

            CharacterMovement targetMovement =
                context.Target != null
                    ? context.Target.GetComponentInParent<
                        CharacterMovement>()
                    : null;

            if (isEclipse &&
                shadowRainSkill != null &&
                behaviour.ShadowRainAttack != null)
            {
                EnemyAttackContext rainContext =
                    new EnemyAttackContext(
                        enemy,
                        behaviour.ShadowRainAttack,
                        context.Target,
                        context.RuntimeDamageMultiplier
                    );

                shadowRainSkill.CastRainAsync(
                    rainContext,
                    eclipseRainWaves,
                    eclipseWaveInterval
                ).Forget();
            }

            float elapsed = 0f;
            bool hitboxOpened = false;

            while (elapsed < activeDuration)
            {
                if (!CanContinue(enemy))
                    break;

                elapsed += Time.deltaTime;
                vacuum.transform.position =
                    enemy.MyTransform.position;

                PullTarget(
                    targetMovement,
                    enemy,
                    radius,
                    strength
                );

                if (!hitboxOpened && elapsed >= damageDelay)
                {
                    hitboxOpened = true;
                    hitbox?.EnableHitBox();
                }

                if (hitboxOpened &&
                    elapsed >= damageDelay + damageWindow)
                {
                    hitbox?.DisableHitBox();
                }

                await UniTask.Yield(PlayerLoopTiming.Update);
            }
        }
        finally
        {
            hitbox?.DisableHitBox();

            if (vacuum != null &&
                vacuum.activeInHierarchy &&
                ObjectPooling.Instance != null)
            {
                ObjectPooling.Instance.ReturnToPool(
                    vacuumPool,
                    vacuum
                );
            }

            if (behaviour != null)
                behaviour.EndVacuum();
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

    private static void PullTarget(
        CharacterMovement movement,
        EnemyBase enemy,
        float radius,
        float strength)
    {
        if (movement?.CC == null)
            return;

        Vector3 direction =
            enemy.MyTransform.position -
            movement.transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > radius * radius ||
            direction.sqrMagnitude <= 0.01f)
        {
            return;
        }

        movement.CC.Move(
            direction.normalized *
            strength *
            Time.deltaTime
        );
    }
}
