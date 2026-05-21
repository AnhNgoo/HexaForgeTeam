using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HUDMenuTest : MenuBase
{
    public override MenuType menuType => MenuType.HUDMenuTest;

    [SerializeField] private Joystick joystick;
    [SerializeField] private EventTouch btn_Dodge;
    [SerializeField] private EventTouch btn_Jump;
    [SerializeField] private EventTouch btn_Attack;
    [SerializeField] private EventTouch btn_LockTarget;
    [SerializeField] private EventTouch btn_HealthRecovery;


    protected override void LoadComponent()
    {
        if (joystick == null)
            joystick = transform.Find("Joystick").GetComponent<Joystick>();
        if (btn_Dodge == null)
            btn_Dodge = transform.Find("Btn_Dodge").GetComponent<EventTouch>();
        if (btn_Jump == null)
            btn_Jump = transform.Find("Btn_Jump").GetComponent<EventTouch>();
        if (btn_Attack == null)
            btn_Attack = transform.Find("Btn_Attack").GetComponent<EventTouch>();
        if (btn_LockTarget == null)
            btn_LockTarget = transform.Find("Btn_LockTarget").GetComponent<EventTouch>();
        if (btn_HealthRecovery == null)
            btn_HealthRecovery = transform.Find("Btn_HealthRecovery").GetComponent<EventTouch>();
    }

    protected override void LoadComponentRuntime()
    {

    }

    public override void Open(object data = null)
    {
        base.Open(data);
        btn_Dodge.onPointerDown.AddListener(OnDodgeButtonClicked);
        btn_Jump.onPointerDown.AddListener(OnJumpButtonClicked);
        btn_Attack.onPointerDown.AddListener(OnAttackButtonClicked);
        btn_LockTarget.onPointerDown.AddListener(OnLockTargetButtonClicked);
        btn_HealthRecovery.onPointerDown.AddListener(OnHealthRecoveryButtonClicked);
    }
    public override void Close()
    {
        base.Close();
        btn_Dodge.onPointerDown.RemoveListener(OnDodgeButtonClicked);
        btn_Jump.onPointerDown.RemoveListener(OnJumpButtonClicked);
        btn_Attack.onPointerDown.RemoveListener(OnAttackButtonClicked);
        btn_LockTarget.onPointerDown.RemoveListener(OnLockTargetButtonClicked);
        btn_HealthRecovery.onPointerDown.RemoveListener(OnHealthRecoveryButtonClicked);
    }

    private void Update()
    {
        EventManager.Instance?.Notify(GameEvent.OnMovement, joystick.Direction);
    }

    private void OnDodgeButtonClicked()
    {
        EventManager.Instance?.Notify(GameEvent.OnDodge);
    }

    private void OnJumpButtonClicked()
    {
        EventManager.Instance?.Notify(GameEvent.OnJump);
        EventManager.Instance?.Notify(GameEvent.OnWallJump);
    }
    private void OnAttackButtonClicked()
    {
        EventManager.Instance?.Notify(GameEvent.OnAttack);
    }
    private void OnLockTargetButtonClicked()
    {
        EventManager.Instance?.Notify(GameEvent.OnLockTarget);
    }

    private void OnHealthRecoveryButtonClicked()
    {
        EventManager.Instance?.Notify(GameEvent.OnHealthRecovery);
    }

}
