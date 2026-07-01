using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class ArrowSelectorUI
{
    public Button leftButton;
    public Button rightButton;
    public TMP_Text valueText;

    public void SetText(string value)
    {
        if (valueText != null)
            valueText.text = value;
    }
}

[Serializable]
public class SettingsTabUI
{
    public Button btnAudio;
    public Button btnGraphics;
    public Button btnController;

    public TMP_Text txtAudio;
    public TMP_Text txtGraphics;
    public TMP_Text txtController;

    public GameObject lineAudio;
    public GameObject lineGraphics;
    public GameObject lineController;

    private static readonly Color SelectedColor =
        new Color(0.92549f, 0.80392f, 0.62353f, 1f);

    public void SetSelected(MenuType type)
    {
        SetTab(
            txtAudio,
            lineAudio,
            type == MenuType.SettingMenu);

        SetTab(
            txtGraphics,
            lineGraphics,
            type == MenuType.GraphicsMenu);

        SetTab(
            txtController,
            lineController,
            type == MenuType.ControllerMenu);
    }

    private void SetTab(
        TMP_Text text,
        GameObject line,
        bool selected)
    {
        if (text != null)
            text.color = selected ? SelectedColor : Color.white;

        if (line != null)
            line.SetActive(selected);
    }
}

public class GraphicsMenu : MenuBase
{
    public override MenuType menuType => MenuType.GraphicsMenu;

    [Header("Setting Tabs")]
    [SerializeField] private SettingsTabUI tabs;

    [Header("Dropdown")]
    [SerializeField] private TMP_Dropdown dropdownResolution;

    [Header("Arrow Selectors")]
    [SerializeField] private ArrowSelectorUI frameRateSelector;
    [SerializeField] private ArrowSelectorUI displayModeSelector;
    [SerializeField] private ArrowSelectorUI sharpeningSelector;

    [Header("Checkboxes")]
    [SerializeField] private Toggle toggleVSync;
    [SerializeField] private Toggle toggleMotionBlur;
    [SerializeField] private Toggle toggleChromaticAberration;

    [Header("Color Grading")]
    [SerializeField] private Slider sliderBrightness;
    [SerializeField] private Slider sliderContrast;
    [SerializeField] private Slider sliderSaturation;
    [SerializeField] private Slider sliderFieldOfView;

    [Header("Buttons")]
    [SerializeField] private Button btnConfirm;
    [SerializeField] private Button btnBack;

    [Header("Selector Values")]
    [SerializeField] private int[] frameRates =
    {
        30, 60, 120, 144, 165, 240, -1
    };

    [SerializeField] private string[] displayModes =
    {
        "Full Screen",
        "Borderless",
        "Windowed"
    };

    [SerializeField] private string[] sharpeningModes =
    {
        "Off",
        "Fidelity FX"
    };

    private readonly List<Resolution> resolutions =
        new List<Resolution>();

    private int frameRateIndex;
    private int displayModeIndex;
    private int sharpeningIndex;

    protected override void LoadComponent() { }

    protected override void LoadComponentRuntime() { }

    public override void Open(object data = null)
    {
        base.Open(data);

        BuildResolutionDropdown();
        LoadSettings();
        AddEvents();

        tabs?.SetSelected(MenuType.GraphicsMenu);
    }

    public override void Close()
    {
        RemoveEvents();
        base.Close();
    }

    private void AddEvents()
    {
        AddButton(frameRateSelector.leftButton, PreviousFrameRate);
        AddButton(frameRateSelector.rightButton, NextFrameRate);

        AddButton(displayModeSelector.leftButton, PreviousDisplayMode);
        AddButton(displayModeSelector.rightButton, NextDisplayMode);

        AddButton(sharpeningSelector.leftButton, PreviousSharpening);
        AddButton(sharpeningSelector.rightButton, NextSharpening);

        AddButton(tabs.btnAudio, OpenAudio);
        AddButton(tabs.btnGraphics, SelectGraphics);
        AddButton(tabs.btnController, OpenController);

        AddButton(btnConfirm, Confirm);
        AddButton(btnBack, Back);
    }

