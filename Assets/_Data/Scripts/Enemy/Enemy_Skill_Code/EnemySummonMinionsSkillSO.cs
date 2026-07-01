using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "EnemySummonMinionsSkillSO", menuName = "Enemy/Skills/SummonMinions")]
public class EnemySummonMinionsSkillSO : EnemyAttackSkillSO
{
    [SerializeField] private PoolType minionPoolType = PoolType.EnemySkeletonMelee;
    [SerializeField] private int summonCount = 2;
    [SerializeField] private float spawnRadius = 3f;
    [SerializeField] private float navMeshSampleRadius = 2f;
    [SerializeField] private string awakenAnimationState = "Skeletons_Awaken_Floor";
    [SerializeField] private float awakenLockDuration = 1.2f;
    [SerializeField] private float summonDelay = 0.8f;
    [SerializeField] private float postSummonLockDuration = 0.4f;

    public override void OnAttackStart(EnemyAttackContext context)
    {
        if (context?.Enemy == null || context.Target == null) return;

        SummonAsync(context).Forget();
    }

    public override void OnAttackImpact(EnemyAttackContext context)
    {
        // Đổi sang start summon trực tiếp trong OnAttackStart thử xem
    }

    private async UniTaskVoid SummonAsync(EnemyAttackContext context)
    {
        EnemyBase enemy = context.Enemy;
        SkeletonMageMinibossBehaviour mage =
            enemy.GetComponent<SkeletonMageMinibossBehaviour>();

        mage?.SetCastingLocked(true);
        enemy.Locomotion.StopMoving();

        await UniTask.Delay(System.TimeSpan.FromSeconds(summonDelay));

        SpawnMinions(context, mage);

        await UniTask.Delay(System.TimeSpan.FromSeconds(postSummonLockDuration));

        mage?.SetCastingLocked(false);
    }

    private void SpawnMinions(EnemyAttackContext context, SkeletonMageMinibossBehaviour mage)
    {
        for (int i = 0; i < summonCount; i++)
        {
            Vector3 dir =
                Quaternion.Euler(0f, i * 360f / summonCount, 0f) *
                context.Enemy.MyTransform.forward;

            Vector3 rawPos = context.Enemy.MyTransform.position + dir * spawnRadius;

            if (!NavMesh.SamplePosition(rawPos, out NavMeshHit hit, navMeshSampleRadius, NavMesh.AllAreas))
                continue;

            GameObject obj = ObjectPooling.Instance.SpawnFromPool(
                minionPoolType,
                hit.position,
                Quaternion.identity
            );

            if (obj == null) continue;

            EnemyBase minion = obj.GetComponent<EnemyBase>();
            if (minion == null) continue;

            minion.InitSummoned(hit.position, context.Target);
            mage?.RegisterMinion(minion);

            PlayAwakenAsync(minion).Forget();
        }
    }

    private async UniTaskVoid PlayAwakenAsync(EnemyBase minion)
    {
        minion.Locomotion.StopMoving();

        Animator animator = minion.AnimatorController.Animator;
        if (animator != null && animator.HasState(0, Animator.StringToHash(awakenAnimationState)))
            animator.CrossFadeInFixedTime(awakenAnimationState, 0.05f);

        await UniTask.Delay(System.TimeSpan.FromSeconds(awakenLockDuration));

        if (minion != null && minion.Health.CurrentHealth > 0f)
            minion.StateMachine.ChangeState(minion.StateMachine.EnemyChaseState);
    }
}