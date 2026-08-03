using TMPro;
using UnityEngine.InputSystem;

public class ReceiveRecoveryBottle : LoadComponents
{
    [UnityEngine.SerializeField]
    private TMP_Text keyTextReceiveRecoveryBottle;

    protected override void LoadComponent()
    {
        if (keyTextReceiveRecoveryBottle == null)
        {
            keyTextReceiveRecoveryBottle = GetComponent<TMP_Text>();
        }

        if (keyTextReceiveRecoveryBottle == null)
        {
            keyTextReceiveRecoveryBottle = GetComponentInChildren<TMP_Text>(true);
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
        if (keyTextReceiveRecoveryBottle == null)
            return;

        keyTextReceiveRecoveryBottle.text = GetReceiveRecoveryBottleBindingDisplay();
    }

    private string GetReceiveRecoveryBottleBindingDisplay()
    {
        InputAction receiveRecoveryBottleAction =
            InputManager.InputActions.Keyboard.HealthRecovery;

        for (int index = 0;
             index < receiveRecoveryBottleAction.bindings.Count;
             index++)
        {
            if (receiveRecoveryBottleAction.bindings[index].name == "R")
            {
                string displayText =
                    receiveRecoveryBottleAction.GetBindingDisplayString(index);

                return string.IsNullOrWhiteSpace(displayText)
                    ? "R"
                    : displayText;
            }
        }

        return "R";
    }
}