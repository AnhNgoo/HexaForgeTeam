using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;

public class PlayFabDataManager : MonoBehaviour
{
    public static PlayFabDataManager Instance;
    private bool needSaveCloud = false;
    private float saveTimer = 0f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #region SAVE CLOUD
    public void SaveCloud()
    {
        if (SaveLoadManager.Instance == null || SaveLoadManager.Instance.SaveData == null) return;

        string json = JsonUtility.ToJson(SaveLoadManager.Instance.SaveData);

        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string> { { "PlayerData", json } }
        };

        PlayFabClientAPI.UpdateUserData(request, OnSaveSuccess, OnPlayFabError);
    }

    private void OnSaveSuccess(UpdateUserDataResult result)
    {
        Debug.Log("[PlayFabDataManager] Cloud Save Success!");
    }
    #endregion

    #region LOAD CLOUD
    public void LoadCloud(System.Action<bool> onComplete = null)
    {
        LoadTrace.Mark("Sending PlayFab GetUserData");
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), result =>
        {
            Debug.Log("[PlayFabDataManager] Cloud Load Success!");
            LoadTrace.Mark("GetUserData response received");

            if (SaveLoadManager.Instance == null)
            {
                SaveLoadManager.Instance = FindFirstObjectByType<SaveLoadManager>();
            }

            if (SaveLoadManager.Instance == null)
            {
                Debug.LogError("[PlayFabDataManager] KHÔNG THỂ tìm thấy SaveLoadManager!");
                onComplete?.Invoke(false);
                return;
            }

            if (result.Data == null || !result.Data.ContainsKey("PlayerData") || string.IsNullOrEmpty(result.Data["PlayerData"].Value))
            {
                Debug.Log("[CLOUD] Dữ liệu mới. Khởi tạo dữ liệu mặc định...");
                SaveLoadManager.Instance.SaveData = new GameSaveData();
                SaveLoadManager.Instance.SaveData.isTutorialCompleted = false;
                LoadTrace.Mark("Writing local save");
                SaveLoadManager.Instance.SaveGame();
                LoadTrace.Mark("Local save completed");
            }
            else
            {
                string json = result.Data["PlayerData"].Value;
                LoadTrace.Mark("Parsing cloud JSON");
                GameSaveData cloudData = JsonUtility.FromJson<GameSaveData>(json);
                LoadTrace.Mark("Cloud JSON parsed");
                if (cloudData != null)
                {
                    SaveLoadManager.Instance.SaveData = cloudData;
                    LoadTrace.Mark("Writing local save");
                    SaveLoadManager.Instance.SaveGame();
                    LoadTrace.Mark("Local save completed");
                }
            }

            onComplete?.Invoke(true);
        }, error =>
        {
            OnPlayFabError(error);
            onComplete?.Invoke(false);
        });
    }
    #endregion

    private void OnPlayFabError(PlayFabError error)
    {
        Debug.LogError(error.GenerateErrorReport());
    }

    public void MarkDirty()
    {
        needSaveCloud = true;
    }

    private void Update()
    {
        if (!needSaveCloud) return;

        saveTimer += Time.deltaTime;
        if (saveTimer >= 5f)
        {
            saveTimer = 0f;
            needSaveCloud = false;
            SaveCloud();
        }
    }

    private void OnApplicationQuit()
    {
        if (needSaveCloud)
        {
            SaveCloud();
        }
    }
}