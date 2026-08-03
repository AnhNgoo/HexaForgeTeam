using TMPro;
using UnityEngine.InputSystem;

public class Dodge : LoadComponents
{
    [UnityEngine.SerializeField]
    private TMP_Text keyTextDodge;

    protected override void LoadComponent()
    {
        if (keyTextDodge == null)
        {
            keyTextDodge = GetComponent<TMP_Text>();
        }

        if (keyTextDodge == null)
        {
            keyTextDodge = GetComponentInChildren<TMP_Text>(true);
        }
    }

    protected override void LoadComponentRuntime()
    {
        LoadComponent();
        RefreshKeyText();
    }

    private void OnEnable()
    {
        RefreshKeyText();
    }

    private void RefreshKeyText()
    {
        if (keyTextDodge == null)
            return;

        keyTextDodge.text = GetDodgeBindingDisplay();
    }

    private string GetDodgeBindingDisplay()
    {
        InputAction dodgeAction =
            InputManager.InputActions.Keyboard.Dodge;

        for (int index = 0;
             index < dodgeAction.bindings.Count;
             index++)
        {
            if (dodgeAction.bindings[index].name == "Left Shift")
            {
                string displayText =
                    dodgeAction.GetBindingDisplayString(index);

                return string.IsNullOrWhiteSpace(displayText)
                    ? "Left Shift"
                    : displayText;
            }
        }

        return "Left Shift";
    }
}