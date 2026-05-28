using System.IO;
using UnityEngine;

public class SaveLoadManager :
    MonoBehaviour
{
    public static SaveLoadManager Instance;

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
            Application.persistentDataPath
            + "/RuneInventory.json";
    }

    private void Start()
    {
        LoadInventory();
    }

    public void SaveInventory()
    {
        if (RuneInventory.Instance == null)
        {
            return;
        }

        RuneInventorySaveData saveData =
            new RuneInventorySaveData();

        saveData.runes =
            RuneInventory.Instance.runes;

        string json =
            JsonUtility.ToJson(
                saveData,
                true);

        File.WriteAllText(
            savePath,
            json);

        Debug.Log(
            "Saved Rune Inventory");
    }

    public void LoadInventory()
    {
        if (!File.Exists(savePath))
        {
            return;
        }

        string json =
            File.ReadAllText(savePath);

        RuneInventorySaveData saveData =
            JsonUtility.FromJson
            <RuneInventorySaveData>(json);

        if (saveData == null)
        {
            return;
        }

        if (RuneInventory.Instance == null)
        {
            return;
        }

        RuneInventory.Instance.runes =
            saveData.runes;

        Debug.Log(
            "Loaded Rune Inventory");
    }
}