using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LogoutMenu : MonoBehaviour
{
    [Header("Parent")]
    [SerializeField] private SystemSettingsPanel systemSettingsPanel;

    [Header("Confirmation")]
    [SerializeField] private GameObject confirmationRoot;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Button btnLogout;
    [SerializeField] private Button btnCancel;

    [Header("Content")]
    [SerializeField, TextArea]
    private string confirmationMessage = "Are you sure you want to log out?";

    [SerializeField]
    private SystemSettingPage cancelReturnPage = SystemSettingPage.Audio;

    private bool eventsAdded;

    private void OnEnable()
    {
        AddEvents();
        ShowConfirmation();
    }

    private void OnDisable()
    {
        RemoveEvents();
    }

    private void AddEvents()
    {
        if (eventsAdded)
            return;

        if (btnLogout != null)
            btnLogout.onClick.AddListener(Logout);

        if (btnCancel != null)
            btnCancel.onClick.AddListener(Cancel);

        eventsAdded = true;
    }

    private void RemoveEvents()
    {
        if (!eventsAdded)
            return;

        if (btnLogout != null)
            btnLogout.onClick.RemoveListener(Logout);

        if (btnCancel != null)
            btnCancel.onClick.RemoveListener(Cancel);

        eventsAdded = false;
    }

    public void ShowConfirmation()
    {
        if (descriptionText != null)
            descriptionText.text = confirmationMessage;

        if (confirmationRoot != null)
            confirmationRoot.SetActive(true);
    }

    public void Cancel()
    {
        if (confirmationRoot != null)
            confirmationRoot.SetActive(false);

        if (systemSettingsPanel != null)
            systemSettingsPanel.ShowPage(cancelReturnPage);
    }

    public void Logout()
    {
        if (PlayFabDataManager.Instance != null)
        {
            PlayFabDataManager.Instance.SaveCloud();
        }

        // Xóa cờ phiên làm việc tự động đăng nhập
        PlayerPrefs.DeleteKey("IsAutoLoginActive");
        PlayerPrefs.DeleteKey("LastAccountUser");
        PlayerPrefs.DeleteKey("LastAccountPass");

        if (PlayerPrefs.HasKey("PlayFabID"))
        {
            PlayerPrefs.DeleteKey("PlayFabID");
        }

        PlayerPrefs.Save();

        Time.timeScale = 1f;

        // Bắt đầu luồng nạp Loading Scene Additive mượt mà chuyển về Login Scene
        StartCoroutine(LogoutTransitionRoutine());
    }

    private System.Collections.IEnumerator LogoutTransitionRoutine()
    {
        string loginSceneName = GameSceneData.Instance != null ? GameSceneData.Instance.loginScene : "Login Scene";
        string loadingSceneName = GameSceneData.Instance != null ? GameSceneData.Instance.loadingScene : "Loading Scene";

        // 1. Nạp Loading Scene dạng Additive
        AsyncOperation loadLoading = SceneManager.LoadSceneAsync(loadingSceneName, LoadSceneMode.Additive);
        while (!loadLoading.isDone) yield return null;

        yield return new WaitForSecondsRealtime(0.1f);

        // 2. Cập nhật tên điểm đến trên LoadingUIManager
        if (LoadingUIManager.Instance != null)
        {
            LoadingUIManager.Instance.SetDestinationName(loginSceneName);
        }

        // 3. Nạp Login Scene ngầm
        AsyncOperation loadLogin = SceneManager.LoadSceneAsync(loginSceneName, LoadSceneMode.Single);
        loadLogin.allowSceneActivation = false;

        if (LoadingUIManager.Instance != null)
        {
            yield return StartCoroutine(LoadingUIManager.Instance.TrackProgressRoutine(loadLogin));
        }

        // 4. Kích hoạt chuyển sang Login Scene
        loadLogin.allowSceneActivation = true;
        while (!loadLogin.isDone) yield return null;

        // 5. Giải phóng Loading Scene
        Scene loadingScene = SceneManager.GetSceneByName(loadingSceneName);
        if (loadingScene.isLoaded)
        {
            AsyncOperation unloadLoading = SceneManager.UnloadSceneAsync(loadingScene);
            while (!unloadLoading.isDone) yield return null;
        }
    }
}