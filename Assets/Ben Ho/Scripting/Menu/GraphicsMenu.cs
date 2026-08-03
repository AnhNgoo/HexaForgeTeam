using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
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

public class GraphicsMenu : MenuBase
{
    public override MenuType menuType => MenuType.GraphicsMenu;

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

    [Header("Optional Runtime Targets")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Behaviour motionBlurComponent;
    [SerializeField] private Behaviour chromaticAberrationComponent;
    [SerializeField] private Behaviour sharpeningComponent;

    [Header("Buttons")]
    [SerializeField] private Button btnConfirm;
    [SerializeField] private Button btnBack;

    [Header("Embedded System Settings")]
    [SerializeField] private SystemSettingsPanel systemSettingsPanel;

    [Header("Selector Values")]
    [SerializeField] private int[] frameRates = { 30, 60, 120, 144, 165, 240, -1 };
    [SerializeField] private string[] displayModes = { "Full Screen", "Borderless", "Windowed" };
    [SerializeField] private string[] sharpeningModes = { "Off", "Fidelity FX" };

    private readonly List<Vector2Int> resolutions = new List<Vector2Int>();
    private bool eventsAdded;

    private int frameRateIndex;
    private int displayModeIndex;
    private int sharpeningIndex;

    protected override void LoadComponent() { }
    protected override void LoadComponentRuntime() { }

    public override void Open(object data = null)
    {
        base.Open(data);
        Initialize();
    }

    public override void Close()
    {
        RemoveEvents();
        base.Close();
    }

    private void OnEnable()
    {
        Initialize();
    }

    private void OnDisable()
    {
        RemoveEvents();
    }

    private void Initialize()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        RemoveEvents();
        BuildResolutionDropdown();
        LoadSettings();
        AddEvents();
        ApplyPreviewOnly();
    }

    private void BuildResolutionDropdown()
    {
        if (dropdownResolution == null)
            return;

        resolutions.Clear();

        AddResolution(1280, 720);
        AddResolution(1366, 768);
        AddResolution(1600, 900);
        AddResolution(1680, 1050);
        AddResolution(1920, 1080);
        AddResolution(2560, 1440);
        AddResolution(3840, 2160);

        foreach (Resolution resolution in Screen.resolutions)
            AddResolution(resolution.width, resolution.height);

        resolutions.Sort((a, b) =>
        {
            int widthCompare = a.x.CompareTo(b.x);
            return widthCompare != 0 ? widthCompare : a.y.CompareTo(b.y);
        });

        List<string> options = new List<string>();

        foreach (Vector2Int resolution in resolutions)
            options.Add($"{resolution.x} x {resolution.y}");

        dropdownResolution.ClearOptions();
        dropdownResolution.AddOptions(options);

        int savedWidth = PlayerPrefs.GetInt("Graphics.ResolutionWidth", Screen.width);
        int savedHeight = PlayerPrefs.GetInt("Graphics.ResolutionHeight", Screen.height);

        int selectedIndex = resolutions.FindIndex(item =>
            item.x == savedWidth && item.y == savedHeight);

        if (selectedIndex < 0)
            selectedIndex = resolutions.FindIndex(item =>
                item.x == Screen.width && item.y == Screen.height);

        dropdownResolution.SetValueWithoutNotify(Mathf.Max(0, selectedIndex));
        dropdownResolution.RefreshShownValue();
    }

    private void AddResolution(int width, int height)
    {
        if (width <= 0 || height <= 0)
            return;

        bool exists = resolutions.Exists(item => item.x == width && item.y == height);

        if (!exists)
            resolutions.Add(new Vector2Int(width, height));
    }

    private void AddEvents()
    {
        if (eventsAdded)
            return;

        AddButton(frameRateSelector.leftButton, PreviousFrameRate);
        AddButton(frameRateSelector.rightButton, NextFrameRate);

        AddButton(displayModeSelector.leftButton, PreviousDisplayMode);
        AddButton(displayModeSelector.rightButton, NextDisplayMode);

        AddButton(sharpeningSelector.leftButton, PreviousSharpening);
        AddButton(sharpeningSelector.rightButton, NextSharpening);

        if (dropdownResolution != null)
            dropdownResolution.onValueChanged.AddListener(OnResolutionChanged);

        AddToggle(toggleVSync);
        AddToggle(toggleMotionBlur);
        AddToggle(toggleChromaticAberration);

        AddSlider(sliderBrightness);
        AddSlider(sliderContrast);
        AddSlider(sliderSaturation);
        AddSlider(sliderFieldOfView);

        AddButton(btnConfirm, Confirm);
        AddButton(btnBack, Back);

        eventsAdded = true;
    }

