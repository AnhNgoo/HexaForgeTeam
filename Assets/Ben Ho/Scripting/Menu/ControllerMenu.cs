using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[Serializable]
public class KeyBindingUI
{
    public string actionName;
    public KeyCode defaultKey;
    public Button button;
    public TMP_Text keyText;
}

public class ControllerMenu : MenuBase
{
    public override MenuType menuType => MenuType.ControllerMenu;

    [Header("Setting Tabs")]
    [SerializeField] private SettingsTabUI tabs;

    [Header("Arrow Selectors")]
    [SerializeField] private ArrowSelectorUI controlTypeSelector;

    [Header("Controller Controls")]
    [SerializeField] private Slider sliderHorizontalSensitivity;
    [SerializeField] private Slider sliderVerticalSensitivity;
    [SerializeField] private Toggle toggleVibration;
    [SerializeField] private Toggle toggleAimAssist;

    [Header("Key Bindings")]
    [SerializeField] private KeyBindingUI[] keyBindings;

    [Header("Buttons")]
    [SerializeField] private Button btnConfirm;
    [SerializeField] private Button btnBack;

    [Header("Selector Values")]
    [SerializeField] private string[] controlTypes =
    {
        "Keyboard & Mouse",
        "Controller"
    };

    private int controlTypeIndex;
    private int rebindingIndex = -1;

    private KeyCode[] currentKeys;
    private UnityAction[] keyButtonActions;

    protected override void LoadComponent() { }

    protected override void LoadComponentRuntime() { }

    public override void Open(object data = null)
    {
        base.Open(data);

        LoadSettings();
        AddEvents();

        tabs?.SetSelected(MenuType.ControllerMenu);
    }

    public override void Close()
    {
        rebindingIndex = -1;
        RemoveEvents();
        base.Close();
    }

    private void Update()
    {
        if (rebindingIndex < 0 || !Input.anyKeyDown)
            return;

        foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
        {
            if (!Input.GetKeyDown(key))
                continue;

            if (key == KeyCode.Escape)
            {
                rebindingIndex = -1;
                RefreshKeyTexts();
                return;
            }

            currentKeys[rebindingIndex] = key;
            rebindingIndex = -1;
            RefreshKeyTexts();
            return;
        }
    }

    private void AddEvents()
    {
        AddButton(controlTypeSelector.leftButton, PreviousControlType);
        AddButton(controlTypeSelector.rightButton, NextControlType);

        AddButton(tabs.btnAudio, OpenAudio);
        AddButton(tabs.btnGraphics, OpenGraphics);
        AddButton(tabs.btnController, SelectController);

        AddButton(btnConfirm, Confirm);
        AddButton(btnBack, Back);

        if (keyBindings == null)
            return;

        keyButtonActions = new UnityAction[keyBindings.Length];

        for (int i = 0; i < keyBindings.Length; i++)
        {
            if (keyBindings[i].button == null)
                continue;

            int index = i;
            keyButtonActions[i] = () => BeginRebind(index);

            keyBindings[i].button.onClick.AddListener(
                keyButtonActions[i]);
        }
    }

    private void RemoveEvents()
    {
        RemoveButton(controlTypeSelector.leftButton, PreviousControlType);
        RemoveButton(controlTypeSelector.rightButton, NextControlType);

        RemoveButton(tabs.btnAudio, OpenAudio);
        RemoveButton(tabs.btnGraphics, OpenGraphics);
        RemoveButton(tabs.btnController, SelectController);

        RemoveButton(btnConfirm, Confirm);
        RemoveButton(btnBack, Back);

        if (keyButtonActions == null || keyBindings == null)
            return;

        for (int i = 0; i < keyBindings.Length; i++)
        {
            if (keyBindings[i].button != null &&
                keyButtonActions[i] != null)
            {
                keyBindings[i].button.onClick.RemoveListener(
                    keyButtonActions[i]);
            }
        }
    }

