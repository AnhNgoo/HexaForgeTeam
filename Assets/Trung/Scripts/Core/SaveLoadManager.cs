using System.IO;
using UnityEngine;

public class SaveLoadManager : MonoBehaviour
{
    public static SaveLoadManager Instance;

    public GameSaveData SaveData;

    private string savePath;

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

        savePath =
            Application.persistentDataPath +
            "/GameSave.json";

        LoadGame();
    }

    public void SaveGame()
    {
        string json =
            JsonUtility.ToJson(
                SaveData,
                true);

        File.WriteAllText(
            savePath,
            json);
    }

    public void LoadGame()
    {
        if (!File.Exists(savePath))
        {
            SaveData =
                new GameSaveData();

            SaveGame();

            return;
        }

        string json =
            File.ReadAllText(
                savePath);

        SaveData =
            JsonUtility.FromJson<GameSaveData>(
                json);

        if (SaveData == null)
        {
            SaveData =
                new GameSaveData();
        }
    }
    public void DeleteSave()
{
    if (File.Exists(savePath))
    {
        File.Delete(savePath);
    }

    SaveData = new GameSaveData();
}
}