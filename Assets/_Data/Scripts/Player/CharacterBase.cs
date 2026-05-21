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
[RequireComponent(typeof(CharacterCamera))]
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
    [SerializeField] protected CharacterCamera characterCamera;
    public CharacterCamera CharacterCamera => characterCamera;

    [Header("Character Base Settings")]
    [SerializeField] protected float attackSpeedMultiplier = 0.01f;
    [SerializeField] protected string attackParameterName = "AttackSpeed";
    protected StateController stateController;
    public StateController StateController => stateController;

    public Vector2 JoystickInput { get; private set; } // Lưu trữ input từ joystick
    private Cooldown dodgeCooldown = new Cooldown();
    public bool IsAttacking { get; set; } = false;
    public bool CanAttack { get; set; } = true;
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
        if (characterCamera == null)
            characterCamera = GetComponent<CharacterCamera>();
    }

    protected override void LoadComponentRuntime()
    {

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
        characterCamera.SetFollowTarget();
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
             IsAttacking)
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
             IsAttacking)
            return;

        stateController.ChangeState(new JumpState(this));
    }

    private void OnWallJump()
    {
        if (!characterMovement.WallEdge ||
             characterMovement.IsDodging ||
             characterMovement.JumpLanding ||
             IsAttacking ||
             !characterMovement.CanWallJump)
            return;

        stateController.ChangeState(new WallJumpState(this));
    }


    private void OnHealthRecovery()
    {
        if (IsHealthRecovering ||
             characterMovement.IsDodging ||
             characterMovement.JumpLanding ||
             IsAttacking)
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
             characterMovement.CC.velocity.y < characterMovement.FallThreshold)
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
        if (CharacterCamera == null)
            return;
        CharacterCamera.ToggleLockTarget();
    }

    public virtual void LookAtTarget()
    {
        if (!characterCamera.IsLockingTarget)
            return;
        characterRotate.LookAt(characterCamera.LookAtTarget.position);
    }
}