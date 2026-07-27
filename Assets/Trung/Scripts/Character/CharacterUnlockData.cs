using System;
using System.Collections.Generic;

[System.Serializable]
public class CharacterRuneEquip
{
    public CharacterType characterType;
    public string[] equippedRuneIDs = new string[3] { "", "", "" };

    public CharacterRuneEquip(CharacterType type)
    {
        characterType = type;
        equippedRuneIDs = new string[3] { "", "", "" };
    }
}

[System.Serializable]
public class CharacterUnlockData
{
    public bool kaelUnlocked = true;

    public bool lyraUnlocked = false;

    public bool aresUnlocked = false;

    public bool elaraUnlocked = false;

    public CharacterType selectedCharacter = CharacterType.Kael;

    public List<CharacterRuneEquip> characterRuneBuilds = new List<CharacterRuneEquip>();
}