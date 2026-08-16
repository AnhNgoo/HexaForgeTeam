using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialSkipHandler : MonoBehaviour
{

    private void Start()
    {
        if (PlayerManager.Instance == null)
        {
            Debug.LogError("[Tutorial] Không tìm thấy PlayerManager.");
            return;
        }

        PlayerManager.Instance.SpawnCharacterInLobby();
        EventManager.Notify(GameEvent.OnLoadingComplete);
    }

    public void SkipOrCompleteTutorial()
    {
        if (SaveLoadManager.Instance != null && SaveLoadManager.Instance.SaveData != null)
        {
            SaveLoadManager.Instance.SaveData.isTutorialCompleted = true; // cite: 39
            SaveLoadManager.Instance.SaveGame(); // cite: 39

            if (PlayFabDataManager.Instance != null)
            {
                Debug.Log("[Tutorial] Ép lưu tiến trình hoàn thành Tutorial lên đám mây PlayFab lập tức..."); // cite: 39
                PlayFabDataManager.Instance.SaveCloud(); // cite: 39
            }
        }

        StartCoroutine(TransitionWithLoadingRoutine());
    }

    private IEnumerator TransitionWithLoadingRoutine()
    {
        string targetLobbyScene = GameSceneData.Instance != null 
            ? GameSceneData.Instance.GetSceneName(SceneType.LobbyMain) 
            : "LobbyMain Scene";
            
        string loadingSceneName = GameSceneData.Instance != null 
            ? GameSceneData.Instance.GetSceneName(SceneType.Loading) 
            : "Loading Scene";

        Debug.Log($"[Tutorial] Đang nạp Additive {loadingSceneName}...");

        AsyncOperation loadLoading = SceneManager.LoadSceneAsync(loadingSceneName, LoadSceneMode.Additive);
        while (!loadLoading.isDone)
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.1f);

        if (LoadingUIManager.Instance != null)
        {
            LoadingUIManager.Instance.SetDestinationName(targetLobbyScene);
        }

        Debug.Log($"[Tutorial] Đang nạp {targetLobbyScene} ngầm...");

        AsyncOperation loadLobby = SceneManager.LoadSceneAsync(targetLobbyScene, LoadSceneMode.Single);
        loadLobby.allowSceneActivation = false;

        if (LoadingUIManager.Instance != null)
        {
            yield return StartCoroutine(LoadingUIManager.Instance.TrackProgressRoutine(loadLobby));
        }

        loadLobby.allowSceneActivation = true;
        while (!loadLobby.isDone)
        {
            yield return null;
        }

        Debug.Log($"[Tutorial] Đã nạp thành công {targetLobbyScene}. Tiến hành dọn dẹp {loadingSceneName}...");

        Scene loadingScene = SceneManager.GetSceneByName(loadingSceneName);
        if (loadingScene.isLoaded)
        {
            AsyncOperation unloadLoading = SceneManager.UnloadSceneAsync(loadingScene);
            while (!unloadLoading.isDone)
            {
                yield return null;
            }
            Debug.Log("[Tutorial] Đã giải phóng Loading Scene thành công.");
        }
    }
}