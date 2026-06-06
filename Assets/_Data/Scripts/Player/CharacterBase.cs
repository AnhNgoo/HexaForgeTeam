using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Cysharp.Threading.Tasks;

[RequireComponent(typeof(CharacterAnimation))]
[RequireComponent(typeof(CharacterMovement))]
[RequireComponent(typeof(CharacterRotate))]
[RequireComponent(typeof(CharacterWeapon))]
[RequireComponent(typeof(CharacterCombat))]
[RequireComponent(typeof(CharacterLockTarget))]
[RequireComponent(typeof(CharacterSkill))]
[RequireComponent(typeof(CharacterInput))]
public abstract class CharacterBase : LoadComponents
{
    [Header("Character Data")]
    [SerializeField] protected CharacterData characterData;
    public CharacterData CharacterData => characterData;

    [Header("Character Models")]
    [SerializeField] protected GameObject visuals;
    [SerializeField] protected GameObject characterVisual;

    [Header("Character Components")]
    [SerializeField] protected CharacterAnimation characterAnimation;
    public CharacterAnimation CharacterAnimation => characterAnimation;
    [SerializeField] protected CharacterMovement characterMovement;
    public CharacterMovement CharacterMovement => characterMovement;
    [SerializeField] protected CharacterRotate characterRotate;
    public CharacterRotate CharacterRotate => characterRotate;
    [SerializeField] protected CharacterWeapon characterWeapon;
    public CharacterWeapon CharacterWeapon => characterWeapon;
    [SerializeField] protected CharacterCombat characterCombat;
    public CharacterCombat CharacterCombat => characterCombat;
    [SerializeField] protected CharacterLockTarget characterLockTarget;
    public CharacterLockTarget CharacterLockTarget => characterLockTarget;
    [SerializeField] protected CharacterSkill characterSkill;
    public CharacterSkill CharacterSkill => characterSkill;
    [SerializeField] protected CharacterInput characterInput;
    public CharacterInput CharacterInput => characterInput;

    [Header("Character Effect General")]
    [SerializeField] protected GameObject effectPoints;
    public GameObject punchEffectPoint_1;
    public PoolType punchEffect_1 = PoolType.HitEffect_1;
    public GameObject punchEffectPoint_2;
    public PoolType punchEffect_2 = PoolType.HitEffect_2;
    public GameObject punchEffectPoint_3;
    public PoolType punchEffect_3 = PoolType.HitEffect_2;
    public GameObject punchEffectPoint_4;
    public PoolType punchEffect_4 = PoolType.HitEffect_2;

    [Header("Character Base Settings")]
    [SerializeField] protected float attackSpeedMultiplier = 0.01f;
    [SerializeField] protected string attackParameterName = "AttackSpeed";
    protected StateController stateController;
    public StateController StateController => stateController;
    private Cooldown dodgeCooldown = new Cooldown();
    public bool IsHealthRecovering { get; set; } = false;

    protected override void LoadComponent()
    {
        if (characterAnimation == null)
            characterAnimation = GetComponent<CharacterAnimation>();
        if (characterMovement == null)
            characterMovement = GetComponent<CharacterMovement>();
        if (characterRotate == null)
            characterRotate = GetComponent<CharacterRotate>();
        if (visuals == null)
            visuals = transform.Find("Visuals").gameObject;
        if (characterWeapon == null)
            characterWeapon = GetComponent<CharacterWeapon>();
        if (characterCombat == null)
            characterCombat = GetComponent<CharacterCombat>();
        if (characterLockTarget == null)
            characterLockTarget = GetComponent<CharacterLockTarget>();
        if (characterSkill == null)
            characterSkill = GetComponent<CharacterSkill>();
        if (characterInput == null)
            characterInput = GetComponent<CharacterInput>();
        LoadEffectPoints();
    }

    protected override void LoadComponentRuntime()
    {

    }

