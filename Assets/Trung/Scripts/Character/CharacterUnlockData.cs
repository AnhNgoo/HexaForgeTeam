using System;

[System.Serializable]
public class CharacterUnlockData
{
    public bool kaelUnlocked = true;

    public bool lyraUnlocked = false;

    public bool aresUnlocked = false;

    public bool elaraUnlocked = false;

    public CharacterType selectedCharacter =
        CharacterType.Kael;
}