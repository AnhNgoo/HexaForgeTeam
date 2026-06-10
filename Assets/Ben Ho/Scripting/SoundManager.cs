using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Sources")]
    public AudioSource bgMusicSource;
    public AudioSource sfxSource;
    public AudioSource collisionSource;
    public AudioSource dialogueSource;

    [Header("Audio Mixer")]
    public AudioMixer audioMixer;

    [Header("Mixer Parameters")]
    public string masterVolumeParam = "MasterVolume";
    public string musicVolumeParam = "MusicVolume";
    public string sfxVolumeParam = "SoundEffectsVolume";
    public string dialogueVolumeParam = "DialogueVolume";
    public string collisionVolumeParam = "CollisionVolume";

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        LoadAudioSettings();

        if (bgMusicSource != null && bgMusicSource.clip != null && !bgMusicSource.isPlaying)
        {
            bgMusicSource.loop = true;
            bgMusicSource.Play();
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void PlayCollision(AudioClip clip)
    {
        if (collisionSource != null && clip != null)
        {
            collisionSource.PlayOneShot(clip);
        }
    }

    public void PlayDialogue(AudioClip clip)
    {
        if (dialogueSource != null && clip != null)
        {
            dialogueSource.PlayOneShot(clip);
        }
    }

    public void LoadAudioSettings()
    {
        SetVolume(masterVolumeParam, PlayerPrefs.GetFloat("MasterVolume", 1f));
        SetVolume(musicVolumeParam, PlayerPrefs.GetFloat("MusicVolume", 1f));
        SetVolume(sfxVolumeParam, PlayerPrefs.GetFloat("SoundEffectsVolume", 1f));
        SetVolume(dialogueVolumeParam, PlayerPrefs.GetFloat("DialogueVolume", 1f));

        bool backgroundOn = PlayerPrefs.GetInt("BackgroundSoundOn", 1) == 1;
        bool collisionOn = PlayerPrefs.GetInt("CollisionSoundOn", 1) == 1;

        if (!backgroundOn)
            audioMixer.SetFloat(musicVolumeParam, -80f);

        if (!collisionOn)
            audioMixer.SetFloat(collisionVolumeParam, -80f);
        else
            SetVolume(collisionVolumeParam, 1f);
    }

    public void SetVolume(string parameterName, float value)
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
}