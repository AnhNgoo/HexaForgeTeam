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
public class CharacterBase : LoadComponents
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

    [Header("Character Effect General")]
    [SerializeField] protected GameObject effectsContainer;
    public GameObject punchEffect_1;
    public GameObject punchEffect_2;
    public GameObject punchEffect_3;
    public GameObject punchEffect_4;

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

        LoadEffects();
    }

    protected override void LoadComponentRuntime()
    {

    }

    protected virtual void LoadEffects()
    {
        if (effectsContainer == null)
            effectsContainer = transform.Find("Effects")?.gameObject;
        if (effectsContainer == null)
            return;

        if (punchEffect_1 == null)
            punchEffect_1 = effectsContainer.transform.Find("PunchEffect_1")?.gameObject;
        if (punchEffect_2 == null)
            punchEffect_2 = effectsContainer.transform.Find("PunchEffect_2")?.gameObject;
        if (punchEffect_3 == null)
            punchEffect_3 = effectsContainer.transform.Find("PunchEffect_3")?.gameObject;
        if (punchEffect_4 == null)
            punchEffect_4 = effectsContainer.transform.Find("PunchEffect_4")?.gameObject;
    }
    #region Init Character

    //Test
    protected override void Awake()
    {
        base.Awake();
        characterAnimation.Init(characterVisual);
        AddAnimationEvents();
        Init(characterData);
    }
    [Button("Init Character Data")]
    protected virtual void Init(CharacterData data)
    {
        if (data != null)
            characterData = Instantiate(data);

        characterLockTarget.SetFollowTarget();
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
    protected virtual async void OnAttack()
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