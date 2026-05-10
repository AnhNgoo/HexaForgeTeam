using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Cysharp.Threading.Tasks;

public class CharacterBase : LoadComponents
{
    [SerializeField] protected CharacterData characterData;
    public CharacterData CharacterData => characterData;
    [SerializeField] protected GameObject visuals;
    [SerializeField] protected CharacterAnimation characterAnimation;
    public CharacterAnimation CharacterAnimation => characterAnimation;
    [SerializeField] protected CharacterMovement characterMovement;
    public CharacterMovement CharacterMovement => characterMovement;
    [SerializeField] protected CharacterRotate characterRotate;
    public CharacterRotate CharacterRotate => characterRotate;



    protected StateController stateController;
    public StateController StateController => stateController;

    public Vector2 JoystickInput { get; private set; } // Lưu trữ input từ joystick
    private Cooldown dodgeCooldown = new Cooldown();
    public bool IsAttacking { get; set; } = false;
    public bool CanAttack { get; set; } = true;

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
    }

    protected override void LoadComponentRuntime()
    {

    }

    [Button("Init Character Data")]
    protected virtual void Init(CharacterData data)
    {
        characterData = Instantiate(data);
    }
    protected virtual void OnEnable()
    {
        EventManager.Instance?.Subscribe(GameEvent.OnMovement, OnMovement);
        EventManager.Instance?.Subscribe(GameEvent.OnDodge, _ => OnDodge());
        EventManager.Instance?.Subscribe(GameEvent.OnJump, _ => OnJump());
        EventManager.Instance?.Subscribe(GameEvent.OnAttack, _ => OnAttack());
    }

    protected virtual void OnDisable()
    {
        EventManager.Instance?.Unsubscribe(GameEvent.OnMovement, OnMovement);
        EventManager.Instance?.Unsubscribe(GameEvent.OnDodge, _ => OnDodge());
        EventManager.Instance?.Unsubscribe(GameEvent.OnJump, _ => OnJump());
        EventManager.Instance?.Unsubscribe(GameEvent.OnAttack, _ => OnAttack());
    }


    protected virtual void Start()
    {
        stateController = new StateController();
        stateController.ChangeState(new IdleState(this));
    }

    protected virtual void Update()
    {
        stateController?.currentState?.Update();
        characterMovement.CheckGrounded();
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

    #region Attack
    protected virtual void OnAttack()
    {

    }

    protected virtual bool CheckConditionAttack()
    {
        if (!characterMovement.IsGrounded ||
             characterMovement.IsDodging ||
             characterMovement.JumpLanding)
            return false;
        return true;
    }
    #endregion
}
