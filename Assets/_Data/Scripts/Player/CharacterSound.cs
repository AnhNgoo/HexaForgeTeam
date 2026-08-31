using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSound : MonoBehaviour
{
    public AudioSource Sound;
    public AudioClip jumpSoundEffect;
    public AudioClip deathSoundEffect;
    public AudioClip dodgeSoundEffect;
    public AudioClip hurtSoundEffect;

    public void Play(AudioClip clip)
    {
        if (Sound != null && clip != null)
        {
            Sound.PlayOneShot(clip);
        }
    }
}
