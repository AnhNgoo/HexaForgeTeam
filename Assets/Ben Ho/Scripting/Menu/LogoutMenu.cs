using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System;

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
        CheckIsInLobby();
    }

    private void OnDisable()
    {
        RemoveEvents();
        CheckIsInLobby();
    }

    private void CheckIsInLobby()
    {
        if (GameManager.Instance != null)
        {
            isInLobby = GameManager.Instance.MapType == MapType.Lobby;
        }
    }

    private bool IsInTutorialScene()
    {
        if (GameManager.Instance != null && GameManager.Instance.MapType == MapType.Tutorial)
        {
            return true;
        }

        GameSceneData sceneData = GameSceneData.Instance;
        string tutorialSceneName = sceneData != null
            ? sceneData.GetSceneName(SceneType.Tutorial)
            : "Tutorial Scene";

        string activeScene = SceneManager.GetActiveScene().name;
        if (activeScene.Equals(tutorialSceneName, StringComparison.OrdinalIgnoreCase) ||
            activeScene.IndexOf("Tutorial", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return true;
        }

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene s = SceneManager.GetSceneAt(i);
            if (s.isLoaded && (s.name.Equals(tutorialSceneName, StringComparison.OrdinalIgnoreCase) ||
                               s.name.IndexOf("Tutorial", StringComparison.OrdinalIgnoreCase) >= 0))
            {
                return true;
            }
        }

        return false;
    }

    private bool CheckIsInLobby()
    {
        if (IsInTutorialScene())
        {
            return true;
        }

        if (RunManager.Instance != null && RunManager.Instance.IsRunActive)
        {
            return false;
        }

        if (GameManager.Instance != null && GameManager.Instance.MapType != MapType.None)
        {
            return GameManager.Instance.MapType == MapType.Lobby;
        }

        GameSceneData sceneData = GameSceneData.Instance;
        if (sceneData != null)
        {
            string bossName = sceneData.GetSceneName(SceneType.FinalBoss);
            string run1Name = sceneData.GetSceneName(SceneType.RunGameplay);
            string run2Name = sceneData.GetSceneName(SceneType.RunGameplay2);

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene s = SceneManager.GetSceneAt(i);
                if (s.isLoaded)
                {
                    if (s.name.Equals(bossName, StringComparison.OrdinalIgnoreCase) ||
                        s.name.Equals(run1Name, StringComparison.OrdinalIgnoreCase) ||
                        (!string.IsNullOrEmpty(run2Name) && s.name.Equals(run2Name, StringComparison.OrdinalIgnoreCase)) ||
                        s.name.IndexOf("Boss", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        s.name.IndexOf("Arena", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return false;
                    }
                }
            }
        }

        string activeScene = SceneManager.GetActiveScene().name;
        if (activeScene.IndexOf("Boss", StringComparison.OrdinalIgnoreCase) >= 0 ||
            activeScene.IndexOf("Arena", StringComparison.OrdinalIgnoreCase) >= 0 ||
            activeScene.IndexOf("Run", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return false;
        }

        return true;
    }

    private void CheckContextState()
    {
        isInLobby = CheckIsInLobby();

        if (btnLogoutText != null)
        {
            btnLogoutText.text = "Confirm";
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
        CheckContextState();

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

        float duration = UnityEngine.Random.Range(5.0f, 7.0f);
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