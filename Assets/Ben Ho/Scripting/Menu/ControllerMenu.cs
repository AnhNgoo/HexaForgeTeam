using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[Serializable]
public class KeyBindingUI
{
    public string actionName;
    public Key defaultKey;
    public Button button;
    public TMP_Text keyText;
}

public static class GameKeyBindings
{
    public static Key GetKey(string actionName, Key defaultKey)
    {
        string savedKey = PlayerPrefs.GetString(
            "Controller.Key." + actionName,
            defaultKey.ToString());

        if (Enum.TryParse(savedKey, out Key key))
            return key;

        return defaultKey;
    }

    public static bool WasPressedThisFrame(string actionName, Key defaultKey)
    {
        if (Keyboard.current == null)
            return false;

        Key key = GetKey(actionName, defaultKey);
        KeyControlWrapper keyControl = new KeyControlWrapper(key);

        return keyControl.WasPressedThisFrame();
    }

    private readonly struct KeyControlWrapper
    {
        private readonly Key key;

        public KeyControlWrapper(Key key)
        {
            this.key = key;
        }

        public bool WasPressedThisFrame()
        {
            if (Keyboard.current == null)
                return false;

            try
            {
                return Keyboard.current[key].wasPressedThisFrame;
            }
            catch
            {
                return false;
            }
        }
    }
}

public class ControllerMenu : MenuBase
{
    public override MenuType menuType => MenuType.ControllerMenu;

    [Header("Setting Tabs")]
    [SerializeField] private SettingsTabUI tabs;

    [Header("Controller Controls")]
    [SerializeField] private Slider sliderHorizontalSensitivity;
    [SerializeField] private Slider sliderVerticalSensitivity;
    [SerializeField] private Toggle toggleVibration;
    [SerializeField] private Toggle toggleAimAssist;

    [Header("Control Type")]
    [SerializeField] private ArrowSelectorUI controlTypeSelector;
    [SerializeField] private string[] controlTypes = { "Keyboard & Mouse", "Controller" };

    [Header("Key Bindings")]
    [SerializeField] private KeyBindingUI[] keyBindings;

    [Header("Buttons")]
    [SerializeField] private Button btnConfirm;
    [SerializeField] private Button btnBack;

    [Header("Embedded System Settings")]
    [SerializeField] private SystemSettingsPanel systemSettingsPanel;

    private bool eventsAdded;
    private int controlTypeIndex;
    private int rebindingIndex = -1;

    private Key[] currentKeys;
    private UnityAction[] keyButtonActions;

    protected override void LoadComponent() { }
    protected override void LoadComponentRuntime() { }

    public override void Open(object data = null)
    {
        base.Open(data);
        Initialize();
    }

    public override void Close()
    {
        rebindingIndex = -1;
        RemoveEvents();
        base.Close();
    }

    private void OnEnable()
    {
        Initialize();
    }

    private void OnDisable()
    {
        rebindingIndex = -1;
        RemoveEvents();
    }

    private void Update()
    {
        if (rebindingIndex < 0)
            return;

        if (Keyboard.current == null)
            return;

        Key? pressedKey = GetPressedKey();

        if (!pressedKey.HasValue)
            return;

        if (pressedKey.Value == Key.Escape)
        {
            rebindingIndex = -1;
            RefreshKeyTexts();
            return;
        }

        currentKeys[rebindingIndex] = pressedKey.Value;
        rebindingIndex = -1;
        RefreshKeyTexts();
    }

    private void Initialize()
    {
        RemoveEvents();
        EnsureRuntimeKeyBindings();
        LoadSettings();
        AddEvents();
        tabs?.SetSelected(MenuType.ControllerMenu);
    }

