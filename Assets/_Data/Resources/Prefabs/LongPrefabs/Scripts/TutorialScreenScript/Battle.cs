using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class Battle : LoadComponents
{
    [SerializeField] private TMP_Text battleText;

    protected override void LoadComponent()
    {
        if (battleText == null)
        {
            battleText = GetComponent<TMP_Text>();
        }

        if (battleText == null)
        {
            battleText = GetComponentInChildren<TMP_Text>(true);
        }
    }

    protected override void LoadComponentRuntime()
    {
        LoadComponent();
        RefreshBattleText();
    }

    private void OnEnable()
    {
        RefreshBattleText();
    }

    private void RefreshBattleText()
    {
        if (battleText == null)
            return;

        battleText.text = GetBattleBindingDisplay();
    }

    private string GetBattleBindingDisplay()
    {
        InputAction battleAction = 
            InputManager.InputActions.Keyboard.Attack;

        for (int index = 0;
             index < battleAction.bindings.Count;
             index++)
        {
            if (battleAction.bindings[index].name == "Left Button")
            {
                string displayText =
                    battleAction.GetBindingDisplayString(index);

                return string.IsNullOrWhiteSpace(displayText)
                    ? "Left Button"
                    : displayText;
            }
        }

        return "Left Button";
    }
}
