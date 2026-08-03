using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;

public class AudioManager : Singleton<AudioManager>
{
    [Header("Lifetime")]
    [SerializeField] private bool persistAcrossScenes = true;

    [Header("Audio Sources")]
    [FormerlySerializedAs("bgMusicSource")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource dialogueSource;

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private AudioMixerGroup musicOutputGroup;
    [SerializeField] private AudioMixerGroup sfxOutputGroup;
    [SerializeField] private AudioMixerGroup dialogueOutputGroup;

    [Header("Exposed Mixer Parameters")]
    [SerializeField] private string masterVolumeParam = "MasterVolume";
    [FormerlySerializedAs("musicVolumeParameter")]
    [SerializeField] private string musicVolumeParam = "MusicVolume";
    [FormerlySerializedAs("sfxVolumeParameter")]
    [SerializeField] private string sfxVolumeParam = "SoundEffectsVolume";
    [SerializeField] private string dialogueVolumeParam = "DialogueVolume";

    [Header("Default Volume")]
    [SerializeField, Range(0f, 1f)] private float defaultMasterVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float defaultMusicVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float defaultSfxVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float defaultDialogueVolume = 1f;

    private const string AudioDataResourcesPath = "ScriptableObjects/Audio";
    private const float MutedDecibels = -80f;
    private const float MinLinearVolume = 0.0001f;

    private const string MasterVolumeKey = "Audio.MasterVolume";
    private const string MusicVolumeKey = "Audio.MusicVolume";
    private const string SfxVolumeKey = "Audio.SFXVolume";
    private const string DialogueVolumeKey = "Audio.DialogueVolume";

    private const string MasterMutedKey = "Audio.MasterMuted";
    private const string MusicEnabledKey = "Audio.BackgroundSound";
    private const string SfxMutedKey = "Audio.SFXMuted";
    private const string DialogueMutedKey = "Audio.DialogueMuted";

    private readonly Dictionary<AudioClipName, AudioClipDataSO> audioClipLookup =
        new Dictionary<AudioClipName, AudioClipDataSO>();

    private bool isInitialized;
    private bool isAudioDatabaseLoaded;
    private bool masterMuted;
    private bool musicMuted;
    private bool sfxMuted;
    private bool dialogueMuted;
    private float currentMusicVolumeScale = 1f;

    public float MasterVolumeLinear { get; private set; } = 1f;
    public float MusicVolumeLinear { get; private set; } = 1f;
    public float SfxVolumeLinear { get; private set; } = 1f;
    public float DialogueVolumeLinear { get; private set; } = 1f;

    public AudioSource MusicSource => musicSource;
    public AudioSource SfxSource => sfxSource;
    public AudioSource DialogueSource => dialogueSource;

    protected override void Awake()
    {
        isDontDestroyOnLoad = persistAcrossScenes;
        base.Awake();

        if (Instance != this)
            return;

        Initialize();
    }

    private void Start()
    {
        // Reapply after the mixer's startup snapshot has initialized.
        ApplyAllVolumes();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    protected override void LoadComponent()
    {
        AssignExistingAudioSources();
        ResolveMixerReferences();
    }

    protected override void LoadComponentRuntime()
    {
        Initialize();
    }

    public void Initialize()
    {
        if (isInitialized)
            return;

        EnsureRuntimeAudioSources();
        ResolveMixerReferences();
        LoadAudioDatabase();
        LoadVolumeSettings();
        isInitialized = true;
    }

    public bool Play(AudioClipName clipName)
    {
        if (!TryGetAudioData(clipName, out AudioClipDataSO data))
            return false;

        switch (data.Channel)
        {
            case AudioChannel.Music:
                PlayMusic(data.Clip, data.Loop, data.Volume);
                break;
            case AudioChannel.Dialogue:
                PlayDialogue(data.Clip, data.Volume);
                break;
            default:
                PlaySfx(data.Clip, data.Volume);
                break;
        }

        return true;
    }

    public void PlayMusic(AudioClipName clipName, bool loop = true)
    {
        if (TryGetAudioData(clipName, out AudioClipDataSO data))
            PlayMusic(data.Clip, loop, data.Volume);
    }

    public void PlayMusic(AudioClip clip, bool loop = true, float volume = 1f)
    {
        if (!CanPlay(musicSource, clip, "Music"))
            return;

        musicSource.loop = loop;
        currentMusicVolumeScale = Mathf.Clamp01(volume);
        ApplyMusicSourceVolume();

        if (musicSource.isPlaying && musicSource.clip == clip)
            return;

        musicSource.clip = clip;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource == null)
            return;

        musicSource.Stop();
        musicSource.clip = null;
    }

    public void PauseMusic()
    {
        if (musicSource != null)
            musicSource.Pause();
    }

    public void ResumeMusic()
    {
        if (musicSource != null)
            musicSource.UnPause();
    }

    public void PlaySfx(AudioClipName clipName)
    {
        if (TryGetAudioData(clipName, out AudioClipDataSO data))
            PlaySfx(data.Clip, data.Volume);
    }

    public void PlaySfx(AudioClip clip, float volume = 1f)
    {
        PlayOneShot(sfxSource, clip, AudioChannel.Sfx, volume, "SFX");
    }

    public void PlaySfx(AudioClipName clipName, AudioSource targetSource, float volume = 1f)
    {
        if (!TryGetAudioData(clipName, out AudioClipDataSO data))
            return;

        PlayOneShot(targetSource, data.Clip, AudioChannel.Sfx, data.Volume * volume, "SFX");
    }

    public void PlaySfx(AudioClip clip, AudioSource targetSource, float volume = 1f)
    {
        PlayOneShot(targetSource, clip, AudioChannel.Sfx, volume, "SFX");
    }

    public void PlayDialogue(AudioClipName clipName)
    {
        if (TryGetAudioData(clipName, out AudioClipDataSO data))
            PlayDialogue(data.Clip, data.Volume);
    }

    public void PlayDialogue(AudioClip clip, float volume = 1f)
    {
        PlayOneShot(dialogueSource, clip, AudioChannel.Dialogue, volume, "Dialogue");
    }

    public void StopDialogue()
    {
        if (dialogueSource != null)
            dialogueSource.Stop();
    }

    public void SetMasterVolume(float linearVolume)
    {
        SetVolume(AudioChannel.Master, linearVolume);
    }

    public void SetMusicVolume(float linearVolume)
    {
        SetVolume(AudioChannel.Music, linearVolume);
    }

    public void SetSfxVolume(float linearVolume)
    {
        SetVolume(AudioChannel.Sfx, linearVolume);
    }

    public void SetDialogueVolume(float linearVolume)
    {
        SetVolume(AudioChannel.Dialogue, linearVolume);
    }

    public void SetVolume(AudioChannel channel, float linearVolume, bool save = true)
    {
        float clampedVolume = Mathf.Clamp01(linearVolume);

        switch (channel)
        {
            case AudioChannel.Master:
                MasterVolumeLinear = clampedVolume;
                break;
            case AudioChannel.Music:
                MusicVolumeLinear = clampedVolume;
                break;
            case AudioChannel.Dialogue:
                DialogueVolumeLinear = clampedVolume;
                break;
            default:
                SfxVolumeLinear = clampedVolume;
                break;
        }

        ApplyVolume(channel);

        if (!save)
            return;

        PlayerPrefs.SetFloat(GetVolumeKey(channel), clampedVolume);
        PlayerPrefs.Save();
    }

    public float GetVolume(AudioChannel channel)
    {
        switch (channel)
        {
            case AudioChannel.Master:
                return MasterVolumeLinear;
            case AudioChannel.Music:
                return MusicVolumeLinear;
            case AudioChannel.Dialogue:
                return DialogueVolumeLinear;
            default:
                return SfxVolumeLinear;
        }
    }

    public float GetMusicVolume()
    {
        return MusicVolumeLinear;
    }

    public float GetSfxVolume()
    {
        return SfxVolumeLinear;
    }

    public void SetMuted(AudioChannel channel, bool muted, bool save = true)
    {
        switch (channel)
        {
            case AudioChannel.Master:
                masterMuted = muted;
                break;
            case AudioChannel.Music:
                musicMuted = muted;
                break;
            case AudioChannel.Dialogue:
                dialogueMuted = muted;
                break;
            default:
                sfxMuted = muted;
                break;
        }

        ApplyVolume(channel);

        if (!save)
            return;

        SaveMutedState(channel, muted);
        PlayerPrefs.Save();
    }

    public bool IsMuted(AudioChannel channel)
    {
        switch (channel)
        {
            case AudioChannel.Master:
                return masterMuted;
            case AudioChannel.Music:
                return musicMuted;
            case AudioChannel.Dialogue:
                return dialogueMuted;
            default:
                return sfxMuted;
        }
    }

    public void LoadVolumeSettings()
    {
        MasterVolumeLinear = LoadFloat(MasterVolumeKey, "MasterVolume", defaultMasterVolume);
        MusicVolumeLinear = LoadFloat(MusicVolumeKey, "MusicVolume", defaultMusicVolume);
        SfxVolumeLinear = LoadFloat(SfxVolumeKey, "SoundEffectsVolume", defaultSfxVolume);
        DialogueVolumeLinear = LoadFloat(DialogueVolumeKey, "DialogueVolume", defaultDialogueVolume);

        masterMuted = PlayerPrefs.GetInt(MasterMutedKey, 0) == 1;
        musicMuted = !LoadEnabledToggle(MusicEnabledKey, "BackgroundSoundOn");
        sfxMuted = PlayerPrefs.GetInt(SfxMutedKey, 0) == 1;
        dialogueMuted = PlayerPrefs.GetInt(DialogueMutedKey, 0) == 1;

        ApplyAllVolumes();
    }

    public bool TryGetClip(AudioClipName clipName, out AudioClip clip)
    {
        if (TryGetAudioData(clipName, out AudioClipDataSO data))
        {
            clip = data.Clip;
            return true;
        }

        clip = null;
        return false;
    }

    public void ReloadAudioDatabase()
    {
        isAudioDatabaseLoaded = false;
        LoadAudioDatabase();
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
                Debug.LogWarning($"Duplicate audio mapping for {data.ClipName}. The latest asset will be used.", data);

            audioClipLookup[data.ClipName] = data;
        }

        isAudioDatabaseLoaded = true;
    }

