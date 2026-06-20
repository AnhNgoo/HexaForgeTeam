using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SettingMenu : MenuBase
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

    protected override void LoadComponent()
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

    protected override void LoadComponentRuntime()
    {

    }

    public override void Open(object data = null)
    {
        base.Open(data);

        LoadSettings();
        AddEvents();
        PreviewSettings();
    }

    public override void Close()
    {
        RemoveEvents();

        base.Close();
    }

    private void AddEvents()
    {
        if (slider_MasterVolume != null)
            slider_MasterVolume.onValueChanged.AddListener(OnVolumeChanged);

        if (slider_MusicVolume != null)
            slider_MusicVolume.onValueChanged.AddListener(OnVolumeChanged);

        if (slider_SFXVolume != null)
            slider_SFXVolume.onValueChanged.AddListener(OnVolumeChanged);

        if (slider_DialogueVolume != null)
            slider_DialogueVolume.onValueChanged.AddListener(OnVolumeChanged);

        if (toggle_BackgroundSound != null)
            toggle_BackgroundSound.onValueChanged.AddListener(OnToggleChanged);

        if (toggle_CollisionSound != null)
            toggle_CollisionSound.onValueChanged.AddListener(OnToggleChanged);

        if (btn_Confirm != null)
            btn_Confirm.onClick.AddListener(OnConfirmButtonClicked);

        if (btn_Back != null)
            btn_Back.onClick.AddListener(OnBackButtonClicked);
    }

    private void RemoveEvents()
    {
        if (slider_MasterVolume != null)
            slider_MasterVolume.onValueChanged.RemoveListener(OnVolumeChanged);

        if (slider_MusicVolume != null)
            slider_MusicVolume.onValueChanged.RemoveListener(OnVolumeChanged);

        if (slider_SFXVolume != null)
            slider_SFXVolume.onValueChanged.RemoveListener(OnVolumeChanged);

        if (slider_DialogueVolume != null)
            slider_DialogueVolume.onValueChanged.RemoveListener(OnVolumeChanged);

        if (toggle_BackgroundSound != null)
            toggle_BackgroundSound.onValueChanged.RemoveListener(OnToggleChanged);

        if (toggle_CollisionSound != null)
            toggle_CollisionSound.onValueChanged.RemoveListener(OnToggleChanged);

        if (btn_Confirm != null)
            btn_Confirm.onClick.RemoveListener(OnConfirmButtonClicked);

        if (btn_Back != null)
            btn_Back.onClick.RemoveListener(OnBackButtonClicked);
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
        if (slider_MasterVolume != null)
            slider_MasterVolume.value = PlayerPrefs.GetFloat("MasterVolume", 1f);

        if (slider_MusicVolume != null)
            slider_MusicVolume.value = PlayerPrefs.GetFloat("MusicVolume", 1f);

        if (slider_SFXVolume != null)
            slider_SFXVolume.value = PlayerPrefs.GetFloat("SoundEffectsVolume", 1f);

        if (slider_DialogueVolume != null)
            slider_DialogueVolume.value = PlayerPrefs.GetFloat("DialogueVolume", 1f);

        if (toggle_BackgroundSound != null)
            toggle_BackgroundSound.isOn = PlayerPrefs.GetInt("BackgroundSoundOn", 1) == 1;

        if (toggle_CollisionSound != null)
            toggle_CollisionSound.isOn = PlayerPrefs.GetInt("CollisionSoundOn", 1) == 1;
    }

    private void PreviewSettings()
    {
        if (audioMixer == null) return;

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

    private float GetSliderValue(Slider slider)
    {
        if (slider == null) return 1f;
        return slider.value;
    }

    private void SetMixerVolume(string parameterName, float value)
    {
        if (audioMixer == null) return;

        if (value <= 0.0001f)
        {
            audioMixer.SetFloat(parameterName, -80f);
        }
        else
        {
            float dbValue = Mathf.Log10(value) * 20f;
            audioMixer.SetFloat(parameterName, dbValue);
        }
    }

    private void OnConfirmButtonClicked()
    {
        SaveSettings();
        PreviewSettings();

        Debug.Log("Setting confirmed");
    }

    private void SaveSettings()
    {
        if (slider_MasterVolume != null)
            PlayerPrefs.SetFloat("MasterVolume", slider_MasterVolume.value);

        if (slider_MusicVolume != null)
            PlayerPrefs.SetFloat("MusicVolume", slider_MusicVolume.value);

        if (slider_SFXVolume != null)
            PlayerPrefs.SetFloat("SoundEffectsVolume", slider_SFXVolume.value);

        if (slider_DialogueVolume != null)
            PlayerPrefs.SetFloat("DialogueVolume", slider_DialogueVolume.value);

        if (toggle_BackgroundSound != null)
            PlayerPrefs.SetInt("BackgroundSoundOn", toggle_BackgroundSound.isOn ? 1 : 0);

        if (toggle_CollisionSound != null)
            PlayerPrefs.SetInt("CollisionSoundOn", toggle_CollisionSound.isOn ? 1 : 0);

        PlayerPrefs.Save();
    }

    private void OnBackButtonClicked()
    {
        Debug.Log("Setting back button clicked");

        LoadSettings();
        PreviewSettings();

        UIManager.Instance.ChangeMenu(SettingMenuData.BackMenu);
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