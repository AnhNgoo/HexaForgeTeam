using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(AudioSource))]
public class AudioSourcePlayer : LoadComponents
{
    [SerializeField] private AudioSource audioSource;
    [FormerlySerializedAs("autoPlayOnStart")]
    [SerializeField] private bool autoPlayOnEnable = false;

    protected override void LoadComponent()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    protected override void LoadComponentRuntime()
    {

    }

    private void OnEnable()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (autoPlayOnEnable)
            Play();
    }

    public void Play()
    {
        if (audioSource == null)
            return;

        audioSource.Play();
    }

    public void PlayOneShot()
    {
        if (audioSource == null || audioSource.clip == null)
            return;

        audioSource.PlayOneShot(audioSource.clip);
    }

    public void PlayOneShot(AudioClip clip)
    {
        if (audioSource == null || clip == null)
            return;

        audioSource.PlayOneShot(clip);
    }

    public void PlayWithClip(AudioClip clip)
    {
        if (audioSource == null || clip == null)
            return;

        audioSource.clip = clip;
        audioSource.Play();
    }

    public void PlayWithClip()
    {
        Play();
    }

    public void Stop()
    {
        if (audioSource == null)
            return;

        audioSource.Stop();
    }

    public void Pause()
    {
        if (audioSource == null)
            return;

        audioSource.Pause();
    }

    public void UnPause()
    {
        if (audioSource == null)
            return;

        audioSource.UnPause();
    }
}