    protected virtual void LoadEffectPoints()
    {
        if (effectPoints == null)
            effectPoints = transform.Find("EffectPoints")?.gameObject;
        if (effectPoints == null)
            return;

        if (punchEffectPoint_1 == null)
            punchEffectPoint_1 = effectPoints?.transform.Find("PunchEffectPoint_1")?.gameObject;
        if (punchEffectPoint_2 == null)
            punchEffectPoint_2 = effectPoints?.transform.Find("PunchEffectPoint_2")?.gameObject;
        if (punchEffectPoint_3 == null)
            punchEffectPoint_3 = effectPoints?.transform.Find("PunchEffectPoint_3")?.gameObject;
        if (punchEffectPoint_4 == null)
            punchEffectPoint_4 = effectPoints?.transform.Find("PunchEffectPoint_4")?.gameObject;
    }
    #region Init Character

    //Test
    protected override void Awake()
    {
        base.Awake();
        Init(characterData);
    }
    [Button("Init Character Data")]
    protected virtual void Init(CharacterData data)
    {
        if (data != null)
            characterData = Instantiate(data);

        characterInput.Init();
        characterAnimation.Init(characterVisual);
        characterLockTarget.SetFollowTarget();
        characterCombat?.Init(this, InitAttackCombos(), InitPunchCombos());
        InitSkills();
    }

    // Điều chỉnh tốc độ animation tấn công dựa trên tốc độ tấn công của character
    public virtual void SetAttackSpeed(float speed)
    {
        characterAnimation.SetAnimationSpeed(attackParameterName, speed * attackSpeedMultiplier);
    }
    #endregion
    protected virtual void Start()
    {
        stateController = new StateController();
        stateController.ChangeState(new IdleState(this));
    }

    protected virtual void Update()
    {
        if (!CheckAnyStateTransition())
            stateController?.currentState?.Update();

        characterMovement.SetMoveDirection(characterInput.moveInput);
        if (characterInput.LockTarget)
            OnLockTarget();
    }

    protected virtual void FixedUpdate()
    {
        stateController?.currentState?.FixedUpdate();
    }

    protected virtual bool CheckAnyStateTransition()
    {
        //Chuyển về FallState nếu đang ở trên không và bắt đầu rơi
        if (!CharacterMovement.IsGrounded && CharacterMovement.CC.velocity.y < CharacterMovement.FallThreshold)
        {
            StateController.ChangeState(new FallState(this));
            return true;
        }


        return false;
    }
    public virtual void Dodge()
    {
        if (dodgeCooldown.IsOnCooldown)
            return;

        dodgeCooldown.StartCooldown(characterMovement.DodgeCooldown);

        stateController.ChangeState(new DodgeState(this));
    }

    #region Attack 
    public virtual void Attack()
    {
        characterCombat?.TryAttack();
    }

    /// <summary>
    /// Khởi tạo các đòn tấn công, bắt buộc phải override
    /// </summary>
    /// <returns></returns>
    protected virtual IAttackStep[] InitAttackCombos()
    {
        return null;
    }

    // Dùng để set punch combo khác cho nhân vật cụ thể
    protected virtual IAttackStep[] InitPunchCombos()
    {
        return null;
    }
    #endregion

    #region Skill
    protected virtual void InitSkills()
    {
        characterSkill?.Init(this, GetSkill_1(), GetSkill_2());
    }

    protected virtual ICharacterSkill GetSkill_1()
    {
        return null;
    }

    protected virtual ICharacterSkill GetSkill_2()
    {
        return null;
    }

    public virtual void Skill_1()
    {
        characterSkill?.UseSkill1();
    }

    public virtual void Skill_2()
    {
        characterSkill?.UseSkill2();
    }

    #endregion
    protected virtual void OnLockTarget()
    {
        if (CharacterLockTarget == null)
            return;
        CharacterLockTarget.ToggleLockTarget();
    }

    //Nhìn về phía mục tiêu khi đang khóa mục tiêu
    public virtual void LookAtTarget()
    {
        if (!characterLockTarget.IsLockingTarget)
            return;
        characterRotate.LookAt(characterLockTarget.Target.position);
    }


    #region AddAnimationEvents
    protected virtual void AddAnimationEvents()
    {
    }
    #endregion
}