    private bool TryGetAudioData(AudioClipName clipName, out AudioClipDataSO data)
    {
        if (!isAudioDatabaseLoaded)
            LoadAudioDatabase();

        if (clipName != AudioClipName.None && audioClipLookup.TryGetValue(clipName, out data) && data != null)
            return true;

        data = null;

        if (clipName != AudioClipName.None)
            Debug.LogWarning($"No AudioClipDataSO is mapped for {clipName} in Resources/{AudioDataResourcesPath}.", this);

        return false;
    }

    private void EnsureRuntimeAudioSources()
    {
        AssignExistingAudioSources();

        musicSource = EnsureAudioSource(musicSource, "Music AudioSource", true);
        sfxSource = EnsureAudioSource(sfxSource, "SFX AudioSource", false);
        dialogueSource = EnsureAudioSource(dialogueSource, "Dialogue AudioSource", false);
    }

    private void AssignExistingAudioSources()
    {
        AudioSource[] sources = GetComponentsInChildren<AudioSource>(true);

        if (musicSource == null && sources.Length > 0)
            musicSource = sources[0];
        if (sfxSource == null && sources.Length > 1)
            sfxSource = sources[1];
        if (dialogueSource == null && sources.Length > 2)
            dialogueSource = sources[2];
    }

    private AudioSource EnsureAudioSource(AudioSource source, string objectName, bool loop)
    {
        if (source != null)
            return source;

        Transform existing = transform.Find(objectName);
        if (existing != null && existing.TryGetComponent(out AudioSource existingSource))
            return existingSource;

        GameObject sourceObject = new GameObject(objectName);
        sourceObject.transform.SetParent(transform, false);

        AudioSource createdSource = sourceObject.AddComponent<AudioSource>();
        createdSource.playOnAwake = false;
        createdSource.loop = loop;
        createdSource.spatialBlend = 0f;
        return createdSource;
    }