    private void RemoveEvents()
    {
        RemoveButton(frameRateSelector.leftButton, PreviousFrameRate);
        RemoveButton(frameRateSelector.rightButton, NextFrameRate);

        RemoveButton(displayModeSelector.leftButton, PreviousDisplayMode);
        RemoveButton(displayModeSelector.rightButton, NextDisplayMode);

        RemoveButton(sharpeningSelector.leftButton, PreviousSharpening);
        RemoveButton(sharpeningSelector.rightButton, NextSharpening);

        RemoveButton(tabs.btnAudio, OpenAudio);
        RemoveButton(tabs.btnGraphics, SelectGraphics);
        RemoveButton(tabs.btnController, OpenController);

        RemoveButton(btnConfirm, Confirm);
        RemoveButton(btnBack, Back);
    }

    private void BuildResolutionDropdown()
    {
        if (dropdownResolution == null)
            return;

        resolutions.Clear();
        List<string> options = new List<string>();

        foreach (Resolution resolution in Screen.resolutions)
        {
            bool duplicated = resolutions.Exists(item =>
                item.width == resolution.width &&
                item.height == resolution.height);

            if (duplicated)
                continue;

            resolutions.Add(resolution);
            options.Add($"{resolution.width} X {resolution.height}");
        }

        dropdownResolution.ClearOptions();
        dropdownResolution.AddOptions(options);

        int savedWidth = PlayerPrefs.GetInt(
            "Graphics.ResolutionWidth",
            Screen.width);

        int savedHeight = PlayerPrefs.GetInt(
            "Graphics.ResolutionHeight",
            Screen.height);

        int selectedIndex = resolutions.FindIndex(item =>
            item.width == savedWidth &&
            item.height == savedHeight);

        if (selectedIndex < 0)
            selectedIndex = resolutions.FindIndex(item =>
                item.width == Screen.width &&
                item.height == Screen.height);

        dropdownResolution.SetValueWithoutNotify(
            Mathf.Max(0, selectedIndex));

        dropdownResolution.RefreshShownValue();
    }

    private void LoadSettings()
    {
        frameRateIndex = FindFrameRateIndex(
            PlayerPrefs.GetInt("Graphics.FrameRate", 60));

        displayModeIndex = Mathf.Clamp(
            PlayerPrefs.GetInt("Graphics.DisplayMode", 0),
            0,
            displayModes.Length - 1);

        sharpeningIndex = Mathf.Clamp(
            PlayerPrefs.GetInt("Graphics.Sharpening", 1),
            0,
            sharpeningModes.Length - 1);

        SetToggle(
            toggleVSync,
            PlayerPrefs.GetInt("Graphics.VSync", 1) == 1);

        SetToggle(
            toggleMotionBlur,
            PlayerPrefs.GetInt("Graphics.MotionBlur", 1) == 1);

        SetToggle(
            toggleChromaticAberration,
            PlayerPrefs.GetInt(
                "Graphics.ChromaticAberration", 1) == 1);

        SetSlider(
            sliderBrightness,
            PlayerPrefs.GetFloat("Graphics.Brightness", 1f));

        SetSlider(
            sliderContrast,
            PlayerPrefs.GetFloat("Graphics.Contrast", 1f));

        SetSlider(
            sliderSaturation,
            PlayerPrefs.GetFloat("Graphics.Saturation", 1f));

        SetSlider(
            sliderFieldOfView,
            PlayerPrefs.GetFloat("Graphics.FieldOfView", 60f));

        RefreshSelectors();
    }

    private int FindFrameRateIndex(int frameRate)
    {
        int index = Array.IndexOf(frameRates, frameRate);
        return index >= 0 ? index : 1;
    }

    private void PreviousFrameRate()
    {
        frameRateIndex = WrapIndex(
            frameRateIndex - 1,
            frameRates.Length);

        RefreshSelectors();
    }

    private void NextFrameRate()
    {
        frameRateIndex = WrapIndex(
            frameRateIndex + 1,
            frameRates.Length);

        RefreshSelectors();
    }

    private void PreviousDisplayMode()
    {
        displayModeIndex = WrapIndex(
            displayModeIndex - 1,
            displayModes.Length);

        RefreshSelectors();
    }

    private void NextDisplayMode()
    {
        displayModeIndex = WrapIndex(
            displayModeIndex + 1,
            displayModes.Length);

        RefreshSelectors();
    }

