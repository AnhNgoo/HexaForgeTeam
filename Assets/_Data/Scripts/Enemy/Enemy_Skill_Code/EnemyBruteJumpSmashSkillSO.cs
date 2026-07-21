using Cysharp.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(
    fileName = "EnemyBruteJumpSmashSkill",
    menuName = "Enemy/Skills/Brute Jump Smash")]
public class EnemyBruteJumpSmashSkillSO : EnemyAttackSkillSO
{
    [SerializeField] private PoolType shockwavePool;
    [SerializeField] private EnemyEarthPillarsSkillSO earthPillarsSkill;

    [SerializeField] private int ringCount = 3;
    [SerializeField] private float intervalBetweenRings = 0.18f;
    [SerializeField] private float ringLifetime = 0.65f;

    [Header("Phase 2")]
    [SerializeField] private float repeatAnimationDelay = 0.2f;
    [SerializeField] private float secondImpactDelay = 0.75f;

    public override void OnAttackImpact(EnemyAttackContext context)
    {
        BruteBossBehaviour brute =
            context?.Enemy?.GetComponent<BruteBossBehaviour>();

        if (brute == null ||
            !brute.TryBeginJumpSequence(
                out bool doubleSmash,
                out bool cataclysm))
            return;

        ExecuteAsync(context, brute, doubleSmash, cataclysm).Forget();
    }

    private async UniTaskVoid ExecuteAsync(
        EnemyAttackContext context,
        BruteBossBehaviour brute,
        bool doubleSmash,
        bool cataclysm)
    {
        brute.SetJumpSequenceLocked(doubleSmash);

        await SpawnRingsAsync(context);

        if (cataclysm)
        {
            brute.NotifyJumpSmashFinished(context.Target);

            if (earthPillarsSkill != null) await earthPillarsSkill.CastCataclysmAsync(context);

            brute.EndJumpSequence();
            return;
        }

        if (doubleSmash)
        {
            await UniTask.Delay(
                System.TimeSpan.FromSeconds(repeatAnimationDelay)
            );

            Animator animator = context.Enemy.AnimatorController.Animator;
            animator.speed = brute.ModifyAttackAnimationSpeed(1f);
            animator.CrossFadeInFixedTime(
                context.AttackData.animationStateName,
                0.05f
            );

            float attackSpeed = brute.ModifyAttackAnimationSpeed(1f);

            await UniTask.Delay(System.TimeSpan.FromSeconds(
                secondImpactDelay / attackSpeed
            ));

            await SpawnRingsAsync(context);
        }

        brute.NotifyJumpSmashFinished(context.Target);
        brute.EndJumpSequence();
    }

    private async UniTask SpawnRingsAsync(EnemyAttackContext context)
    {
        for (int i = 0; i < ringCount; i++)
        {
            GameObject ring = ObjectPooling.Instance.SpawnFromPool(
                shockwavePool,
                context.Enemy.MyTransform.position,
                Quaternion.identity
            );

            if (ring != null)
            {
                EnemyHitbox hitbox =
                    ring.GetComponentInChildren<EnemyHitbox>(true);

                hitbox?.Initialize(context.Enemy, context.AttackData, context.RuntimeDamageMultiplier);
                hitbox?.EnableHitBox();

                ReturnRingAsync(ring, hitbox).Forget();
            }

            await UniTask.Delay(
                System.TimeSpan.FromSeconds(intervalBetweenRings)
            );
        }
    }

    private async UniTaskVoid ReturnRingAsync(
        GameObject ring,
        EnemyHitbox hitbox)
    {
        await UniTask.Delay(
            System.TimeSpan.FromSeconds(ringLifetime)
        );

        hitbox?.DisableHitBox();

        if (ring != null && ring.activeInHierarchy)
            ObjectPooling.Instance.ReturnToPool(shockwavePool, ring);
    }
}