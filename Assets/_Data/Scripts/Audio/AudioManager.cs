using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : Singleton<AudioManager>
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Mixer")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string musicVolumeParameter = "MusicVolume";
    [SerializeField] private string sfxVolumeParameter = "SFXVolume";

    [Header("Defaults")]
    [SerializeField, Range(0f, 1f)] private float defaultMusicVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float defaultSfxVolume = 1f;

    private const string MusicVolumeKey = "Audio_MusicVolume";
    private const string SfxVolumeKey = "Audio_SFXVolume";
    private const string AudioDataResourcesPath = "ScriptableObjects/Audio";
    private const float MinLinearVolume = 0.0001f;
    [ShowInInspector] private readonly Dictionary<AudioClipName, AudioClip> audioClipLookup = new Dictionary<AudioClipName, AudioClip>();
    private bool isAudioDatabaseLoaded;

    public float MusicVolumeLinear { get; private set; } = 1f;
    public float SfxVolumeLinear { get; private set; } = 1f;
    public AudioSource MusicSource => musicSource;
    public AudioSource SfxSource => sfxSource;

    private void Reset()
    {
        EnsureAudioSources();
    }

    private void Start()
    {
        EnsureAudioSources();
        LoadAudioDatabase();
        LoadVolumeSettings();
    }

    public void PlayMusic(AudioClipName clipName, bool loop = true)
    {
        if (musicSource == null)
        {
            Debug.LogWarning("Music AudioSource is missing on AudioManager.", this);
            return;
        }

        if (!TryGetAudioClip(clipName, out AudioClip clip))
            return;

        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();
    }

    public void PlaySfx(AudioClipName clipName)
    {
        if (sfxSource == null)
        {
            Debug.LogWarning("SFX AudioSource is missing on AudioManager.", this);
            return;
        }

        if (!TryGetAudioClip(clipName, out AudioClip clip))
            return;

        sfxSource.PlayOneShot(clip);
    }

    public void SetMusicVolume(float linearVolume)
    {
        MusicVolumeLinear = Mathf.Clamp01(linearVolume);
        ApplyVolumeToMixer(musicVolumeParameter, MusicVolumeLinear);

        PlayerPrefs.SetFloat(MusicVolumeKey, MusicVolumeLinear);
        PlayerPrefs.Save();
    }

    public void SetSfxVolume(float linearVolume)
    {
        SfxVolumeLinear = Mathf.Clamp01(linearVolume);
        ApplyVolumeToMixer(sfxVolumeParameter, SfxVolumeLinear);

        PlayerPrefs.SetFloat(SfxVolumeKey, SfxVolumeLinear);
        PlayerPrefs.Save();
    }

    public float GetMusicVolume()
    {
        return MusicVolumeLinear;
    }

    public float GetSfxVolume()
    {
        return SfxVolumeLinear;
    }

    public void LoadVolumeSettings()
    {
        MusicVolumeLinear = PlayerPrefs.GetFloat(MusicVolumeKey, defaultMusicVolume);
        SfxVolumeLinear = PlayerPrefs.GetFloat(SfxVolumeKey, defaultSfxVolume);

        ApplyVolumeToMixer(musicVolumeParameter, MusicVolumeLinear);
        ApplyVolumeToMixer(sfxVolumeParameter, SfxVolumeLinear);
    }

    private void LoadAudioDatabase()
    {
        audioClipLookup.Clear();

        AudioClipDataSO[] audioData = Resources.LoadAll<AudioClipDataSO>(AudioDataResourcesPath);
        foreach (AudioClipDataSO data in audioData)
        {
            if (data == null || data.ClipName == AudioClipName.None || data.Clip == null)
                continue;

            if (audioClipLookup.ContainsKey(data.ClipName))
                Debug.LogWarning($"Duplicate audio mapping for {data.ClipName}. The latest asset will override previous mapping.", data);

            audioClipLookup[data.ClipName] = data.Clip;
        }

        isAudioDatabaseLoaded = true;
    }

    private bool TryGetAudioClip(AudioClipName clipName, out AudioClip clip)
    {
        if (!isAudioDatabaseLoaded)
            LoadAudioDatabase();

        if (clipName == AudioClipName.None)
        {
            clip = null;
            return false;
        }

        if (audioClipLookup.TryGetValue(clipName, out clip) && clip != null)
            return true;

        Debug.LogWarning($"No audio clip is mapped for enum value {clipName}.", this);
        return false;
    }

    private void EnsureAudioSources()
    {
        AudioSource[] sources = GetComponents<AudioSource>();

        if (musicSource == null && sources.Length > 0)
            musicSource = sources[0];

        if (sfxSource == null)
        {
            if (sources.Length > 1)
                sfxSource = sources[1];
            else if (sources.Length > 0)
                sfxSource = sources[0];
        }
    }

    private void ApplyVolumeToMixer(string exposedParameter, float linearVolume)
    {
        if (audioMixer == null || string.IsNullOrWhiteSpace(exposedParameter))
            return;

        float dbVolume = Mathf.Log10(Mathf.Max(MinLinearVolume, linearVolume)) * 20f;
        audioMixer.SetFloat(exposedParameter, dbVolume);
    }

}
