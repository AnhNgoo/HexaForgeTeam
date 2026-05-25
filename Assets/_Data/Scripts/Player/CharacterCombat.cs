using System.Collections;
using UnityEngine;

public class CharacterCombat : LoadComponents
{
    [SerializeField] private bool debugMode = true;
    [Header("Attack Hitbox Settings")]
    [SerializeField] private float forwardAttackOffset = 1.5f;
    [SerializeField] private float attackHitBoxRadius = 1f;
    [Header("Combat Settings")]
    [SerializeField] private float nextAttackTime = 1f; //Thời gian xong animation để thực hiện đòn tấn công tiếp theo
    public float NextAttackTime => nextAttackTime;
    [SerializeField] private float comboResetDelay = 0.4f; //Thời gian đợi xem player có đánh tiếp không
    [SerializeField] private float finalAttackTime = 1f; //Thời gian xong animation đòn cuối để reset combo

    private CharacterBase characterBase;
    private IAttackStep[] weaponCombos;
    private IAttackStep[] punchCombos;
    private int currentComboIndex = 0; //Chỉ số đòn tấn công hiện tại trong chuỗi combo
    private bool isComboWindowOpen = false;
    public bool IsAttacking { get; set; } = false;
    public bool CanAttack { get; set; } = true;
    public bool FirstAttack => currentComboIndex == 0; //Đòn tấn công đầu tiên trong chuỗi combo
    private Coroutine comboCoroutine;
    private Cooldown cooldownAttackTimer = new Cooldown();

    protected override void LoadComponent()
    {
    }

    protected override void LoadComponentRuntime()
    {

    }

    public void Init(CharacterBase character, IAttackStep[] combos)
    {
        characterBase = character;
        weaponCombos = combos;
        InitPunchCombos();
    }

    public void SetWeaponCombos(IAttackStep[] combos)
    {
        weaponCombos = combos;
    }

    public void TryAttack()
    {
        if (characterBase == null)
            return;

        if (cooldownAttackTimer.IsOnCooldown)
            return;

        if (!characterBase.CheckConditionAttack())
            return;

        if (!CanAttack)
            return;

        if (!IsAttacking)
            characterBase.StateController.ChangeState(new AttackState(characterBase));

        IAttackStep[] activeCombos = GetActiveCombos();
        if (activeCombos == null || activeCombos.Length == 0)
            return;

        CanAttack = false;
        if (isComboWindowOpen)
        {
            currentComboIndex++;
            if (currentComboIndex >= activeCombos.Length)
            {
                currentComboIndex = 0;
            }
        }
        else
        {
            currentComboIndex = 0;
        }

        if (comboCoroutine != null)
        {
            StopCoroutine(comboCoroutine);
            comboCoroutine = null;
        }

        comboCoroutine = StartCoroutine(AttackRoutine(characterBase, activeCombos, currentComboIndex));
    }

    private IEnumerator AttackRoutine(CharacterBase character, IAttackStep[] combos, int comboIndex)
    {
        if (combos == null || combos.Length == 0)
            yield break;

        comboIndex = Mathf.Clamp(comboIndex, 0, combos.Length - 1);
        IAttackStep attackStep = combos[comboIndex];

        CanAttack = false;
        IsAttacking = true;
        isComboWindowOpen = false;

        //Thực hiện đòn tấn công
        attackStep.Attack(character);

        // Đợi cho đến khi animation bắt đầu
        yield return new WaitUntil(() =>
            !character.CharacterAnimation.Animator.IsInTransition(0) &&
            character.CharacterAnimation.Animator.GetCurrentAnimatorStateInfo(0).IsName(attackStep.AttackStateName));

        //Nếu đòn cuối thì chạy full animation
        float animationLength = comboIndex == combos.Length - 1 ? finalAttackTime : nextAttackTime;
        // Đợi cho đến khi animation hoàn thành
        yield return new WaitUntil(() =>
        {
            return character.CharacterAnimation.GetAnimationTime(attackStep.AttackStateName) > animationLength &&
                   !character.CharacterAnimation.Animator.IsInTransition(0);
        });

        //Chuyển về trạng thái idle và mở cửa sổ combo để có thể tiếp tục chuỗi combo
        isComboWindowOpen = true;
        CanAttack = true;
        IsAttacking = false;
        characterBase.StateController.ChangeState(new IdleState(characterBase));

        // Đợi thêm một khoảng thời gian để xem người chơi có đánh tiếp không, nếu không thì reset combo
        float delay = comboIndex == combos.Length - 1 ? finalAttackTime : comboResetDelay;
        yield return new WaitForSeconds(delay);

        isComboWindowOpen = false;
    }

    // Lấy combo đang sử dụng, ưu tiên combo vũ khí nếu có, nếu không thì dùng combo tay không
    private IAttackStep[] GetActiveCombos()
    {
        if (characterBase.CharacterWeapon == null || characterBase.CharacterWeapon.CurrentWeapon == null)
            return punchCombos;

        return weaponCombos;
    }

    // Khởi tạo combo tay không
    private void InitPunchCombos()
    {
        if (punchCombos != null && punchCombos.Length > 0)
            return;

        punchCombos = new IAttackStep[4]
        {
            new PunchStep_1(characterBase),
            new PunchStep_2(characterBase),
            new PunchStep_3(characterBase),
            new PunchStep_4(characterBase)
        };
    }

    #region Melee

    //Bật hixbox tấn công
    public void AttackHitBox()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position + transform.forward * forwardAttackOffset, attackHitBoxRadius);

        foreach (Collider hitCollider in hitColliders)
        {
            AttackHandler(hitCollider);
        }
    }

    // Xử lý logic khi đòn tấn công chạm trúng đối tượng
    private void AttackHandler(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            DebugNote.Green("Xử lý va chạm với enemy ở đây, Hit Enemy: " + other.name);
            CameraShake.Instance.Shake();
        }
    }
    #endregion

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!debugMode)
            return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.forward * forwardAttackOffset, attackHitBoxRadius);
    }
#endif
}