    private void RemoveEvents()
    {
        if (!eventsAdded)
            return;

        RemoveButton(frameRateSelector.leftButton, PreviousFrameRate);
        RemoveButton(frameRateSelector.rightButton, NextFrameRate);

        RemoveButton(displayModeSelector.leftButton, PreviousDisplayMode);
        RemoveButton(displayModeSelector.rightButton, NextDisplayMode);

        RemoveButton(sharpeningSelector.leftButton, PreviousSharpening);
        RemoveButton(sharpeningSelector.rightButton, NextSharpening);

        if (dropdownResolution != null)
            dropdownResolution.onValueChanged.RemoveListener(OnResolutionChanged);

        RemoveToggle(toggleVSync);
        RemoveToggle(toggleMotionBlur);
        RemoveToggle(toggleChromaticAberration);

        RemoveSlider(sliderBrightness);
        RemoveSlider(sliderContrast);
        RemoveSlider(sliderSaturation);
        RemoveSlider(sliderFieldOfView);

        RemoveButton(btnConfirm, Confirm);
        RemoveButton(btnBack, Back);

        eventsAdded = false;
    }

    private void LoadSettings()
    {
        frameRateIndex = FindIndex(frameRates, PlayerPrefs.GetInt("Graphics.FrameRate", 60), 1);

        displayModeIndex = Mathf.Clamp(
            PlayerPrefs.GetInt("Graphics.DisplayMode", 1),
            0,
            displayModes.Length - 1);

        sharpeningIndex = Mathf.Clamp(
            PlayerPrefs.GetInt("Graphics.Sharpening", 0),
            0,
            sharpeningModes.Length - 1);

        SetToggle(toggleVSync, PlayerPrefs.GetInt("Graphics.VSync", 0) == 1);
        SetToggle(toggleMotionBlur, PlayerPrefs.GetInt("Graphics.MotionBlur", 0) == 1);
        SetToggle(toggleChromaticAberration, PlayerPrefs.GetInt("Graphics.ChromaticAberration", 0) == 1);

        SetSlider(sliderBrightness, PlayerPrefs.GetFloat("Graphics.Brightness", 1f));
        SetSlider(sliderContrast, PlayerPrefs.GetFloat("Graphics.Contrast", 1f));
        SetSlider(sliderSaturation, PlayerPrefs.GetFloat("Graphics.Saturation", 1f));
        SetSlider(sliderFieldOfView, PlayerPrefs.GetFloat("Graphics.FieldOfView", 60f));

        RefreshSelectors();
    }

    private void SaveSettings()
    {
        PlayerPrefs.SetInt("Graphics.FrameRate", frameRates[frameRateIndex]);
        PlayerPrefs.SetInt("Graphics.DisplayMode", displayModeIndex);
        PlayerPrefs.SetInt("Graphics.Sharpening", sharpeningIndex);

        SaveToggle("Graphics.VSync", toggleVSync);
        SaveToggle("Graphics.MotionBlur", toggleMotionBlur);
        SaveToggle("Graphics.ChromaticAberration", toggleChromaticAberration);

        SaveSlider("Graphics.Brightness", sliderBrightness);
        SaveSlider("Graphics.Contrast", sliderContrast);
        SaveSlider("Graphics.Saturation", sliderSaturation);
        SaveSlider("Graphics.FieldOfView", sliderFieldOfView);

        if (dropdownResolution != null && dropdownResolution.value >= 0 && dropdownResolution.value < resolutions.Count)
        {
            Vector2Int resolution = resolutions[dropdownResolution.value];

            PlayerPrefs.SetInt("Graphics.ResolutionWidth", resolution.x);
            PlayerPrefs.SetInt("Graphics.ResolutionHeight", resolution.y);
        }

        PlayerPrefs.Save();
    }

