using System.IO;
using UnityEngine;
using System.Collections.Generic;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance;

    private CharacterUnlockData data = new CharacterUnlockData();

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
        Invoke(nameof(CheckUnlockCharacter), 0.1f);
    }

    private void Start()
    {
        if (InteractManagerV2.Instance != null)
        {
            InteractManagerV2.Instance.ForceUnlockState();
        }
    }

    public CharacterType GetSelectedCharacter()
    {
        return data.selectedCharacter;
    }

    public void SelectCharacter(CharacterType type)
    {
        if (!IsUnlocked(type))
        {
            return;
        }

        data.selectedCharacter = type;

        // Đồng bộ dữ liệu lưu tên nhân vật cho PlayerPrefs
        PlayerPrefs.SetString("SelectedCharacter", type.ToString());
        PlayerPrefs.Save();

        if (RuneEquipUI.Instance != null)
        {
            var field = typeof(RuneEquipUI).GetField("viewingCharacter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(RuneEquipUI.Instance, type);
            }
        }

        SaveData();
    }

    public bool IsUnlocked(CharacterType type)
    {
        switch (type)
        {
            case CharacterType.Kael: return data.kaelUnlocked;
            case CharacterType.Lyra: return data.lyraUnlocked;
            case CharacterType.Ares: return data.aresUnlocked;
            case CharacterType.Elara: return data.elaraUnlocked;
        }
        return false;
    }

    public void CheckUnlockCharacter()
    {
        int level = 1;
        if (AccountLevelManager.Instance != null)
        {
            level = AccountLevelManager.Instance.GetLevel();
        }

        data.kaelUnlocked = true;
        data.lyraUnlocked = (level >= 5);
        data.aresUnlocked = (level >= 15);
        data.elaraUnlocked = (level >= 25);

        if (!IsUnlocked(data.selectedCharacter))
        {
            data.selectedCharacter = CharacterType.Kael;
            PlayerPrefs.SetString("SelectedCharacter", CharacterType.Kael.ToString());
            PlayerPrefs.Save();
        }

        SaveData();

        CharacterSelectUI ui = FindFirstObjectByType<CharacterSelectUI>();
        if (ui != null)
        {
            ui.RefreshUI();
        }
    }

    private void SaveData()
    {
        if (SaveLoadManager.Instance != null && SaveLoadManager.Instance.SaveData != null)
        {
            SaveLoadManager.Instance.SaveData.characterData = data;
            SaveLoadManager.Instance.SaveGame();
        }

        if (PlayFabDataManager.Instance != null)
        {
            PlayFabDataManager.Instance.MarkDirty();
        }
    }

    private void LoadData()
    {
        if (SaveLoadManager.Instance != null && SaveLoadManager.Instance.SaveData != null)
        {
            data = SaveLoadManager.Instance.SaveData.characterData;
        }

        if (data == null)
        {
            data = new CharacterUnlockData();
        }

        if (data.characterRuneBuilds == null || data.characterRuneBuilds.Count == 0)
        {
            data.characterRuneBuilds = new List<CharacterRuneEquip>
            {
                new CharacterRuneEquip(CharacterType.Kael),
                new CharacterRuneEquip(CharacterType.Lyra),
                new CharacterRuneEquip(CharacterType.Ares),
                new CharacterRuneEquip(CharacterType.Elara)
            };
        }

        CheckUnlockCharacter();
    }

    public void ResetCharacterData()
    {
        data = new CharacterUnlockData();
        data.kaelUnlocked = true;
        data.lyraUnlocked = false;
        data.aresUnlocked = false;
        data.elaraUnlocked = false;
        data.selectedCharacter = CharacterType.Kael;

        PlayerPrefs.SetString("SelectedCharacter", CharacterType.Kael.ToString());
        PlayerPrefs.Save();

        SaveData();
    }

    public CharacterRuneEquip GetCharacterRuneBuild(CharacterType type)
    {
        if (data == null || data.characterRuneBuilds == null) return null;

        for (int i = 0; i < data.characterRuneBuilds.Count; i++)
        {
            if (data.characterRuneBuilds[i].characterType == type)
            {
                return data.characterRuneBuilds[i];
            }
        }
        return null;
    }
}