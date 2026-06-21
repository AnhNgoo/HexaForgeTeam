using System.Collections;
using UnityEngine;
using Cysharp.Threading.Tasks;
public class CharacterMelee : CharacterBase
{
    [Header("Melee Attack Effects")]
    public GameObject meleeAttackEffectPoint_1;
    public PoolType hitEffect_2 = PoolType.HitEffect_2;
    public PoolType meleeAttackEffect_1 = PoolType.SlashEffect_1;
    public GameObject meleeAttackEffectPoint_2;
    public PoolType meleeAttackEffect_2 = PoolType.SlashEffect_1;
    public GameObject meleeAttackEffectPoint_3;
    public PoolType meleeAttackEffect_3 = PoolType.SlashEffect_1;
    public GameObject meleeAttackEffectPoint_4;
    public PoolType meleeAttackEffect_4 = PoolType.Earthquake_1;
    [Header("Debug")]
    [SerializeField] protected bool debugMode = false; // Bật để hiển thị gizmo kiểm tra kẻ địch gần

    protected override void LoadEffectPoints()
    {
        base.LoadEffectPoints();
        if (meleeAttackEffectPoint_1 == null)
            meleeAttackEffectPoint_1 = effectPoints?.transform.Find("MeleeAttackEffectPoint_1")?.gameObject;
        if (meleeAttackEffectPoint_2 == null)
            meleeAttackEffectPoint_2 = effectPoints?.transform.Find("MeleeAttackEffectPoint_2")?.gameObject;
        if (meleeAttackEffectPoint_3 == null)
            meleeAttackEffectPoint_3 = effectPoints?.transform.Find("MeleeAttackEffectPoint_3")?.gameObject;
        if (meleeAttackEffectPoint_4 == null)
            meleeAttackEffectPoint_4 = effectPoints?.transform.Find("MeleeAttackEffectPoint_4")?.gameObject;
    }

    public override void Attack()
    {
        if (characterCombat.CurrentComboIndex == 0) // Chỉ áp sát mục tiêu nếu đây là đòn tấn công đầu tiên trong chuỗi combo
            MeleeSnapToTarget();
        characterCombat?.TryAttack();
    }

    //Hỗ trợ áp sát mục tiêu khi tấn công
    protected void MeleeSnapToTarget()
    {
        if (CharacterLockTarget == null ||
        !CharacterLockTarget.IsLockingTarget || // Chỉ áp sát nếu đang khóa mục tiêu
        !CharacterMovement.IsGrounded ||
        CharacterMovement.IsDodging ||
        IsHealthRecovering)
            return;

        Transform target = CharacterLockTarget.Target;

        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        if (distanceToTarget < meleeSnapThreshold.x || distanceToTarget > meleeSnapThreshold.y) return; // Nếu mục tiêu quá gần hoặc quá xa, không áp sát

        LungeToTarget();
    }

    protected virtual async void LungeToTarget()
    {
        CharacterMovement.IsLunging = true;

        float distanceToTarget = Vector3.Distance(transform.position, CharacterLockTarget.Target.position);

        while (distanceToTarget > meleeSnapThreshold.x && !CheckObstacleInFront())
        {
            Vector3 directionToTarget = (CharacterLockTarget.Target.position - transform.position).normalized;
            Vector2 direction = new Vector2(directionToTarget.x, directionToTarget.z);
            CharacterMovement.Lunge(direction, characterData.stats.speed);

            distanceToTarget = Vector3.Distance(transform.position, CharacterLockTarget.Target.position);
            await UniTask.Yield();
        }
        CharacterMovement.Stop();
        CharacterMovement.IsLunging = false;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!debugMode)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + transform.forward * ZoffsetCheckForNearEnemy, radiusCheckForNearEnemy);
    }
#endif
}