    private void ResolveMixerReferences()
    {
        if (audioMixer == null)
            audioMixer = FindMixerFromSources();

        if (audioMixer != null)
        {
            musicOutputGroup = ResolveGroup(musicOutputGroup, "Music");
            sfxOutputGroup = ResolveGroup(sfxOutputGroup, "SFX");
            dialogueOutputGroup = ResolveGroup(dialogueOutputGroup, "Dialogue");
        }

        AssignOutputGroup(musicSource, musicOutputGroup);
        AssignOutputGroup(sfxSource, sfxOutputGroup);
        AssignOutputGroup(dialogueSource, dialogueOutputGroup);
    }

    private AudioMixer FindMixerFromSources()
    {
        AudioSource[] sources = { musicSource, sfxSource, dialogueSource};

        foreach (AudioSource source in sources)
        {
            if (source != null && source.outputAudioMixerGroup != null)
                return source.outputAudioMixerGroup.audioMixer;
        }

        return null;
    }

    private AudioMixerGroup ResolveGroup(AudioMixerGroup currentGroup, string groupName)
    {
        if (currentGroup != null || audioMixer == null)
            return currentGroup;

        AudioMixerGroup[] matches = audioMixer.FindMatchingGroups(groupName);
        return matches.Length > 0 ? matches[0] : null;
    }

