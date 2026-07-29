using TMPro;
using UnityEngine.InputSystem;

public class MoveRight : LoadComponents
{
    [UnityEngine.SerializeField]
    private TMP_Text keyTextRight;
    protected override void LoadComponent()
    {
        if (keyTextRight == null)
        {
            keyTextRight = GetComponent<TMP_Text>();
        }
        if (keyTextRight == null)
        {
            keyTextRight = GetComponentInChildren<TMP_Text>(true);
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
        if (keyTextRight == null) 
            return;

        keyTextRight.text = GetRightBindingDisplay();
    }

    private string GetRightBindingDisplay()
    {
        if (InputManager.InputActions == null)
            return "D";

        InputAction moveAction = 
            InputManager.InputActions.Keyboard.Move;

        for (int index = 0; 
             index < moveAction.bindings.Count; 
             index++)
        {
            if (moveAction.bindings[index].name == "right")
            {
                string displayText = 
                    moveAction.GetBindingDisplayString(index);
                return string.IsNullOrWhiteSpace(displayText)
                    ? "D"   
                    : displayText;
            }
        }

        return "D";
    }
}