// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;
// using UnityEngine.EventSystems;
// using UnityEngine.InputSystem;
// #if UNITY_EDITOR
// using UnityEditor;
// #endif

// public class HUDMenuTest : MenuBase
// {
//     public override MenuType menuType => MenuType.HUDMenuTest;

//     [SerializeField] private Joystick joystick;
//     [SerializeField] private EventTouch btn_Dodge;
//     [SerializeField] private EventTouch btn_Jump;
//     [SerializeField] private EventTouch btn_Attack;
//     [SerializeField] private EventTouch btn_LockTarget;
//     [SerializeField] private EventTouch btn_HealthRecovery;
//     [SerializeField] private EventTouch btn_Skill_1;
//     [SerializeField] private EventTouch btn_Skill_2;


//     protected override void LoadComponent()
//     {
//         if (joystick == null)
//             joystick = transform.Find("Joystick").GetComponent<Joystick>();
//         if (btn_Dodge == null)
//             btn_Dodge = transform.Find("Btn_Dodge").GetComponent<EventTouch>();
//         if (btn_Jump == null)
//             btn_Jump = transform.Find("Btn_Jump").GetComponent<EventTouch>();
//         if (btn_Attack == null)
//             btn_Attack = transform.Find("Btn_Attack").GetComponent<EventTouch>();
//         if (btn_LockTarget == null)
//             btn_LockTarget = transform.Find("Btn_LockTarget").GetComponent<EventTouch>();
//         if (btn_HealthRecovery == null)
//             btn_HealthRecovery = transform.Find("Btn_HealthRecovery").GetComponent<EventTouch>();
//         if (btn_Skill_1 == null)
//             btn_Skill_1 = transform.Find("Btn_Skill_1").GetComponent<EventTouch>();
//         if (btn_Skill_2 == null)
//             btn_Skill_2 = transform.Find("Btn_Skill_2").GetComponent<EventTouch>();
//     }

//     protected override void LoadComponentRuntime()
//     {

//     }

//     public override void Open(object data = null)
//     {
//         base.Open(data);
//         btn_Dodge.onPointerDown.AddListener(OnDodgeButtonClicked);
//         btn_Jump.onPointerDown.AddListener(OnJumpButtonClicked);
//         btn_Attack.onPointerDown.AddListener(OnAttackButtonClicked);
//         btn_LockTarget.onPointerDown.AddListener(OnLockTargetButtonClicked);
//         btn_HealthRecovery.onPointerDown.AddListener(OnHealthRecoveryButtonClicked);
//         btn_Skill_1.onPointerDown.AddListener(OnSkill_1ButtonClicked);
//         btn_Skill_2.onPointerDown.AddListener(OnSkill_2ButtonClicked);
//     }
//     public override void Close()
//     {
//         base.Close();
//         btn_Dodge.onPointerDown.RemoveListener(OnDodgeButtonClicked);
//         btn_Jump.onPointerDown.RemoveListener(OnJumpButtonClicked);
//         btn_Attack.onPointerDown.RemoveListener(OnAttackButtonClicked);
//         btn_LockTarget.onPointerDown.RemoveListener(OnLockTargetButtonClicked);
//         btn_HealthRecovery.onPointerDown.RemoveListener(OnHealthRecoveryButtonClicked);
//         btn_Skill_1.onPointerDown.RemoveListener(OnSkill_1ButtonClicked);
//         btn_Skill_2.onPointerDown.RemoveListener(OnSkill_2ButtonClicked);
//     }

//     private void Start()
//     {
//         EventManager.Subscribe(GameEvent.OnActiveSkill_1, OnActiveSkill_1);
//         EventManager.Subscribe(GameEvent.OnActiveSkill_2, OnActiveSkill_2);
//     }

//     private void OnDestroy()
//     {
//         EventManager.Unsubscribe(GameEvent.OnActiveSkill_1, OnActiveSkill_1);
//         EventManager.Unsubscribe(GameEvent.OnActiveSkill_2, OnActiveSkill_2);
//     }
//     private void Update()
//     {
// #if UNITY_EDITOR
//         if (!IsSimulatorFocused())
//         {
//             EventManager.Notify(GameEvent.OnMovement, Vector2.zero);
//             return;
//         }
// #endif

//         Vector2 move = joystick.Direction;

// #if UNITY_EDITOR || UNITY_STANDALONE
//         Vector2 keyboard = Vector2.zero;

//         if (Keyboard.current != null)
//         {
//             if (Keyboard.current.wKey.isPressed)
//                 keyboard.y = 1;

//             if (Keyboard.current.sKey.isPressed)
//                 keyboard.y = -1;

//             if (Keyboard.current.aKey.isPressed)
//                 keyboard.x = -1;

//             if (Keyboard.current.dKey.isPressed)
//                 keyboard.x = 1;
//         }

//         if (keyboard != Vector2.zero)
//         {
//             float speedMultiplier = 0.5f;

//             if (Keyboard.current.cKey.isPressed)
//                 speedMultiplier = 1f;
//             else if (Keyboard.current.vKey.isPressed)
//                 speedMultiplier = 0.2f;

//             move = keyboard * speedMultiplier;
//         }
// #endif

//         EventManager.Notify(GameEvent.OnMovement, move);
//     }

//     private bool IsSimulatorFocused()
//     {
// #if UNITY_EDITOR
//         EditorWindow focusedWindow = EditorWindow.focusedWindow;

//         if (focusedWindow == null)
//             return false;

//         string windowName = focusedWindow.titleContent.text;

//         return windowName.Contains("Simulator") ||
//                windowName.Contains("Device Simulator") ||
//                windowName.Contains("Game");
// #else
//     return true;
// #endif
//     }

//     private void OnDodgeButtonClicked()
//     {
//         EventManager.Notify(GameEvent.OnDodge);
//     }

//     private void OnJumpButtonClicked()
//     {
//         EventManager.Notify(GameEvent.OnJump);
//         EventManager.Notify(GameEvent.OnWallJump);
//     }
//     private void OnAttackButtonClicked()
//     {
//         EventManager.Notify(GameEvent.OnAttack);
//     }
//     private void OnLockTargetButtonClicked()
//     {
//         EventManager.Notify(GameEvent.OnLockTarget);
//     }

//     private void OnHealthRecoveryButtonClicked()
//     {
//         EventManager.Notify(GameEvent.OnHealthRecovery);
//     }

//     private void OnSkill_1ButtonClicked()
//     {
//         EventManager.Notify(GameEvent.OnSkill_1);
//     }

//     private void OnSkill_2ButtonClicked()
//     {
//         EventManager.Notify(GameEvent.OnSkill_2);
//     }

//     private void OnActiveSkill_1(object obj)
//     {
//         if (obj is not bool isActive) return;

//         btn_Skill_1.SetInteractable(isActive);
//     }

//     private void OnActiveSkill_2(object obj)
//     {
//         if (obj is not bool isActive) return;

//         btn_Skill_2.SetInteractable(isActive);
//     }
// }
