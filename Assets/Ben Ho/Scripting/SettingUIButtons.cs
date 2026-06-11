using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class SettingUIButtons : MonoBehaviour
{
    [Header("Audio Mixer")]
    public AudioMixer audioMixer;

    [Header("Volume Sliders")]
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider soundEffectsVolumeSlider;
    public Slider dialogueVolumeSlider;

    [Header("Toggle Buttons")]
    public Toggle backgroundSoundToggle;
    public Toggle collisionSoundToggle;

    [Header("Audio Mixer Parameter Names")]
    public string masterVolumeParam = "MasterVolume";
    public string musicVolumeParam = "MusicVolume";
    public string soundEffectsVolumeParam = "SoundEffectsVolume";
    public string dialogueVolumeParam = "DialogueVolume";
    public string collisionVolumeParam = "CollisionVolume";

    [Header("Back Setting")]
    public GameObject settingPanel;
    public string backSceneName = "UI Menu";

    private void Start()
    {
        LoadSettings();

        masterVolumeSlider.onValueChanged.AddListener(delegate { PreviewSettings(); });
        musicVolumeSlider.onValueChanged.AddListener(delegate { PreviewSettings(); });
        soundEffectsVolumeSlider.onValueChanged.AddListener(delegate { PreviewSettings(); });
        dialogueVolumeSlider.onValueChanged.AddListener(delegate { PreviewSettings(); });

        backgroundSoundToggle.onValueChanged.AddListener(delegate { PreviewSettings(); });
        collisionSoundToggle.onValueChanged.AddListener(delegate { PreviewSettings(); });

        PreviewSettings();
    }

    private void LoadSettings()
    {
        masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
        musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
        soundEffectsVolumeSlider.value = PlayerPrefs.GetFloat("SoundEffectsVolume", 1f);
        dialogueVolumeSlider.value = PlayerPrefs.GetFloat("DialogueVolume", 1f);

        backgroundSoundToggle.isOn = PlayerPrefs.GetInt("BackgroundSoundOn", 1) == 1;
        collisionSoundToggle.isOn = PlayerPrefs.GetInt("CollisionSoundOn", 1) == 1;
    }

    private void PreviewSettings()
    {
        SetMixerVolume(masterVolumeParam, masterVolumeSlider.value);
        SetMixerVolume(soundEffectsVolumeParam, soundEffectsVolumeSlider.value);
        SetMixerVolume(dialogueVolumeParam, dialogueVolumeSlider.value);

        if (backgroundSoundToggle.isOn)
        {
            SetMixerVolume(musicVolumeParam, musicVolumeSlider.value);
        }
        else
        {
            audioMixer.SetFloat(musicVolumeParam, -80f);
        }

        if (collisionSoundToggle.isOn)
        {
            SetMixerVolume(collisionVolumeParam, 1f);
        }
        else
        {
            audioMixer.SetFloat(collisionVolumeParam, -80f);
        }
    }

    private void SetMixerVolume(string parameterName, float value)
    {
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

    public void ConfirmButton()
    {
        PlayerPrefs.SetFloat("MasterVolume", masterVolumeSlider.value);
        PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider.value);
        PlayerPrefs.SetFloat("SoundEffectsVolume", soundEffectsVolumeSlider.value);
        PlayerPrefs.SetFloat("DialogueVolume", dialogueVolumeSlider.value);

        PlayerPrefs.SetInt("BackgroundSoundOn", backgroundSoundToggle.isOn ? 1 : 0);
        PlayerPrefs.SetInt("CollisionSoundOn", collisionSoundToggle.isOn ? 1 : 0);

        PlayerPrefs.Save();

        PreviewSettings();

        Debug.Log("Đã lưu cài đặt âm thanh.");
    }

    public void BackButton()
    {
        LoadSettings();
        PreviewSettings();

        SceneManager.LoadScene(SettingReturnData.BackSceneName);
    }
}