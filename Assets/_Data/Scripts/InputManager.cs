using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : Singleton<InputManager>
{
    public const string BindingOverridesPlayerPrefsKey = "Controller.BindingOverrides";

    public static InputActions InputActions { get; set; }

    protected override void Awake()
    {
        base.Awake();

        if (Instance != this)
            return;

        InputActions = new InputActions();

        string bindingOverrides = PlayerPrefs.GetString(
            BindingOverridesPlayerPrefsKey,
            string.Empty);

        if (!string.IsNullOrEmpty(bindingOverrides))
            InputActions.LoadBindingOverridesFromJson(bindingOverrides);

        InputActions.Enable();
    }

    private void OnDestroy()
    {
        if (Instance != this)
            return;

        InputActions?.Disable();
        InputActions?.Dispose();
        InputActions = null;
    }
}