    private void LoadSettings()
    {
        controlTypeIndex = Mathf.Clamp(
            PlayerPrefs.GetInt("Controller.ControlType", 0),
            0,
            controlTypes.Length - 1);

        if (sliderHorizontalSensitivity != null)
        {
            sliderHorizontalSensitivity.SetValueWithoutNotify(
                PlayerPrefs.GetFloat(
                    "Controller.HorizontalSensitivity", 1f));
        }

        if (sliderVerticalSensitivity != null)
        {
            sliderVerticalSensitivity.SetValueWithoutNotify(
                PlayerPrefs.GetFloat(
                    "Controller.VerticalSensitivity", 1f));
        }

        if (toggleAimAssist != null)
        {
            toggleAimAssist.SetIsOnWithoutNotify(
                PlayerPrefs.GetInt("Controller.AimAssist", 1) == 1);
        }

        if (toggleVibration != null)
        {
            toggleVibration.SetIsOnWithoutNotify(
                PlayerPrefs.GetInt(
                    "Controller.Vibration", 1) == 1);
        }

        LoadKeyBindings();
        RefreshSelectors();
    }

    private void LoadKeyBindings()
    {
        if (keyBindings == null)
            return;

        currentKeys = new KeyCode[keyBindings.Length];

        for (int i = 0; i < keyBindings.Length; i++)
        {
            KeyBindingUI binding = keyBindings[i];

            string savedKey = PlayerPrefs.GetString(
                "Controller.Key." + binding.actionName,
                binding.defaultKey.ToString());

            if (!Enum.TryParse(savedKey, out currentKeys[i]))
                currentKeys[i] = binding.defaultKey;
        }

        RefreshKeyTexts();
    }

    private void PreviousControlType()
    {
        controlTypeIndex = WrapIndex(
            controlTypeIndex - 1,
            controlTypes.Length);

        RefreshSelectors();
    }

    private void NextControlType()
    {
        controlTypeIndex = WrapIndex(
            controlTypeIndex + 1,
            controlTypes.Length);

        RefreshSelectors();
    }

    private void RefreshSelectors()
    {
        controlTypeSelector.SetText(
            controlTypes[controlTypeIndex]);
    }

    public void BeginRebind(int index)
    {
        if (currentKeys == null ||
            index < 0 ||
            index >= currentKeys.Length)
            return;

        rebindingIndex = index;

        if (keyBindings[index].keyText != null)
            keyBindings[index].keyText.text = "...";
    }

    private void RefreshKeyTexts()
    {
        if (currentKeys == null)
            return;

        for (int i = 0; i < currentKeys.Length; i++)
        {
            if (keyBindings[i].keyText != null)
            {
                keyBindings[i].keyText.text =
                    currentKeys[i].ToString();
            }
        }
    }

    private void Confirm()
    {
        PlayerPrefs.SetInt(
            "Controller.ControlType",
            controlTypeIndex);


        if (sliderHorizontalSensitivity != null)
        {
            PlayerPrefs.SetFloat(
                "Controller.HorizontalSensitivity",
                sliderHorizontalSensitivity.value);
        }

        if (sliderVerticalSensitivity != null)
        {
            PlayerPrefs.SetFloat(
                "Controller.VerticalSensitivity",
                sliderVerticalSensitivity.value);
        }
        
        if (toggleAimAssist != null)
        {
            PlayerPrefs.SetInt(
                "Controller.AimAssist",
                toggleAimAssist.isOn ? 1 : 0);
        }

        if (toggleVibration != null)
        {
            PlayerPrefs.SetInt(
                "Controller.Vibration",
                toggleVibration.isOn ? 1 : 0);
        }

        if (currentKeys != null)
        {
            for (int i = 0; i < currentKeys.Length; i++)
            {
                PlayerPrefs.SetString(
                    "Controller.Key." +
                    keyBindings[i].actionName,
                    currentKeys[i].ToString());
            }
        }

        PlayerPrefs.Save();
    }

    private void Back()
    {
        LoadSettings();
        UIManager.Instance.ChangeMenu(SettingMenuData.BackMenu);
    }

    private void OpenAudio()
    {
        UIManager.Instance.ChangeMenu(MenuType.SettingMenu);
    }

    private void OpenGraphics()
    {
        UIManager.Instance.ChangeMenu(MenuType.GraphicsMenu);
    }

    private void SelectController()
    {
        tabs.SetSelected(MenuType.ControllerMenu);
    }

    private int WrapIndex(int index, int length)
    {
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
}