    private void PreviousSharpening()
    {
        sharpeningIndex = WrapIndex(
            sharpeningIndex - 1,
            sharpeningModes.Length);

        RefreshSelectors();
    }

    private void NextSharpening()
    {
        sharpeningIndex = WrapIndex(
            sharpeningIndex + 1,
            sharpeningModes.Length);

        RefreshSelectors();
    }

    private void RefreshSelectors()
    {
        int frameRate = frameRates[frameRateIndex];

        frameRateSelector.SetText(
            frameRate < 0 ? "Unlimited" : frameRate.ToString());

        displayModeSelector.SetText(
            displayModes[displayModeIndex]);

        sharpeningSelector.SetText(
            sharpeningModes[sharpeningIndex]);
    }

    private int WrapIndex(int index, int length)
    {
        if (length <= 0)
            return 0;

        if (index < 0)
            return length - 1;

        if (index >= length)
            return 0;

        return index;
    }

    private void Confirm()
    {
        SaveSettings();
        ApplySettings();
    }

    private void SaveSettings()
    {
        PlayerPrefs.SetInt(
            "Graphics.FrameRate",
            frameRates[frameRateIndex]);

        PlayerPrefs.SetInt(
            "Graphics.DisplayMode",
            displayModeIndex);

        PlayerPrefs.SetInt(
            "Graphics.Sharpening",
            sharpeningIndex);

        SaveToggle("Graphics.VSync", toggleVSync);
        SaveToggle("Graphics.MotionBlur", toggleMotionBlur);
        SaveToggle(
            "Graphics.ChromaticAberration",
            toggleChromaticAberration);

        SaveSlider("Graphics.Brightness", sliderBrightness);
        SaveSlider("Graphics.Contrast", sliderContrast);
        SaveSlider("Graphics.Saturation", sliderSaturation);
        SaveSlider("Graphics.FieldOfView", sliderFieldOfView);

        if (dropdownResolution != null &&
            dropdownResolution.value < resolutions.Count)
        {
            Resolution resolution =
                resolutions[dropdownResolution.value];

            PlayerPrefs.SetInt(
                "Graphics.ResolutionWidth",
                resolution.width);

            PlayerPrefs.SetInt(
                "Graphics.ResolutionHeight",
                resolution.height);
        }

        PlayerPrefs.Save();
    }

    private void ApplySettings()
    {
        QualitySettings.vSyncCount =
            toggleVSync != null && toggleVSync.isOn ? 1 : 0;

        Application.targetFrameRate =
            frameRates[frameRateIndex];

        if (dropdownResolution == null ||
            dropdownResolution.value >= resolutions.Count)
            return;

        Resolution resolution =
            resolutions[dropdownResolution.value];

        Screen.SetResolution(
            resolution.width,
            resolution.height,
            GetFullScreenMode());
    }

    private FullScreenMode GetFullScreenMode()
    {
        switch (displayModeIndex)
        {
            case 0:
                return FullScreenMode.ExclusiveFullScreen;

            case 1:
                return FullScreenMode.FullScreenWindow;

            default:
                return FullScreenMode.Windowed;
        }
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

    private void SelectGraphics()
    {
        tabs.SetSelected(MenuType.GraphicsMenu);
    }

    private void OpenController()
    {
        UIManager.Instance.ChangeMenu(MenuType.ControllerMenu);
    }

    private void SetToggle(Toggle toggle, bool value)
    {
        if (toggle != null)
            toggle.SetIsOnWithoutNotify(value);
    }

    private void SetSlider(Slider slider, float value)
    {
        if (slider != null)
            slider.SetValueWithoutNotify(value);
    }

    private void SaveToggle(string key, Toggle toggle)
    {
        if (toggle != null)
            PlayerPrefs.SetInt(key, toggle.isOn ? 1 : 0);
    }

    private void SaveSlider(string key, Slider slider)
    {
        if (slider != null)
            PlayerPrefs.SetFloat(key, slider.value);
    }

    private void AddButton(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button != null)
            button.onClick.AddListener(action);
    }

    private void RemoveButton(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button != null)
            button.onClick.RemoveListener(action);
    }
}