    private void AssignOutputGroup(AudioSource source, AudioMixerGroup outputGroup)
    {
        if (source != null && source.outputAudioMixerGroup == null && outputGroup != null)
            source.outputAudioMixerGroup = outputGroup;
    }

    private bool CanPlay(AudioSource source, AudioClip clip, string channelName)
    {
        if (source == null)
        {
            Debug.LogWarning($"{channelName} AudioSource is missing on AudioManager.", this);
            return false;
        }

        return clip != null;
    }

    private void PlayOneShot(AudioSource source, AudioClip clip, AudioChannel channel, float volume, string channelName)
    {
        if (!CanPlay(source, clip, channelName))
            return;

        if (source.outputAudioMixerGroup == null)
            source.outputAudioMixerGroup = GetOutputGroup(channel);

        float volumeScale = Mathf.Clamp01(volume);
        bool usesManagedSourceVolume = source == sfxSource || source == dialogueSource;

        if (source.outputAudioMixerGroup == null && !usesManagedSourceVolume)
            volumeScale *= GetEffectiveLinearVolume(channel);

        source.PlayOneShot(clip, volumeScale);
    }

    private AudioMixerGroup GetOutputGroup(AudioChannel channel)
    {
        switch (channel)
        {
            case AudioChannel.Music:
                return musicOutputGroup;
            case AudioChannel.Dialogue:
                return dialogueOutputGroup;
            default:
                return sfxOutputGroup;
        }
    }

    private float GetEffectiveLinearVolume(AudioChannel channel)
    {
        if (masterMuted || IsMuted(channel))
            return 0f;

        return MasterVolumeLinear * GetVolume(channel);
    }

    private void ApplyAllVolumes()
    {
        ApplyVolume(AudioChannel.Master);
        ApplyVolume(AudioChannel.Music);
        ApplyVolume(AudioChannel.Sfx);
        ApplyVolume(AudioChannel.Dialogue);
        ApplyMusicSourceVolume();
    }

