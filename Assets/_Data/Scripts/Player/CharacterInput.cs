using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterInput : MonoBehaviour
{
    [SerializeField] private Vector2 moveInput;
    public Vector2 MoveInput => moveInput;
    [SerializeField] private bool walk = false;
    public bool Walk => walk;
    [SerializeField] private bool sprint = false;
    public bool Sprint => sprint;
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

    private InputActions inputActions => InputManager.InputActions;


    private void Update()
    {
        if (inputActions == null)
        {
            Debug.LogWarning("InputActions is null in CharacterInput");
            return;
        }

        moveInput = inputActions.Keyboard.Move.ReadValue<Vector2>();
        walk = inputActions.Keyboard.Walk.IsPressed();
        if (inputActions.Keyboard.Sprint.triggered && !sprint)
        {
            sprint = true;
        }
        else if (inputActions.Keyboard.Sprint.triggered && sprint)
        {
            sprint = false;
        }

        dodge = inputActions.Keyboard.Dodge.triggered;
        jump = inputActions.Keyboard.Jump.triggered;
        wallJump = inputActions.Keyboard.Jump.triggered;
        attack = inputActions.Keyboard.Attack.triggered;
        healthRecovery = inputActions.Keyboard.HealthRecovery.triggered;
        lockTarget = inputActions.Keyboard.LockTarget.triggered;
        skill_1 = inputActions.Keyboard.Skill_1.triggered;
        skill_2 = inputActions.Keyboard.Skill_2.triggered;
    }
}