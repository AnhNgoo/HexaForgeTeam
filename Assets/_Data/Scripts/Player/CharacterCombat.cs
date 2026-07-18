using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;
public class CharacterCombat : LoadComponents
{
    [Header("Combat Settings")]
    [SerializeField] private float cooldownTime = 0f; //Thời gian chờ giữa các đòn tấn công
    [SerializeField] private float nextAttackTime = 0.9f; //Thời gian xong animation để thực hiện đòn tấn công tiếp theo
    [SerializeField] private float finalAttackTime = 0.9f; //Thời gian xong animation đòn cuối để reset combo
    public float NextAttackTime => nextAttackTime;
    [SerializeField] private float comboResetDelay = 0.4f; //Thời gian đợi xem player có đánh tiếp không

    [SerializeField] private IAttackStep[] weaponCombos;
    [SerializeField] private IAttackStep[] punchCombos;

    public int CurrentComboIndex => currentComboIndex;
    public bool IsAttacking { get; set; } = false;
    public bool CanAttack { get; set; } = true;
    private Coroutine comboCoroutine;
    private Cooldown cooldownAttackTimer = new Cooldown();
    private CharacterBase characterBase;

    private int currentComboIndex = 0; //Chỉ số đòn tấn công hiện tại trong chuỗi combo
    private bool isComboWindowOpen = false;
    protected override void LoadComponent()
    {
    }

    protected override void LoadComponentRuntime()
    {

    }

    public void Init(CharacterBase character, IAttackStep[] weaponCombos, IAttackStep[] punchCombos = null)
    {
        characterBase = character;
        this.weaponCombos = weaponCombos;
        InitPunchCombos(punchCombos);
    }

    public void SetWeaponCombos(IAttackStep[] combos)
    {
        weaponCombos = combos;
    }

    public void TryAttack(bool canMove = false, int layerIndex = 0)
    {
        if (characterBase == null)
            return;

        if (cooldownAttackTimer.IsOnCooldown)
            return;

        if (!CanAttack)
            return;

        if (!IsAttacking)
            characterBase.StateController.ChangeState(new CombatState(characterBase, canMove));

        IAttackStep[] activeCombos = GetActiveCombos();
        if (activeCombos == null || activeCombos.Length == 0)
            return;

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

        comboCoroutine = StartCoroutine(AttackRoutine(characterBase, activeCombos, currentComboIndex, layerIndex));
    }

    private IEnumerator AttackRoutine(CharacterBase character, IAttackStep[] combos, int comboIndex, int layerIndex = 0)
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

        Animator animator = character.CharacterAnimation.Animator;
        if (animator == null || !animator.isActiveAndEnabled || animator.runtimeAnimatorController == null)
        {
            ResetCombo();
            yield break;
        }

        // Đợi cho đến khi animation bắt đầu
        yield return new WaitUntil(() =>
            !animator.IsInTransition(layerIndex) &&
            animator.GetCurrentAnimatorStateInfo(layerIndex).IsName(attackStep.AttackStateName));


        //Nếu đòn cuối thì chạy full animation
        float animationLength = comboIndex == combos.Length - 1 ? finalAttackTime : nextAttackTime;
        // Đợi cho đến khi animation hoàn thành
        yield return new WaitUntil(() =>
             character.CharacterAnimation.GetAnimationTime(attackStep.AttackStateName, layerIndex) > animationLength &&
                   !animator.IsInTransition(layerIndex)
        );

        //Chuyển về trạng thái idle và mở cửa sổ combo để có thể tiếp tục chuỗi combo
        isComboWindowOpen = true;
        CanAttack = true;
        IsAttacking = false;

        characterBase.StateController.ChangeState(new IdleState(characterBase));

        if (comboIndex == combos.Length - 1)
            cooldownAttackTimer.StartCooldown(cooldownTime);

        // Đợi thêm một khoảng thời gian để xem người chơi có đánh tiếp không, nếu không thì reset combo
        float delay = comboIndex == combos.Length - 1 ? 0 : comboResetDelay; // Nếu là đòn cuối thì không cần delay nữa
        yield return new WaitForSeconds(delay);

        ResetCombo();
    }

    public void ResetCombo()
    {
        isComboWindowOpen = false;
        CanAttack = true;
        IsAttacking = false;
        currentComboIndex = 0;
    }
    // Lấy combo đang sử dụng, ưu tiên combo vũ khí nếu có, nếu không thì dùng combo tay không
    private IAttackStep[] GetActiveCombos()
    {
        if (characterBase.CharacterWeapon == null || characterBase.CharacterWeapon.CurrentWeapon == null)
        {
            return punchCombos;
        }

        return weaponCombos;
    }

    //Khởi tạo combo tay không
    private void InitPunchCombos(IAttackStep[] combos = null)
    {
        //Nếu có punch combo riêng thì dùng, nếu không thì khởi tạo combo tay không mặc định
        punchCombos = combos ?? new IAttackStep[4]
        {
            new PunchStep_1(characterBase),
            new PunchStep_2(characterBase),
            new PunchStep_3(characterBase),
            new PunchStep_4(characterBase)
        };
    }
}
