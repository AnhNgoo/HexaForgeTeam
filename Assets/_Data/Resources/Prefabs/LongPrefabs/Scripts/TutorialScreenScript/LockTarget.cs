using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class LockTarget : LoadComponents
{
    [SerializeField] private TMP_Text lockTargetText;
    protected override void LoadComponent()
    {
        if (lockTargetText == null)
        {
            lockTargetText = GetComponent<TMP_Text>();
        }

        if (lockTargetText == null)
        {
            lockTargetText = GetComponentInChildren<TMP_Text>(true);
        }
    }

    protected override void LoadComponentRuntime()
    {
        LoadComponent();
        RefreshLockTargetText();
    }

    private void OnEnable()
    {
        RefreshLockTargetText();
    }

    private void RefreshLockTargetText()
    {
        if (lockTargetText == null)
            return;

        lockTargetText.text = GetLockTargetBindingDisplay();
    }

    private string GetLockTargetBindingDisplay()
    {
        InputAction lockTargetAction =
            InputManager.InputActions.Keyboard.LockTarget;

        for (int index = 0;
             index < lockTargetAction.bindings.Count;
             index++)
        {
            if (lockTargetAction.bindings[index].name == "Middle Button")
            {
                string displayText =
                    lockTargetAction.GetBindingDisplayString(index);

                return string.IsNullOrWhiteSpace(displayText)
                    ? "Middle Button"
                    : displayText;
            }
        }

        return "Middle Button";
    }
}
