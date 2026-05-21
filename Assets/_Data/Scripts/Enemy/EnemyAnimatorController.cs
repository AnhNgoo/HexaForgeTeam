using UnityEngine;

public class EnemyAnimatorController : MonoBehaviour
{
    private EnemyBase _enemyBase;
    [SerializeField] private Animator _animator;
    public Animator Animator => _animator;

    [Header("Animations StringToHash")]
    [SerializeField] private int idleHash;
    [SerializeField] private int chaseHash;
    [SerializeField] private int staggerHash;
    [SerializeField] private int dieHash;

    #region Getters
    public int IdleHash => idleHash;
    public int ChaseHash => chaseHash;
    public int StaggerHash => staggerHash;
    public int DieHash => dieHash;
    #endregion

    public void Initialize(EnemyBase enemyBase)
    {
        _enemyBase = enemyBase;
        if (_animator == null) _animator = GetComponent<Animator>();
        idleHash = Animator.StringToHash("Idle");
        chaseHash = Animator.StringToHash("Chase");
        dieHash = Animator.StringToHash("Die");
        staggerHash = Animator.StringToHash("Take Damage");
        Debug.Log($"{gameObject.name} - EnemyAnimatorController đã được khởi tạo!");
    }

    public void OnValidate()
    {
        if (_animator == null)
        {
            _animator = GetComponentInChildren<Animator>();
            if (_animator == null)
            {
                Debug.LogError("Animator component is missing on " + gameObject.name);
            }
        }
    }

    public void PlayAttackAnimation(AttackDataSO attackData)
    {
        if (_animator != null && attackData != null)
        {
            _animator.CrossFadeInFixedTime(attackData.animationStateName, attackData.transitionDuration);
        }
    }

    public void PlayAnimation(int animationHash, float transitionDuration = 0.1f)
    {
        if (_animator != null)
        {
            _animator.CrossFadeInFixedTime(animationHash, transitionDuration);
        }
    }

    public void OpenHitBox()
    {
        _enemyBase.Combat.OpenHitbox(); //Gọi hàm mở hitbox từ EnemyCombat để đảm bảo rằng hitbox sẽ được kích hoạt đúng thời điểm, tránh lỗi hitbox không mở khi animation tấn công đang diễn ra
    }

    public void CloseHitBox()
    {
        _enemyBase.Combat.CloseHitbox(); //Gọi hàm đóng hitbox từ EnemyCombat để đảm bảo rằng hitbox sẽ được đóng đúng thời điểm, tránh lỗi hitbox vẫn mở sau khi animation kết thúc
    }
}
