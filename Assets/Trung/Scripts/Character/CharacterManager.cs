using System.IO;
using UnityEngine;
using System.Collections.Generic;

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

        Invoke(
        nameof(CheckUnlockCharacter),
        0.1f);
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

        // MỚI: Ép RuneEquipUI đồng bộ theo đúng nhân vật đang được chọn để tránh lỗi hiển thị lệch pha chỉ số
        if (RuneEquipUI.Instance != null)
        {
            // Dùng Reflection hoặc gọi gián tiếp để gán biến private viewingCharacter nếu cần
            // Ở đây ta ép trực tiếp bằng cách gán thông qua hàm Refresh hoặc phương thức thích hợp
            var field = typeof(RuneEquipUI).GetField("viewingCharacter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(RuneEquipUI.Instance, type);
            }
        }

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
        CharacterSelectUI ui =
    FindFirstObjectByType<CharacterSelectUI>();

if (ui != null)
{
    ui.RefreshUI();
}
    }

    private void SaveData()
{
    SaveLoadManager.Instance
        .SaveData.characterData =
        data;

    SaveLoadManager.Instance.SaveGame();
if (PlayFabDataManager.Instance != null)
{
    PlayFabDataManager.Instance.MarkDirty(); 
}
if (PlayFabDataManager.Instance != null)
{
    PlayFabDataManager.Instance
        .MarkDirty();
}
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
    
    if (!IsUnlocked(
    data.selectedCharacter))
{
    data.selectedCharacter =
        CharacterType.Kael;
}
}
public void ResetCharacterData()
{
    data =
        new CharacterUnlockData();

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