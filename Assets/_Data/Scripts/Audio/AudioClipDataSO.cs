using UnityEngine;

[CreateAssetMenu(fileName = "AudioClipData", menuName = "ScriptableObjects/Audio Clip Data", order = 1)]
public class AudioClipDataSO : ScriptableObject
{
    [SerializeField] private AudioClipName clipName = AudioClipName.None;
    [SerializeField] private AudioClip clip;

    public AudioClipName ClipName => clipName;
    public AudioClip Clip => clip;
}
