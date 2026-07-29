using TMPro;
using UnityEngine.InputSystem;

public class MoveLeft : LoadComponents
{
    [UnityEngine.SerializeField]
    private TMP_Text keyTextLeft;

    protected override void LoadComponent()
    {
        if (keyTextLeft == null)
        {
            keyTextLeft = GetComponent<TMP_Text>();
        }

        if (keyTextLeft == null)
        {
            keyTextLeft = GetComponentInChildren<TMP_Text>(true);
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
        if (keyTextLeft == null)
            return;

        keyTextLeft.text = GetLeftBindingDisplay();
    }

    private string GetLeftBindingDisplay()
    {
        if (InputManager.InputActions == null)
            return "A";

        InputAction moveAction =
            InputManager.InputActions.Keyboard.Move;

        for (int index = 0;
             index < moveAction.bindings.Count;
             index++)
        {
            if (moveAction.bindings[index].name == "left")
            {
                string displayText =
                    moveAction.GetBindingDisplayString(index);

                return string.IsNullOrWhiteSpace(displayText)
                    ? "A"
                    : displayText;
            }
        }

        return "A";
    }
}