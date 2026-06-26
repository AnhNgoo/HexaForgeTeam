using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "EnemyBurrowAttackSkillSO", menuName = "Enemy/Skills/BurrowAttack")]
public class EnemyBurrowAttackSkillSO : EnemyAttackSkillSO
{
    [SerializeField] private string burrowDownState = "Idle to Underground";
    [SerializeField] private string emergeState = "Underground to Head Only";
    [SerializeField] private string biteState = "Head Attack In Place";

    [SerializeField] private float burrowDownDuration = 0.7f;
    [SerializeField] private float emergeDuration = 0.45f;
    [SerializeField] private float biteHitboxDuration = 0.25f;
    [SerializeField] private float emergeDistanceFromTarget = 1.4f;
    [SerializeField] private float navMeshSampleRadius = 2f;

    public override void OnAttackStart(EnemyAttackContext context)
    {
        if (context?.Enemy == null || context.Target == null || context.AttackData == null) return;
        BurrowAsync(context).Forget();
    }

    private async UniTaskVoid BurrowAsync(EnemyAttackContext context)
    {
        EnemyBase enemy = context.Enemy;
        BurrowMinibossBehaviour behaviour = enemy.GetComponent<BurrowMinibossBehaviour>();

        behaviour?.SetActionLocked(true);
        enemy.Combat.ForceCloseHitbox();
        enemy.Locomotion.StopMoving();

        Animator animator = enemy.AnimatorController.Animator;
        animator?.CrossFadeInFixedTime(burrowDownState, 0.05f);

        await UniTask.Delay(System.TimeSpan.FromSeconds(burrowDownDuration));

        Vector3 targetPos = context.Target.position;
        Vector3 dir = enemy.MyTransform.position - targetPos;
        dir.y = 0f;

        if (dir.sqrMagnitude <= 0.01f)
            dir = -context.Target.forward;

        Vector3 rawEmergePos = targetPos + dir.normalized * emergeDistanceFromTarget;
        rawEmergePos = enemy.Detection.ClampPointToLeash(rawEmergePos);

        if (NavMesh.SamplePosition(rawEmergePos, out NavMeshHit hit, navMeshSampleRadius, NavMesh.AllAreas))
            rawEmergePos = hit.position;

        enemy.Locomotion.WarpTo(rawEmergePos);
        FaceTarget(enemy, context.Target);

        animator?.CrossFadeInFixedTime(emergeState, 0.05f);
        await UniTask.Delay(System.TimeSpan.FromSeconds(emergeDuration));

        FaceTarget(enemy, context.Target);
        animator?.CrossFadeInFixedTime(biteState, 0.05f);

        enemy.Combat.EnableHitbox(context.AttackData.hitboxType);
        behaviour?.NotifyBurrowEmerged();

        await UniTask.Delay(System.TimeSpan.FromSeconds(biteHitboxDuration));

        enemy.Combat.DisableHitbox(context.AttackData.hitboxType);
        behaviour?.SetActionLocked(false);
    }

    private void FaceTarget(EnemyBase enemy, Transform target)
    {
        Vector3 dir = target.position - enemy.MyTransform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude > 0.01f)
            enemy.MyTransform.rotation = Quaternion.LookRotation(dir.normalized);
    }
}