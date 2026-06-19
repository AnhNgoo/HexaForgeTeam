using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;

public class InputManager : Singleton<InputManager>
{
    public static InputActions InputActions { get; set; }

    protected override void Awake()
    {
        base.Awake();
        InputActions = new InputActions();
        InputActions?.Enable();
    }

    private void OnDestroy()
    {
        InputActions?.Disable();
        InputActions?.Dispose();
    }
}
