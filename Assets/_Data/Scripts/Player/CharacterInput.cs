using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterInput : MonoBehaviour
{
    public bool isMoving = false;
    public Vector2 moveInput;
    [SerializeField] private bool dodge = false;
    public bool Dodge => dodge;
    [SerializeField] private bool jump = false;
    public bool Jump => jump;
    [SerializeField] private bool wallJump = false;
    public bool WallJump => wallJump;
    [SerializeField] private bool attack = false;
    public bool Attack => attack;
    [SerializeField] private bool healthRecovery = false;
    public bool HealthRecovery => healthRecovery;
    [SerializeField] private bool lockTarget = false;
    public bool LockTarget => lockTarget;
    [SerializeField] private bool skill_1 = false;
    public bool Skill_1 => skill_1;
    [SerializeField] private bool skill_2 = false;
    public bool Skill_2 => skill_2;

    public void Init()
    {
        EventManager.Subscribe(GameEvent.OnMovement, OnMovement);
        EventManager.Subscribe(GameEvent.OnDodge, OnDodge);
        EventManager.Subscribe(GameEvent.OnJump, OnJump);
        EventManager.Subscribe(GameEvent.OnWallJump, OnWallJump);
        EventManager.Subscribe(GameEvent.OnAttack, OnAttack);
        EventManager.Subscribe(GameEvent.OnLockTarget, OnLockTarget);
        EventManager.Subscribe(GameEvent.OnHealthRecovery, OnHealthRecovery);
        EventManager.Subscribe(GameEvent.OnSkill_1, OnSkill_1);
        EventManager.Subscribe(GameEvent.OnSkill_2, OnSkill_2);
    }

    public void Reset()
    {
        EventManager.Unsubscribe(GameEvent.OnMovement, OnMovement);
        EventManager.Unsubscribe(GameEvent.OnDodge, OnDodge);
        EventManager.Unsubscribe(GameEvent.OnJump, OnJump);
        EventManager.Unsubscribe(GameEvent.OnWallJump, OnWallJump);
        EventManager.Unsubscribe(GameEvent.OnAttack, OnAttack);
        EventManager.Unsubscribe(GameEvent.OnLockTarget, OnLockTarget);
        EventManager.Unsubscribe(GameEvent.OnHealthRecovery, OnHealthRecovery);
        EventManager.Unsubscribe(GameEvent.OnSkill_1, OnSkill_1);
        EventManager.Unsubscribe(GameEvent.OnSkill_2, OnSkill_2);
    }


    private void LateUpdate()
    {
        // Reset các input sau khi đã được xử lý trong Update của CharacterBase
        dodge = false;
        jump = false;
        wallJump = false;
        attack = false;
        healthRecovery = false;
        lockTarget = false;
        skill_1 = false;
        skill_2 = false;
    }

    private void OnMovement(object obj)
    {
        if (obj is not Vector2 moveInput) return;

        this.moveInput = moveInput;
        isMoving = moveInput != Vector2.zero;
    }
    private void OnDodge(object obj)
    {
        dodge = true;
    }

    private void OnJump(object obj)
    {
        jump = true;
    }

    private void OnWallJump(object obj)
    {
        wallJump = true;
    }

    private void OnAttack(object obj)
    {
        attack = true;
    }

    private void OnLockTarget(object obj)
    {
        lockTarget = true;
    }

    private void OnHealthRecovery(object obj)
    {
        healthRecovery = true;
    }

    private void OnSkill_1(object obj)
    {
        skill_1 = true;
    }

    private void OnSkill_2(object obj)
    {
        skill_2 = true;
    }
}
