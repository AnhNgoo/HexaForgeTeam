using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class LogoutMenu : MonoBehaviour
{
    [Header("Parent")]
    [SerializeField] private SystemSettingsPanel systemSettingsPanel;

    [Header("Confirmation")]
    [SerializeField] private GameObject confirmationRoot;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Button btnLogout;
    [SerializeField] private TMP_Text btnLogoutText;
    [SerializeField] private Button btnCancel;

    [Header("Content Messages")]
    [SerializeField, TextArea]
    private string logoutConfirmationMessage = "Are you sure you want to log out?";

    [SerializeField, TextArea]
    private string returnLobbyConfirmationMessage = "Are you sure you want to abandon this battle and return to the Lobby?";

    [SerializeField]
    private SystemSettingPage cancelReturnPage = SystemSettingPage.Audio;

    private bool eventsAdded;
    private bool isInLobby = true;

    private void OnEnable()
    {
        CheckContextState();
        AddEvents();
        ShowConfirmation();
    }

    private void OnDisable()
    {
        RemoveEvents();
    }

    private void CheckContextState()
    {
        if (GameManager.Instance != null)
        {
            isInLobby = (GameManager.Instance.MapType == MapType.Lobby);
        }
        else
        {
            string activeScene = SceneManager.GetActiveScene().name;
            isInLobby = activeScene.IndexOf("Lobby", System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        if (btnLogoutText != null)
        {
            btnLogoutText.text = isInLobby ? "Confirm" : "Return Lobby";
        }
    }

    private void AddEvents()
    {
        if (eventsAdded)
            return;

        if (btnLogout != null)
            btnLogout.onClick.AddListener(OnConfirmAction);

        if (btnCancel != null)
            btnCancel.onClick.AddListener(Cancel);

        eventsAdded = true;
    }

    private void RemoveEvents()
    {
        if (!eventsAdded)
            return;

        if (btnLogout != null)
            btnLogout.onClick.RemoveListener(OnConfirmAction);

        if (btnCancel != null)
            btnCancel.onClick.RemoveListener(Cancel);

        eventsAdded = false;
    }

    public void ShowConfirmation()
    {
        CheckContextState();

        if (descriptionText != null)
        {
            descriptionText.text = isInLobby ? logoutConfirmationMessage : returnLobbyConfirmationMessage;
        }

        if (confirmationRoot != null)
            confirmationRoot.SetActive(true);
    }

    public void Cancel()
    {
        if (systemSettingsPanel == null)
            return;

        systemSettingsPanel.ShowPage(cancelReturnPage);
    }

    public void OnConfirmAction()
    {
        if (isInLobby)
        {
            ExecuteLogout();
        }
        else
        {
            ExecuteReturnToLobby();
        }
    }

    private void ExecuteReturnToLobby()
    {
        Time.timeScale = 1f;

        if (systemSettingsPanel != null)
        {
            systemSettingsPanel.Close();
        }

        if (RunManager.Instance != null)
        {
            RunManager.Instance.ReturnToLobby();
        }
    }

    private void ExecuteLogout()
    {
        if (PlayFabDataManager.Instance != null)
        {
            PlayFabDataManager.Instance.SaveCloud();
        }

        PlayerPrefs.DeleteKey("IsAutoLoginActive");
        PlayerPrefs.DeleteKey("LastAccountUser");
        PlayerPrefs.DeleteKey("LastAccountPass");

        if (PlayerPrefs.HasKey("PlayFabID"))
        {
            PlayerPrefs.DeleteKey("PlayFabID");
        }

        PlayerPrefs.Save();

        Time.timeScale = 1f;

        StartCoroutine(LogoutTransitionRoutine());
    }

    private IEnumerator LogoutTransitionRoutine()
    {
        GameSceneData sceneData = GameSceneData.Instance;

        string loginSceneName = sceneData != null
            ? sceneData.GetSceneName(SceneType.Login)
            : "Login Scene";

        string loadingSceneName = sceneData != null
            ? sceneData.GetSceneName(SceneType.Loading)
            : "Loading Scene";

        AsyncOperation loadLoading = SceneManager.LoadSceneAsync(loadingSceneName, LoadSceneMode.Additive);
        while (!loadLoading.isDone) yield return null;

        yield return new WaitForSecondsRealtime(0.05f);

        if (LoadingUIManager.Instance != null)
        {
            LoadingUIManager.Instance.SetDestinationName(loginSceneName);
        }

        AsyncOperation loadLogin = SceneManager.LoadSceneAsync(loginSceneName, LoadSceneMode.Single);
        loadLogin.allowSceneActivation = false;

        float duration = Random.Range(5.0f, 7.0f);
        if (LoadingUIManager.Instance != null)
        {
            yield return StartCoroutine(LoadingUIManager.Instance.TrackProgressRoutine(loadLogin, false, duration));
        }

        loadLogin.allowSceneActivation = true;
        while (!loadLogin.isDone) yield return null;

        Scene loadingScene = SceneManager.GetSceneByName(loadingSceneName);
        if (loadingScene.isLoaded)
        {
            AsyncOperation unloadLoading = SceneManager.UnloadSceneAsync(loadingScene);
            while (!unloadLoading.isDone) yield return null;
        }
    }
}