    private void AddEvents()
    {
        if (eventsAdded)
            return;

        AddButton(controlTypeSelector.leftButton, PreviousControlType);
        AddButton(controlTypeSelector.rightButton, NextControlType);

        AddButton(tabs?.btnAudio, OpenAudioTab);
        AddButton(tabs?.btnGraphics, OpenGraphicsTab);
        AddButton(tabs?.btnController, OpenControllerTab);

        AddButton(btnConfirm, Confirm);
        AddButton(btnBack, Back);

        if (keyBindings != null)
        {
            keyButtonActions = new UnityAction[keyBindings.Length];

            for (int i = 0; i < keyBindings.Length; i++)
            {
                if (keyBindings[i] == null || keyBindings[i].button == null)
                    continue;

                int index = i;
                keyButtonActions[i] = () => BeginRebind(index);
                keyBindings[i].button.onClick.AddListener(keyButtonActions[i]);
            }
        }

        eventsAdded = true;
    }

    private void RemoveEvents()
    {
        if (!eventsAdded)
            return;

        RemoveButton(controlTypeSelector.leftButton, PreviousControlType);
        RemoveButton(controlTypeSelector.rightButton, NextControlType);

        RemoveButton(tabs?.btnAudio, OpenAudioTab);
        RemoveButton(tabs?.btnGraphics, OpenGraphicsTab);
        RemoveButton(tabs?.btnController, OpenControllerTab);

        RemoveButton(btnConfirm, Confirm);
        RemoveButton(btnBack, Back);

        if (keyBindings != null && keyButtonActions != null)
        {
            int count = Mathf.Min(keyBindings.Length, keyButtonActions.Length);

            for (int i = 0; i < count; i++)
            {
                if (keyBindings[i] != null &&
                    keyBindings[i].button != null &&
                    keyButtonActions[i] != null)
                {
                    keyBindings[i].button.onClick.RemoveListener(keyButtonActions[i]);
                }
            }
        }

        keyButtonActions = null;
        eventsAdded = false;
    }

    private void LoadSettings()
    {
        controlTypeIndex = Mathf.Clamp(
            PlayerPrefs.GetInt("Controller.ControlType", 0),
            0,
            Mathf.Max(0, controlTypes.Length - 1));

        SetSlider(
            sliderHorizontalSensitivity,
            PlayerPrefs.GetFloat("Controller.HorizontalSensitivity", 1f));

        SetSlider(
            sliderVerticalSensitivity,
            PlayerPrefs.GetFloat("Controller.VerticalSensitivity", 1f));

        SetToggle(
            toggleVibration,
            PlayerPrefs.GetInt("Controller.Vibration", 1) == 1);

        SetToggle(
            toggleAimAssist,
            PlayerPrefs.GetInt("Controller.AimAssist", 1) == 1);

        LoadKeyBindings();
        RefreshSelectors();
    }

    private void LoadKeyBindings()
    {
        if (keyBindings == null)
            return;

        currentKeys = new Key[keyBindings.Length];

        for (int i = 0; i < keyBindings.Length; i++)
        {
            KeyBindingUI binding = keyBindings[i];

            if (binding == null)
                continue;

            currentKeys[i] = GameKeyBindings.GetKey(
                binding.actionName,
                GetInputDefaultKey(binding.actionName, binding.defaultKey));
        }

        RefreshKeyTexts();
    }

    private void Confirm()
    {
        PlayerPrefs.SetInt("Controller.ControlType", controlTypeIndex);

        SaveSlider("Controller.HorizontalSensitivity", sliderHorizontalSensitivity);
        SaveSlider("Controller.VerticalSensitivity", sliderVerticalSensitivity);

        SaveToggle("Controller.Vibration", toggleVibration);
        SaveToggle("Controller.AimAssist", toggleAimAssist);

        if (keyBindings != null && currentKeys != null)
        {
            int count = Mathf.Min(keyBindings.Length, currentKeys.Length);

            for (int i = 0; i < count; i++)
            {
                if (keyBindings[i] == null || string.IsNullOrWhiteSpace(keyBindings[i].actionName))
                    continue;

                PlayerPrefs.SetString(
                    "Controller.Key." + keyBindings[i].actionName,
                    currentKeys[i].ToString());
            }
        }

        ApplyInputActionOverrides();

        PlayerPrefs.Save();
    }

