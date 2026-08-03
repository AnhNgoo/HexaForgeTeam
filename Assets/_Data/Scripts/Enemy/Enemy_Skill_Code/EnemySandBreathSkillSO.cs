using Cysharp.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemySandBreathSkill",
    menuName = "Enemy/Skills/Sand Breath")]
public class EnemySandBreathSkillSO : EnemyAttackSkillSO
{
    [SerializeField, Min(0.05f)] private float activeDuration = 0.45f;

    public override void OnAttackImpact(EnemyAttackContext context)
    {
        if (context?.Enemy == null || context.AttackData == null ||
            context.AttackData.attackVFX == PoolType.None) return;

        Transform anchor = context.Enemy.Combat.ResolveVFXAnchor(context.AttackData);
        Vector3 position = anchor.position +
            anchor.TransformDirection(context.AttackData.vfxOffset);
        Quaternion rotation = anchor.rotation *
            Quaternion.Euler(context.AttackData.vfxEuler);

        GameObject instance = ObjectPooling.Instance.SpawnFromPool(
            context.AttackData.attackVFX, position, rotation
        );
        if (instance == null) return;

        instance.transform.localScale =
            Vector3.one * context.AttackData.vfxScale;
        EnemyHitbox hitbox = instance.GetComponentInChildren<EnemyHitbox>(true);
        hitbox?.Initialize(context.Enemy, context.AttackData,
            context.RuntimeDamageMultiplier);
        hitbox?.EnableHitBox();
        float attackSpeed = context.Enemy.MinibossBehaviour?
            .ModifyAttackAnimationSpeed(1f) ?? 1f;
        ReturnAsync(context.AttackData.attackVFX, instance, hitbox,
            activeDuration / Mathf.Max(0.01f, attackSpeed)).Forget();
    }

    private async UniTaskVoid ReturnAsync(
        PoolType pool, GameObject instance, EnemyHitbox hitbox, float duration)
    {
        await UniTask.Delay(System.TimeSpan.FromSeconds(duration));
        hitbox?.DisableHitBox();
        if (instance != null && instance.activeInHierarchy)
            ObjectPooling.Instance.ReturnToPool(pool, instance);
    }
}
