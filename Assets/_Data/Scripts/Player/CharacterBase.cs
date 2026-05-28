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

    public Vector2 JoystickInput { get; private set; } // Lưu trữ input từ joystick
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

        characterAnimation.Init(characterVisual);
        AddAnimationEvents();
        characterLockTarget.SetFollowTarget();
        InitSkills();
    }

    // Điều chỉnh tốc độ animation tấn công dựa trên tốc độ tấn công của character
    public virtual void SetAttackSpeed(float speed)
    {
        characterAnimation.SetAnimationSpeed(attackParameterName, speed * attackSpeedMultiplier);
    }
    #endregion
    protected virtual void OnEnable()
    {
        EventManager.Instance?.Subscribe(GameEvent.OnMovement, OnMovement);
        EventManager.Instance?.Subscribe(GameEvent.OnDodge, _ => OnDodge());
        EventManager.Instance?.Subscribe(GameEvent.OnJump, _ => OnJump());
        EventManager.Instance?.Subscribe(GameEvent.OnWallJump, _ => OnWallJump());
        EventManager.Instance?.Subscribe(GameEvent.OnAttack, _ => OnAttack());
        EventManager.Instance?.Subscribe(GameEvent.OnLockTarget, _ => OnLockTarget());
        EventManager.Instance?.Subscribe(GameEvent.OnHealthRecovery, _ => OnHealthRecovery());
        EventManager.Instance?.Subscribe(GameEvent.OnSkill_1, _ => OnSkill_1());
    }

    protected virtual void OnDisable()
    {
        EventManager.Instance?.Unsubscribe(GameEvent.OnMovement, OnMovement);
        EventManager.Instance?.Unsubscribe(GameEvent.OnDodge, _ => OnDodge());
        EventManager.Instance?.Unsubscribe(GameEvent.OnJump, _ => OnJump());
        EventManager.Instance?.Unsubscribe(GameEvent.OnWallJump, _ => OnWallJump());
        EventManager.Instance?.Unsubscribe(GameEvent.OnAttack, _ => OnAttack());
        EventManager.Instance?.Unsubscribe(GameEvent.OnLockTarget, _ => OnLockTarget());
        EventManager.Instance?.Unsubscribe(GameEvent.OnHealthRecovery, _ => OnHealthRecovery());
        EventManager.Instance?.Unsubscribe(GameEvent.OnSkill_1, _ => OnSkill_1());
    }



    protected virtual void Start()
    {
        stateController = new StateController();
        stateController.ChangeState(new IdleState(this));
    }

    protected virtual void Update()
    {
        stateController?.currentState?.Update();
    }

    protected virtual void FixedUpdate()
    {
        stateController?.currentState?.FixedUpdate();
    }

    protected virtual void OnMovement(object obj)
    {
        if (obj is not Vector2 direction) return;

        JoystickInput = direction;
        characterMovement.SetMoveDirection(direction);
    }

    protected virtual void OnDodge()
    {
        if (dodgeCooldown.IsOnCooldown ||
             !characterMovement.IsGrounded ||
             characterMovement.IsDodging ||
             characterMovement.JumpLanding ||
            characterCombat.IsAttacking ||
            IsHealthRecovering ||
            characterMovement.IsLunging
            )
            return;

        dodgeCooldown.StartCooldown(characterMovement.DodgeCooldown);

        if (!characterMovement.IsDodging)
            stateController.ChangeState(new DodgeState(this));
    }

    protected virtual void OnJump()
    {
        if (!characterMovement.IsGrounded ||
             characterMovement.IsDodging ||
             characterMovement.JumpLanding ||
             characterCombat.IsAttacking ||
             IsHealthRecovering)
            return;

        stateController.ChangeState(new JumpState(this));
    }

    private void OnWallJump()
    {
        if (!characterMovement.WallEdge ||
             characterMovement.IsDodging ||
             characterMovement.JumpLanding ||
             characterCombat.IsAttacking ||
             !characterMovement.CanWallJump ||
             IsHealthRecovering)
            return;

        stateController.ChangeState(new WallJumpState(this));
    }

    private void OnHealthRecovery()
    {
        if (IsHealthRecovering ||
             characterMovement.IsDodging ||
             characterMovement.JumpLanding ||
             characterCombat.IsAttacking ||
             !characterMovement.IsGrounded)
            return;

        stateController.ChangeState(new HealthRecoveryState(this));
    }
    #region Attack 
    protected virtual void OnAttack()
    {
        characterCombat?.TryAttack();
    }

    public virtual bool CheckConditionAttack()
    {
        if (characterMovement.IsDodging ||
             characterMovement.JumpLanding ||
             characterMovement.CC.velocity.y < characterMovement.FallThreshold ||
             IsHealthRecovering)
            return false;
        return true;
    }

    /// <summary>
    /// Khởi tạo các đòn tấn công, bắt buộc phải override
    /// </summary>
    /// <returns></returns>
    protected virtual IAttackStep[] InitAttackCombos()
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

    protected virtual void OnSkill_1()
    {
        characterSkill?.UseSkill1();
    }

    #endregion
    protected virtual void OnLockTarget()
    {
        if (CharacterLockTarget == null)
            return;
        CharacterLockTarget.ToggleLockTarget();
    }

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