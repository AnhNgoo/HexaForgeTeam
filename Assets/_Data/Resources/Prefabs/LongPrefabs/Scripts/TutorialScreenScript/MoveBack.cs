using TMPro;
using UnityEngine.InputSystem;

public class MoveBack : LoadComponents
{
    [UnityEngine.SerializeField]
    private TMP_Text keyTextBack;

    protected override void LoadComponent()
    {
        if (keyTextBack == null)
        {
            keyTextBack = GetComponent<TMP_Text>();
        }

        if (keyTextBack == null)
        {
            keyTextBack = GetComponentInChildren<TMP_Text>(true);
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
        if (keyTextBack == null)
            return;

        keyTextBack.text = GetBackBindingDisplay();
    }

    private string GetBackBindingDisplay()
    {
        if (InputManager.InputActions == null)
            return "S";

        InputAction moveAction =
            InputManager.InputActions.Keyboard.Move;

        for (int index = 0;
             index < moveAction.bindings.Count;
             index++)
        {
            if (moveAction.bindings[index].name == "back")
            {
                string displayText =
                    moveAction.GetBindingDisplayString(index);

                return string.IsNullOrWhiteSpace(displayText)
                    ? "S"
                    : displayText;
            }
        }
        return "S";
    }
}