    private void Confirm()
    {
        SaveSettings();
        ApplySettings();
    }

    private void ApplySettings()
    {
        QualitySettings.vSyncCount = toggleVSync != null && toggleVSync.isOn ? 1 : 0;
        Application.targetFrameRate = frameRates[frameRateIndex];

        if (dropdownResolution != null && dropdownResolution.value >= 0 && dropdownResolution.value < resolutions.Count)
        {
            Vector2Int resolution = resolutions[dropdownResolution.value];

            Screen.SetResolution(
                resolution.x,
                resolution.y,
                GetFullScreenMode());
        }

        ApplyPreviewOnly();
    }

    private void ApplyPreviewOnly()
    {
        if (targetCamera != null && sliderFieldOfView != null)
            targetCamera.fieldOfView = sliderFieldOfView.value;

        if (motionBlurComponent != null && toggleMotionBlur != null)
            motionBlurComponent.enabled = toggleMotionBlur.isOn;

        if (chromaticAberrationComponent != null && toggleChromaticAberration != null)
            chromaticAberrationComponent.enabled = toggleChromaticAberration.isOn;

        if (sharpeningComponent != null)
            sharpeningComponent.enabled = sharpeningIndex > 0;
    }

    private void Back()
    {
        LoadSettings();
        ApplyPreviewOnly();

        if (systemSettingsPanel != null)
            systemSettingsPanel.CloseGameSystemMenu();
        else if (UIManager.Instance != null)
            UIManager.Instance.ChangeMenu(SettingMenuData.BackMenu);
    }

    private void OnResolutionChanged(int value)
    {
        // Chi apply that su khi bam Confirm.
    }

    private void PreviousFrameRate()
    {
        frameRateIndex = Wrap(frameRateIndex - 1, frameRates.Length);
        RefreshSelectors();
    }

    private void NextFrameRate()
    {
        frameRateIndex = Wrap(frameRateIndex + 1, frameRates.Length);
        RefreshSelectors();
    }

    private void PreviousDisplayMode()
    {
        displayModeIndex = Wrap(displayModeIndex - 1, displayModes.Length);
        RefreshSelectors();
    }

    private void NextDisplayMode()
    {
        displayModeIndex = Wrap(displayModeIndex + 1, displayModes.Length);
        RefreshSelectors();
    }

    private void PreviousSharpening()
    {
        sharpeningIndex = Wrap(sharpeningIndex - 1, sharpeningModes.Length);
        RefreshSelectors();
    }

    private void NextSharpening()
    {
        sharpeningIndex = Wrap(sharpeningIndex + 1, sharpeningModes.Length);
        RefreshSelectors();
    }

    private void RefreshSelectors()
    {
        int frameRate = frameRates[Mathf.Clamp(frameRateIndex, 0, frameRates.Length - 1)];

        frameRateSelector.SetText(frameRate < 0 ? "Unlimited" : frameRate.ToString());
        displayModeSelector.SetText(displayModes[Mathf.Clamp(displayModeIndex, 0, displayModes.Length - 1)]);
        sharpeningSelector.SetText(sharpeningModes[Mathf.Clamp(sharpeningIndex, 0, sharpeningModes.Length - 1)]);
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

    private int FindIndex(int[] values, int target, int fallback)
    {
        int index = Array.IndexOf(values, target);
        return index >= 0 ? index : fallback;
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

    private void AddToggle(Toggle toggle)
    {
        if (toggle != null)
            toggle.onValueChanged.AddListener(OnVisualSettingChanged);
    }

    private void RemoveToggle(Toggle toggle)
    {
        if (toggle != null)
            toggle.onValueChanged.RemoveListener(OnVisualSettingChanged);
    }

    private void AddSlider(Slider slider)
    {
        if (slider != null)
            slider.onValueChanged.AddListener(OnSliderChanged);
    }

    private void RemoveSlider(Slider slider)
    {
        if (slider != null)
            slider.onValueChanged.RemoveListener(OnSliderChanged);
    }

    private void OnVisualSettingChanged(bool value)
    {
        ApplyPreviewOnly();
    }

    private void OnSliderChanged(float value)
    {
        ApplyPreviewOnly();
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
}