    private void ApplyInputActionOverrides()
    {
        if (Keyboard.current == null || keyBindings == null || currentKeys == null)
            return;

        InputActions inputActions = InputManager.InputActions;
        bool ownsInputActions = inputActions == null;

        if (ownsInputActions)
        {
            inputActions = new InputActions();

            string savedOverrides = PlayerPrefs.GetString(
                InputManager.BindingOverridesPlayerPrefsKey,
                string.Empty);

            if (!string.IsNullOrEmpty(savedOverrides))
                inputActions.LoadBindingOverridesFromJson(savedOverrides);
        }

        inputActions.Disable();

        try
        {
            int count = Mathf.Min(keyBindings.Length, currentKeys.Length);

            for (int i = 0; i < count; i++)
            {
                KeyBindingUI bindingUI = keyBindings[i];

                if (bindingUI == null || string.IsNullOrWhiteSpace(bindingUI.actionName))
                    continue;

                if (currentKeys[i] == Key.None)
                    continue;

                ResolveInputAction(
                    bindingUI.actionName,
                    out string inputActionName,
                    out string compositePartName);

                InputAction action = inputActions.asset.FindAction(
                    "Keyboard/" + inputActionName,
                    false);

                if (action == null)
                {
                    Debug.LogWarning($"ControllerMenu: Input Action '{inputActionName}' was not found.");
                    continue;
                }

                int bindingIndex = FindKeyboardBindingIndex(action, compositePartName);

                if (bindingIndex < 0)
                {
                    Debug.LogWarning(
                        $"ControllerMenu: Keyboard binding for '{bindingUI.actionName}' was not found.");
                    continue;
                }

                action.ApplyBindingOverride(
                    bindingIndex,
                    Keyboard.current[currentKeys[i]].path);
            }

            PlayerPrefs.SetString(
                InputManager.BindingOverridesPlayerPrefsKey,
                inputActions.SaveBindingOverridesAsJson());
        }
        finally
        {
            if (ownsInputActions)
                inputActions.Dispose();
            else
                inputActions.Enable();
        }
    }

