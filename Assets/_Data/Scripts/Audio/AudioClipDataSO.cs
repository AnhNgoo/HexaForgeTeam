using UnityEngine;

[CreateAssetMenu(fileName = "AudioClipData", menuName = "ScriptableObjects/Audio Clip Data", order = 1)]
public class AudioClipDataSO : ScriptableObject
{
    [SerializeField] private AudioClipName clipName = AudioClipName.None;
    [SerializeField] private AudioClip clip;
    [SerializeField] private AudioChannel channel = AudioChannel.Sfx;
    [SerializeField, Range(0f, 1f)] private float volume = 1f;
    [SerializeField] private bool loop;

    public AudioClipName ClipName => clipName;
    public AudioClip Clip => clip;
    public AudioChannel Channel => channel;
    public float Volume => volume;
    public bool Loop => loop;
}
