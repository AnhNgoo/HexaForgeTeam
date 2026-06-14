using System.IO;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance;

    private CharacterUnlockData data =
        new CharacterUnlockData();


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);

            return;
        }


        LoadData();

        CheckUnlockCharacter();
    }

    public CharacterType GetSelectedCharacter()
    {
        return data.selectedCharacter;
    }

    public void SelectCharacter(
        CharacterType type)
    {
        if (!IsUnlocked(type))
        {
            return;
        }

        data.selectedCharacter = type;

        SaveData();
    }

    public bool IsUnlocked(
        CharacterType type)
    {
        switch (type)
        {
            case CharacterType.Kael:
                return data.kaelUnlocked;

            case CharacterType.Lyra:
                return data.lyraUnlocked;

            case CharacterType.Ares:
                return data.aresUnlocked;

            case CharacterType.Elara:
                return data.elaraUnlocked;
        }

        return false;
    }

    public void CheckUnlockCharacter()
    {
        int level =
            AccountLevelManager.Instance
            .GetLevel();

        if (level >= 5)
        {
            data.lyraUnlocked = true;
        }

        if (level >= 15)
        {
            data.aresUnlocked = true;
        }

        if (level >= 25)
        {
            data.elaraUnlocked = true;
        }

        SaveData();
    }

    private void SaveData()
{
    SaveLoadManager.Instance
        .SaveData.characterData =
        data;

    SaveLoadManager.Instance
        .SaveGame();
}

    private void LoadData()
{
    data =
        SaveLoadManager.Instance
        .SaveData.characterData;

    if (data == null)
    {
        data =
            new CharacterUnlockData();
    }
}
}