    private static int FindKeyboardBindingIndex(
        InputAction action,
        string compositePartName)
    {
        for (int i = 0; i < action.bindings.Count; i++)
        {
            InputBinding binding = action.bindings[i];

            if (!string.IsNullOrEmpty(compositePartName))
            {
                if (binding.isPartOfComposite &&
                    string.Equals(
                        binding.name,
                        compositePartName,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }

                continue;
            }

            if (!binding.isComposite &&
                !binding.isPartOfComposite &&
                binding.path.StartsWith(
                    "<Keyboard>",
                    StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        return -1;
    }

    private static void ResolveInputAction(
        string menuActionName,
        out string inputActionName,
        out string compositePartName)
    {
        compositePartName = string.Empty;

        switch (menuActionName.Trim())
        {
            case "Move Forward":
                inputActionName = "Move";
                compositePartName = "up";
                break;
            case "Move Backward":
                inputActionName = "Move";
                compositePartName = "down";
                break;
            case "Move Left":
                inputActionName = "Move";
                compositePartName = "left";
                break;
            case "Move Right":
                inputActionName = "Move";
                compositePartName = "right";
                break;
            case "Sneak":
            case "Sneak/Crouch":
            case "Sneak / Crouch":
                inputActionName = "Walk";
                break;
            default:
                inputActionName = menuActionName.Trim();
                break;
        }
    }

    private static Key GetInputDefaultKey(string menuActionName, Key fallback)
    {
        switch (menuActionName.Trim())
        {
            case "Move Forward":
                return Key.W;
            case "Move Backward":
                return Key.S;
            case "Move Left":
                return Key.A;
            case "Move Right":
                return Key.D;
            case "Jump":
                return Key.Space;
            case "Dodge":
                return Key.LeftShift;
            case "Sprint":
                return Key.LeftAlt;
            case "Sneak":
            case "Sneak/Crouch":
            case "Sneak / Crouch":
                return Key.LeftCtrl;
            case "Interact":
                return Key.F;
            default:
                return fallback;
        }
    }

    private void EnsureRuntimeKeyBindings()
    {
        if (keyBindings != null && keyBindings.Length > 0)
            return;

        RuntimeBindingSpec[] specs =
        {
            new RuntimeBindingSpec("Jump", Key.Space),
            new RuntimeBindingSpec("Dodge", Key.LeftShift),
            new RuntimeBindingSpec("Sprint", Key.LeftAlt),
            new RuntimeBindingSpec("Sneak/Crouch", Key.LeftCtrl, "Sneak / Crouch", "Sneak"),
            new RuntimeBindingSpec("Interact", Key.F),
            new RuntimeBindingSpec("Move Forward", Key.W),
            new RuntimeBindingSpec("Move Backward", Key.S),
            new RuntimeBindingSpec("Move Right", Key.D),
            new RuntimeBindingSpec("Move Left", Key.A)
        };

        TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(true);
        List<KeyBindingUI> bindings = new List<KeyBindingUI>(specs.Length);

        foreach (RuntimeBindingSpec spec in specs)
        {
            TMP_Text titleText = FindText(texts, spec);

            if (titleText == null || titleText.transform.parent == null)
                continue;

            Transform row = titleText.transform.parent;
            TMP_Text keyText = FindKeyText(row, titleText);

            if (keyText == null)
                continue;

            Transform clickTarget = keyText.transform.parent != null
                ? keyText.transform.parent
                : keyText.transform;

            Button button = clickTarget.GetComponent<Button>();

            if (button == null)
            {
                button = clickTarget.gameObject.AddComponent<Button>();
                button.transition = Selectable.Transition.None;
                button.targetGraphic = keyText;
            }

            bindings.Add(new KeyBindingUI
            {
                actionName = spec.ActionName,
                defaultKey = spec.DefaultKey,
                button = button,
                keyText = keyText
            });
        }

        keyBindings = bindings.ToArray();

        if (keyBindings.Length != specs.Length)
        {
            Debug.LogWarning(
                $"ControllerMenu: Auto-wired {keyBindings.Length}/{specs.Length} key binding rows. " +
                "Assign Key Bindings in the Inspector if a row label was renamed.");
        }
    }

    private static TMP_Text FindText(TMP_Text[] texts, RuntimeBindingSpec spec)
    {
        foreach (TMP_Text text in texts)
        {
            if (text == null)
                continue;

            string value = text.text.Trim();

            if (string.Equals(value, spec.ActionName, StringComparison.OrdinalIgnoreCase))
                return text;

            foreach (string alias in spec.Aliases)
            {
                if (string.Equals(value, alias, StringComparison.OrdinalIgnoreCase))
                    return text;
            }
        }

        return null;
    }

    private static TMP_Text FindKeyText(Transform row, TMP_Text titleText)
    {
        TMP_Text[] rowTexts = row.GetComponentsInChildren<TMP_Text>(true);

        foreach (TMP_Text text in rowTexts)
        {
            if (text != null && text != titleText)
                return text;
        }

        return null;
    }

    private readonly struct RuntimeBindingSpec
    {
        public readonly string ActionName;
        public readonly Key DefaultKey;
        public readonly string[] Aliases;

        public RuntimeBindingSpec(
            string actionName,
            Key defaultKey,
            params string[] aliases)
        {
            ActionName = actionName;
            DefaultKey = defaultKey;
            Aliases = aliases ?? Array.Empty<string>();
        }
    }

    private void OpenAudioTab()
    {
        OpenSettingTab(
            MenuType.SettingMenu,
            SystemSettingPage.Audio);
    }

    private void OpenGraphicsTab()
    {
        OpenSettingTab(
            MenuType.GraphicsMenu,
            SystemSettingPage.Graphics);
    }

    private void OpenControllerTab()
    {
        OpenSettingTab(
            MenuType.ControllerMenu,
            SystemSettingPage.Controller);
    }

    private void OpenSettingTab(
        MenuType targetMenu,
        SystemSettingPage systemPage)
    {
        if (systemSettingsPanel != null)
        {
            systemSettingsPanel.ShowPage(systemPage);
            return;
        }

        if (targetMenu == menuType)
        {
            tabs?.SetSelected(menuType);
            return;
        }

        if (UIManager.Instance != null)
            UIManager.Instance.ChangeMenu(targetMenu);
    }

    private void Back()
    {
        LoadSettings();

        if (systemSettingsPanel != null)
            systemSettingsPanel.CloseGameSystemMenu();
        else if (UIManager.Instance != null)
            UIManager.Instance.ChangeMenu(SettingMenuData.BackMenu);
    }

    public void BeginRebind(int index)
    {
        if (currentKeys == null || index < 0 || index >= currentKeys.Length)
            return;

        rebindingIndex = index;

        if (keyBindings[index] != null && keyBindings[index].keyText != null)
            keyBindings[index].keyText.text = "...";
    }

    private Key? GetPressedKey()
    {
        foreach (Key key in Enum.GetValues(typeof(Key)))
        {
            if (key == Key.None)
                continue;

            try
            {
                if (Keyboard.current[key].wasPressedThisFrame)
                    return key;
            }
            catch
            {
                // Some enum values are not valid keyboard keys.
            }
        }

        return null;
    }

    private void PreviousControlType()
    {
        controlTypeIndex = Wrap(controlTypeIndex - 1, controlTypes.Length);
        RefreshSelectors();
    }

    private void NextControlType()
    {
        controlTypeIndex = Wrap(controlTypeIndex + 1, controlTypes.Length);
        RefreshSelectors();
    }

    private void RefreshSelectors()
    {
        if (controlTypes == null || controlTypes.Length == 0)
        {
            controlTypeSelector.SetText("");
            return;
        }

        controlTypeSelector.SetText(controlTypes[Mathf.Clamp(controlTypeIndex, 0, controlTypes.Length - 1)]);
    }

    private void RefreshKeyTexts()
    {
        if (keyBindings == null || currentKeys == null)
            return;

        int count = Mathf.Min(keyBindings.Length, currentKeys.Length);

        for (int i = 0; i < count; i++)
        {
            if (keyBindings[i] != null && keyBindings[i].keyText != null)
                keyBindings[i].keyText.text = FormatKey(currentKeys[i]);
        }
    }

    private string FormatKey(Key key)
    {
        switch (key)
        {
            case Key.Space:
                return "Space";
            case Key.LeftShift:
            case Key.RightShift:
                return "Shift";
            case Key.LeftCtrl:
            case Key.RightCtrl:
                return "Ctrl";
            default:
                return key.ToString();
        }
    }

    private int Wrap(int index, int length)
    {
        if (length <= 0)
            return 0;

        if (index < 0)
            return length - 1;

        if (index >= length)
            return 0;

        return index;
    }

    private void AddButton(Button button, UnityAction action)
    {
        if (button != null)
            button.onClick.AddListener(action);
    }

    private void RemoveButton(Button button, UnityAction action)
    {
        if (button != null)
            button.onClick.RemoveListener(action);
    }

    private void SetSlider(Slider slider, float value)
    {
        if (slider != null)
            slider.SetValueWithoutNotify(value);
    }

    private void SaveSlider(string key, Slider slider)
    {
        if (slider != null)
            PlayerPrefs.SetFloat(key, slider.value);
    }

    private void SetToggle(Toggle toggle, bool value)
    {
        if (toggle != null)
            toggle.SetIsOnWithoutNotify(value);
    }

    private void SaveToggle(string key, Toggle toggle)
    {
        if (toggle != null)
            PlayerPrefs.SetInt(key, toggle.isOn ? 1 : 0);
    }
}
