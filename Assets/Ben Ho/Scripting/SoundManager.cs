using System;
using UnityEngine;

[Obsolete("SoundManager is kept only for compatibility. Use AudioManager instead.")]
public class SoundManager : AudioManager
{
    public new static SoundManager Instance => AudioManager.Instance as SoundManager;

    public AudioSource bgMusicSource => MusicSource;
    public AudioSource sfxSource => SfxSource;
    public AudioSource dialogueSource => DialogueSource;

    public void PlaySFX(AudioClip clip)
    {
        PlaySfx(clip);
    }

    public void LoadAudioSettings()
    {
        LoadVolumeSettings();
    }

    public void SetVolume(string parameterName, float value)
    {
        switch (parameterName)
        {
            case "MasterVolume":
                SetMasterVolume(value);
                break;
            case "MusicVolume":
                SetMusicVolume(value);
                break;
            case "DialogueVolume":
                SetDialogueVolume(value);
                break;
            default:
                SetSfxVolume(value);
                break;
        }
    }
}