    private void ApplyVolume(AudioChannel channel)
    {
        if (audioMixer != null)
        {
            ApplyMixerVolume(GetMixerParameter(channel), GetVolume(channel), IsMuted(channel));
            ApplyUnroutedSourceVolumes();
            ApplyMusicSourceVolume();
            return;
        }

        ApplyMusicSourceVolume();
        ApplySourceVolume(sfxSource, AudioChannel.Sfx);
        ApplySourceVolume(dialogueSource, AudioChannel.Dialogue);
    }

    private void ApplyUnroutedSourceVolumes()
    {
        ApplyUnroutedSourceVolume(sfxSource, AudioChannel.Sfx);
        ApplyUnroutedSourceVolume(dialogueSource, AudioChannel.Dialogue);
    }

    private void ApplyUnroutedSourceVolume(AudioSource source, AudioChannel channel)
    {
        if (source != null && source.outputAudioMixerGroup == null)
            source.volume = GetEffectiveLinearVolume(channel);
    }

    private void ApplySourceVolume(AudioSource source, AudioChannel channel)
    {
        if (source == null)
            return;

        source.volume = GetEffectiveLinearVolume(channel);
    }

    private void ApplyMusicSourceVolume()
    {
        if (musicSource == null)
            return;

        musicSource.volume = musicSource.outputAudioMixerGroup != null
            ? currentMusicVolumeScale
            : currentMusicVolumeScale * GetEffectiveLinearVolume(AudioChannel.Music);
    }

    private void ApplyMixerVolume(string exposedParameter, float linearVolume, bool muted)
    {
        if (string.IsNullOrWhiteSpace(exposedParameter))
            return;

        float decibels = muted || linearVolume <= MinLinearVolume
            ? MutedDecibels
            : Mathf.Log10(linearVolume) * 20f;

        audioMixer.SetFloat(exposedParameter, decibels);
    }

    private string GetMixerParameter(AudioChannel channel)
    {
        switch (channel)
        {
            case AudioChannel.Master:
                return masterVolumeParam;
            case AudioChannel.Music:
                return musicVolumeParam;
            case AudioChannel.Dialogue:
                return dialogueVolumeParam;
            default:
                return sfxVolumeParam;
        }
    }

    private string GetVolumeKey(AudioChannel channel)
    {
        switch (channel)
        {
            case AudioChannel.Master:
                return MasterVolumeKey;
            case AudioChannel.Music:
                return MusicVolumeKey;
            case AudioChannel.Dialogue:
                return DialogueVolumeKey;
            default:
                return SfxVolumeKey;
        }
    }

    private void SaveMutedState(AudioChannel channel, bool muted)
    {
        switch (channel)
        {
            case AudioChannel.Master:
                PlayerPrefs.SetInt(MasterMutedKey, muted ? 1 : 0);
                break;
            case AudioChannel.Music:
                PlayerPrefs.SetInt(MusicEnabledKey, muted ? 0 : 1);
                break;
            case AudioChannel.Dialogue:
                PlayerPrefs.SetInt(DialogueMutedKey, muted ? 1 : 0);
                break;
            default:
                PlayerPrefs.SetInt(SfxMutedKey, muted ? 1 : 0);
                break;
        }
    }

    private float LoadFloat(string key, string legacyKey, float defaultValue)
    {
        float value = PlayerPrefs.HasKey(key)
            ? PlayerPrefs.GetFloat(key)
            : PlayerPrefs.GetFloat(legacyKey, defaultValue);

        return Mathf.Clamp01(value);
    }

    private bool LoadEnabledToggle(string key, string legacyKey)
    {
        return PlayerPrefs.HasKey(key)
            ? PlayerPrefs.GetInt(key, 1) == 1
            : PlayerPrefs.GetInt(legacyKey, 1) == 1;
    }
}
