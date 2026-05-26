using System.Collections;
using UnityEngine;
using Cysharp.Threading.Tasks;
public class CharacterMelee : CharacterBase
{
    [SerializeField] protected Vector2 meleeSnapThreshold = new Vector2(2.5f, 15f); // Tầm áp sát tối thiểu và tối đa để kích hoạt snap
    [SerializeField] protected float ZoffsetCheckForNearEnemy = 1.5f; // Khoảng cách Z để kiểm tra kẻ địch gần trước mặt không để tắt root motion khi tấn công
    [SerializeField] protected float radiusCheckForNearEnemy = 1f; // Bán kính để kiểm tra kẻ địch gần trước mặt không để tắt root motion khi tấn công
    [Header("Melee Attack Effects")]
    public GameObject meleeAttackEffect_1;
    public GameObject meleeAttackEffect_2;
    public GameObject meleeAttackEffect_3;
    public GameObject meleeAttackEffect_4;
    [Header("Debug")]
    [SerializeField] protected bool debugMode = false; // Bật để hiển thị gizmo kiểm tra kẻ địch gần

    protected override void LoadEffects()
    {
        base.LoadEffects();
        if (meleeAttackEffect_1 == null)
            meleeAttackEffect_1 = effectsContainer.transform.Find("MeleeAttackEffect_1")?.gameObject;
        if (meleeAttackEffect_2 == null)
            meleeAttackEffect_2 = effectsContainer.transform.Find("MeleeAttackEffect_2")?.gameObject;
        if (meleeAttackEffect_3 == null)
            meleeAttackEffect_3 = effectsContainer.transform.Find("MeleeAttackEffect_3")?.gameObject;
        if (meleeAttackEffect_4 == null)
            meleeAttackEffect_4 = effectsContainer.transform.Find("MeleeAttackEffect_4")?.gameObject;
    }
    protected override void Init(CharacterData data)
    {
        base.Init(data);
        characterCombat?.Init(this, InitAttackCombos());
    }

    // Override để khởi tạo các đòn tấn công riêng cho Kael
    protected override IAttackStep[] InitAttackCombos()
    {
        return new IAttackStep[4]
        {
            new AttackMeleeStep_1(this),
            new AttackMeleeStep_2(this),
            new AttackMeleeStep_3(this),
            new AttackMeleeStep_4(this)
        };
    }

    protected override async void OnAttack()
    {
        if (characterCombat.FirstAttack) // Chỉ áp sát mục tiêu nếu đây là đòn tấn công đầu tiên trong chuỗi combo
            MeleeSnapToTarget();
        characterCombat?.TryAttack();
    }

    //Hỗ trợ áp sát mục tiêu khi tấn công
    protected async void MeleeSnapToTarget()
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

        while (distanceToTarget > meleeSnapThreshold.x)
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

    public virtual bool CheckForNearEnemy()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position + transform.forward * ZoffsetCheckForNearEnemy, radiusCheckForNearEnemy);

        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Enemy"))
            {
                return true; // Có kẻ địch gần trước mặt
            }
        }
        return false;
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

