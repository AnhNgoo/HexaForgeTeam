using UnityEngine;
using UnityEngine.SceneManagement;
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

        DontDestroyOnLoad(
            gameObject);
    }
    else
    {
        Destroy(gameObject);
    }
}
    private void Start()
{
    Debug.Log(
        "Loading Cloud...");

    LoadCloud();
}

    #region SAVE CLOUD

    public void SaveCloud()
    {
        if (SaveLoadManager.Instance == null)
        {
            Debug.LogError("SaveLoadManager not found");
            return;
        }

        string json =
            JsonUtility.ToJson(
                SaveLoadManager.Instance.SaveData);

        var request =
            new UpdateUserDataRequest
            {
                Data =
                    new Dictionary<string, string>
                    {
                        { "PlayerData", json }
                    }
            };

        PlayFabClientAPI.UpdateUserData(
            request,
            OnSaveSuccess,
            OnPlayFabError);
    }

    private void OnSaveSuccess(UpdateUserDataResult result)
    {
        Debug.Log("Cloud Save Success");
    }

    #endregion

    #region LOAD CLOUD

    public void LoadCloud()
    {
        PlayFabClientAPI.GetUserData(
            new GetUserDataRequest(),
            OnLoadSuccess,
            OnPlayFabError);
    }

    private void OnLoadSuccess(GetUserDataResult result)
    {
        Debug.Log("Cloud Load Success");

        if (result.Data == null)
        {
            Debug.Log("No cloud data found");

            return;
        }

        if (!result.Data.ContainsKey(
    "PlayerData"))
{
    Debug.Log(
        "New Account");

    SaveLoadManager.Instance
        .SaveData =
        new GameSaveData();

    SaveLoadManager.Instance
        .SaveGame();

    SceneManager.LoadScene(
        "LobbyRiu 1");

    return;
}

        string json =
            result.Data["PlayerData"].Value;

        if (string.IsNullOrEmpty(json))
        {
            Debug.Log("PlayerData empty");

            return;
        }

        GameSaveData cloudData =
            JsonUtility.FromJson<GameSaveData>(
                json);

        if (cloudData == null)
        {
            Debug.LogWarning(
                "Failed to parse cloud data");

            return;
        }

        SaveLoadManager.Instance.SaveData =
            cloudData;

        SaveLoadManager.Instance.SaveGame();

        Debug.Log("Cloud Data Applied");
        SceneManager.LoadScene(
    "LobbyRiu 1");

    }

    #endregion

    private void OnPlayFabError(PlayFabError error)
    {
        Debug.LogError(
            error.GenerateErrorReport());
    }
    public void MarkDirty()
{
    needSaveCloud = true;
}
private void Update()
{
    if (!needSaveCloud)
    {
        return;
    }

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