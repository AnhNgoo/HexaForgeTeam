using TMPro;
using UnityEngine.InputSystem;

public class MoveForward : LoadComponents
{
    [UnityEngine.SerializeField]
    private TMP_Text keyTextForward;

    protected override void LoadComponent()
    {
        if (keyTextForward == null)
        {
            keyTextForward = GetComponent<TMP_Text>();
        }

        if (keyTextForward == null)
        {
            keyTextForward = GetComponentInChildren<TMP_Text>(true);
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
        if (keyTextForward == null)
            return;

        keyTextForward.text = GetForwardBindingDisplay();
    }

    private string GetForwardBindingDisplay()
    {
        if (InputManager.InputActions == null)
            return "W";

        InputAction moveAction =
            InputManager.InputActions.Keyboard.Move;

        for (int index = 0;
             index < moveAction.bindings.Count;
             index++)
        {
            if (moveAction.bindings[index].name == "up")
            {
                string displayText =
                    moveAction.GetBindingDisplayString(index);

                return string.IsNullOrWhiteSpace(displayText)
                    ? "W"
                    : displayText;
            }
        }

        return "W";
    }
}