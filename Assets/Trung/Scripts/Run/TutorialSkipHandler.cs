using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialSkipHandler : MonoBehaviour
{
    public void SkipOrCompleteTutorial()
    {
        if (SaveLoadManager.Instance != null && SaveLoadManager.Instance.SaveData != null)
        {
            // 1. Bật cờ xác nhận đã hoàn thành Tutorial xịn cục bộ
            SaveLoadManager.Instance.SaveData.isTutorialCompleted = true; // cite: 42
            SaveLoadManager.Instance.SaveGame(); // Lưu vật lý xuống ổ cứng máy

            // Ép PlayFabDataManager thực hiện đẩy dữ liệu lên Cloud ngay lập tức để khóa tiến trình
            if (PlayFabDataManager.Instance != null) // cite: 42
            {
                Debug.Log("[Tutorial] Ép lưu tiến trình hoàn thành Tutorial lên đám mây PlayFab lập tức..."); // cite: 42
                PlayFabDataManager.Instance.SaveCloud(); // cite: 42
            }
        }

        // 2. Thay vì chuyển cảnh trực tiếp, ta gọi Coroutine điều phối luồng nạp qua Loading Scene
        StartCoroutine(TransitionWithLoadingRoutine());
    }

    private IEnumerator TransitionWithLoadingRoutine()
{
    Debug.Log("[Tutorial] Đang nạp Additive màn hình Loading...");

    // Bước A: Nạp "Loading Scene" song song (Additive) làm màn che
    AsyncOperation loadLoading = SceneManager.LoadSceneAsync("Loading Scene", LoadSceneMode.Additive);
    while (!loadLoading.isDone)
    {
        yield return null;
    }

    yield return new WaitForSeconds(0.1f);

    // Gán thông tin hiển thị địa điểm đích
    if (LoadingUIManager.Instance != null)
    {
        LoadingUIManager.Instance.SetDestinationName("LobbyMain Scene");
    }

    Debug.Log("[Tutorial] Đang nạp LobbyMain Scene ngầm...");

    // Bước B: Nạp Single "LobbyMain Scene" ngầm bên dưới
    AsyncOperation loadLobby = SceneManager.LoadSceneAsync("LobbyMain Scene", LoadSceneMode.Single);
    loadLobby.allowSceneActivation = false;

    // Theo dõi tiến độ Slider
    if (LoadingUIManager.Instance != null)
    {
        yield return StartCoroutine(LoadingUIManager.Instance.TrackProgressRoutine(loadLobby));
    }

    loadLobby.allowSceneActivation = true;
    while (!loadLobby.isDone)
    {
        yield return null;
    }

    Debug.Log("[Tutorial] Đã nạp thành công LobbyMain Scene. Tiến hành dọn dẹp Loading Scene...");

    // Bước C: Giải phóng Loading Scene
    Scene loadingScene = SceneManager.GetSceneByName("Loading Scene");
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