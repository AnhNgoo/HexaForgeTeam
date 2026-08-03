using System;
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
        LoadSettings();
        AddEvents();
    }

    private void AddEvents()
    {
        if (eventsAdded)
            return;

        AddButton(controlTypeSelector.leftButton, PreviousControlType);
        AddButton(controlTypeSelector.rightButton, NextControlType);

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
                binding.defaultKey);
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

        PlayerPrefs.Save();
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