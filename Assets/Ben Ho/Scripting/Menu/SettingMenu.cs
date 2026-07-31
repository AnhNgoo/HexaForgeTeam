using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioMenu : MenuBase
{
    public override MenuType menuType => MenuType.SettingMenu;

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("Volume Sliders")]
    [SerializeField] private Slider slider_MasterVolume;
    [SerializeField] private Slider slider_MusicVolume;
    [SerializeField] private Slider slider_SFXVolume;
    [SerializeField] private Slider slider_DialogueVolume;

    [Header("Sound Toggles")]
    [SerializeField] private Toggle toggle_BackgroundSound;
    [SerializeField] private Toggle toggle_CollisionSound;

    [Header("Buttons")]
    [SerializeField] private Button btn_Confirm;
    [SerializeField] private Button btn_Back;

    [Header("Audio Mixer Parameters")]
    [SerializeField] private string masterVolumeParam = "MasterVolume";
    [SerializeField] private string musicVolumeParam = "MusicVolume";
    [SerializeField] private string sfxVolumeParam = "SoundEffectsVolume";
    [SerializeField] private string dialogueVolumeParam = "DialogueVolume";
    [SerializeField] private string collisionVolumeParam = "CollisionVolume";

    [Header("Embedded System Settings")]
    [SerializeField] private SystemSettingsPanel systemSettingsPanel;

    private bool eventsAdded;

    protected override void LoadComponent()
    {
        AutoFindComponents();
    }

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
        AutoFindComponents();
        RemoveEvents();
        LoadSettings();
        AddEvents();
        PreviewSettings();
    }

    private void AutoFindComponents()
    {
        if (slider_MasterVolume == null)
            slider_MasterVolume = FindDeepChild("Slider_MasterVolume")?.GetComponent<Slider>();

        if (slider_MusicVolume == null)
            slider_MusicVolume = FindDeepChild("Slider_MusicVolume")?.GetComponent<Slider>();

        if (slider_SFXVolume == null)
            slider_SFXVolume = FindDeepChild("Slider_SFXVolume")?.GetComponent<Slider>();

        if (slider_DialogueVolume == null)
            slider_DialogueVolume = FindDeepChild("Slider_DialogueVolume")?.GetComponent<Slider>();

        if (toggle_BackgroundSound == null)
            toggle_BackgroundSound = FindDeepChild("Toggle_BackgroundSound")?.GetComponent<Toggle>();

        if (toggle_CollisionSound == null)
            toggle_CollisionSound = FindDeepChild("Toggle_CollisionSound")?.GetComponent<Toggle>();

        if (btn_Confirm == null)
            btn_Confirm = FindDeepChild("Btn_Confirm")?.GetComponent<Button>();

        if (btn_Back == null)
            btn_Back = FindDeepChild("Btn_Back")?.GetComponent<Button>();
    }

    private void AddEvents()
    {
        if (eventsAdded)
            return;

        AddSlider(slider_MasterVolume);
        AddSlider(slider_MusicVolume);
        AddSlider(slider_SFXVolume);
        AddSlider(slider_DialogueVolume);

        AddToggle(toggle_BackgroundSound);
        AddToggle(toggle_CollisionSound);

        if (btn_Confirm != null)
            btn_Confirm.onClick.AddListener(Confirm);

        if (btn_Back != null)
            btn_Back.onClick.AddListener(Back);

        eventsAdded = true;
    }

    private void RemoveEvents()
    {
        if (!eventsAdded)
            return;

        RemoveSlider(slider_MasterVolume);
        RemoveSlider(slider_MusicVolume);
        RemoveSlider(slider_SFXVolume);
        RemoveSlider(slider_DialogueVolume);

        RemoveToggle(toggle_BackgroundSound);
        RemoveToggle(toggle_CollisionSound);

        if (btn_Confirm != null)
            btn_Confirm.onClick.RemoveListener(Confirm);

        if (btn_Back != null)
            btn_Back.onClick.RemoveListener(Back);

        eventsAdded = false;
    }

    private void AddSlider(Slider slider)
    {
        if (slider != null)
            slider.onValueChanged.AddListener(OnVolumeChanged);
    }

    private void RemoveSlider(Slider slider)
    {
        if (slider != null)
            slider.onValueChanged.RemoveListener(OnVolumeChanged);
    }

    private void AddToggle(Toggle toggle)
    {
        if (toggle != null)
            toggle.onValueChanged.AddListener(OnToggleChanged);
    }

    private void RemoveToggle(Toggle toggle)
    {
        if (toggle != null)
            toggle.onValueChanged.RemoveListener(OnToggleChanged);
    }

    private void OnVolumeChanged(float value)
    {
        PreviewSettings();
    }

    private void OnToggleChanged(bool value)
    {
        PreviewSettings();
    }

    private void LoadSettings()
    {
        SetSlider(slider_MasterVolume, PlayerPrefs.GetFloat("Audio.MasterVolume", 1f));
        SetSlider(slider_MusicVolume, PlayerPrefs.GetFloat("Audio.MusicVolume", 1f));
        SetSlider(slider_SFXVolume, PlayerPrefs.GetFloat("Audio.SFXVolume", 1f));
        SetSlider(slider_DialogueVolume, PlayerPrefs.GetFloat("Audio.DialogueVolume", 1f));

        SetToggle(toggle_BackgroundSound, PlayerPrefs.GetInt("Audio.BackgroundSound", 1) == 1);
        SetToggle(toggle_CollisionSound, PlayerPrefs.GetInt("Audio.CollisionSound", 1) == 1);
    }

    private void SaveSettings()
    {
        SaveSlider("Audio.MasterVolume", slider_MasterVolume);
        SaveSlider("Audio.MusicVolume", slider_MusicVolume);
        SaveSlider("Audio.SFXVolume", slider_SFXVolume);
        SaveSlider("Audio.DialogueVolume", slider_DialogueVolume);

        SaveToggle("Audio.BackgroundSound", toggle_BackgroundSound);
        SaveToggle("Audio.CollisionSound", toggle_CollisionSound);

        PlayerPrefs.Save();
    }

    private void PreviewSettings()
    {
        if (audioMixer == null)
            return;

        SetMixerVolume(masterVolumeParam, GetSliderValue(slider_MasterVolume));
        SetMixerVolume(sfxVolumeParam, GetSliderValue(slider_SFXVolume));
        SetMixerVolume(dialogueVolumeParam, GetSliderValue(slider_DialogueVolume));

        if (toggle_BackgroundSound != null && !toggle_BackgroundSound.isOn)
            audioMixer.SetFloat(musicVolumeParam, -80f);
        else
            SetMixerVolume(musicVolumeParam, GetSliderValue(slider_MusicVolume));

        if (toggle_CollisionSound != null && !toggle_CollisionSound.isOn)
            audioMixer.SetFloat(collisionVolumeParam, -80f);
        else
            SetMixerVolume(collisionVolumeParam, 1f);
    }

    private void SetMixerVolume(string parameterName, float value)
    {
        if (audioMixer == null)
            return;

        float dbValue = value <= 0.0001f ? -80f : Mathf.Log10(value) * 20f;
        audioMixer.SetFloat(parameterName, dbValue);
    }

    private float GetSliderValue(Slider slider)
    {
        return slider != null ? slider.value : 1f;
    }

    private void Confirm()
    {
        SaveSettings();
        PreviewSettings();
    }

    private void Back()
    {
        LoadSettings();
        PreviewSettings();

        if (systemSettingsPanel != null)
            systemSettingsPanel.CloseGameSystemMenu();
        else if (UIManager.Instance != null)
            UIManager.Instance.ChangeMenu(SettingMenuData.BackMenu);
    }

    private void SetSlider(Slider slider, float value)
    {
        if (slider != null)
            slider.SetValueWithoutNotify(value);
    }

    private void SetToggle(Toggle toggle, bool value)
    {
        if (toggle != null)
            toggle.SetIsOnWithoutNotify(value);
    }

    private void SaveSlider(string key, Slider slider)
    {
        if (slider != null)
            PlayerPrefs.SetFloat(key, slider.value);
    }

    private void SaveToggle(string key, Toggle toggle)
    {
        if (toggle != null)
            PlayerPrefs.SetInt(key, toggle.isOn ? 1 : 0);
    }

    private Transform FindDeepChild(string childName)
    {
        Transform[] children = GetComponentsInChildren<Transform>(true);

        foreach (Transform child in children)
        {
            if (child.name == childName)
                return child;
        }

        return null;
    }
}