using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using PlayFab;
using PlayFab.ClientModels;
using DG.Tweening;

public class PlayFabLoginManager : MonoBehaviour
{
    public static PlayFabLoginManager Instance;

    [Header("Universal Loading UI")]
    [SerializeField] private GameObject autoLoginLoadingPanel;
    [SerializeField] private CanvasGroup autoLoginCanvasGroup;
    [SerializeField] private Transform loadingSpinnerTransform;
    [SerializeField] private Slider loadingProgressSlider;
    [SerializeField] private TMP_Text autoLoginStatusText;

    [Header("Login Inputs")]
    [SerializeField] private TMP_Dropdown savedAccountDropdown;
    [SerializeField] private TMP_InputField loginAccountInput;
    [SerializeField] private TMP_InputField loginPasswordInput;
    [SerializeField] private Toggle rememberToggle;

    [Header("Register Inputs")]
    [SerializeField] private TMP_InputField registerUsernameInput;
    [SerializeField] private TMP_InputField registerEmailInput;
    [SerializeField] private TMP_InputField registerPasswordInput;
    [SerializeField] private TMP_InputField confirmPasswordInput;

    private SavedAccountList savedAccounts = new SavedAccountList();
    private bool isLoggingIn = false;
    private Tween spinnerTween;

    private const string AutoLoginKey = "IsAutoLoginActive";
    private const string LastAccountKey = "LastAccountUser";
    private const string LastPasswordKey = "LastAccountPass";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        if (rememberToggle != null) rememberToggle.isOn = false;

        LoadSavedAccounts();

        if (CheckAndTryAutoLogin())
        {
            return;
        }

        if (PlayFabLoginUI.Instance != null)
        {
            PlayFabLoginUI.Instance.SwitchTab(true);
        }
    }

    private bool CheckAndTryAutoLogin()
    {
        bool isAutoLoginActive = PlayerPrefs.GetInt(AutoLoginKey, 0) == 1;
        string lastUser = PlayerPrefs.GetString(LastAccountKey, "");
        string lastPass = PlayerPrefs.GetString(LastPasswordKey, "");

        if (isAutoLoginActive && !string.IsNullOrEmpty(lastUser) && !string.IsNullOrEmpty(lastPass))
        {
            if (loginAccountInput != null) loginAccountInput.text = lastUser;
            if (loginPasswordInput != null) loginPasswordInput.text = lastPass;

            ShowLoadingOverlay($"Welcome back! Reconnecting as {lastUser}...");
            StartCoroutine(AutoLoginRoutine());
            return true;
        }

        return false;
    }

    public void ShowLoadingOverlay(string message)
    {
        if (autoLoginLoadingPanel == null) return;

        autoLoginLoadingPanel.SetActive(true);

        if (autoLoginStatusText != null)
        {
            autoLoginStatusText.SetTextSafe(message);
        }

        if (autoLoginCanvasGroup == null)
        {
            autoLoginCanvasGroup = autoLoginLoadingPanel.GetComponent<CanvasGroup>();
            if (autoLoginCanvasGroup == null) autoLoginCanvasGroup = autoLoginLoadingPanel.AddComponent<CanvasGroup>();
        }

        autoLoginCanvasGroup.DOKill();
        autoLoginCanvasGroup.alpha = 0f;
        autoLoginCanvasGroup.DOFade(1f, 0.25f).SetUpdate(true);

        if (loadingSpinnerTransform != null)
        {
            loadingSpinnerTransform.DOKill();
            loadingSpinnerTransform.localRotation = Quaternion.identity;
            spinnerTween = loadingSpinnerTransform.DORotate(new Vector3(0, 0, -360f), 1.2f, RotateMode.FastBeyond360)
                .SetLoops(-1, LoopType.Incremental)
                .SetEase(Ease.Linear)
                .SetUpdate(true);
        }

        if (loadingProgressSlider != null)
        {
            loadingProgressSlider.DOKill();
            loadingProgressSlider.value = 0f;
            loadingProgressSlider.DOValue(0.85f, 2.0f).SetEase(Ease.OutQuad).SetUpdate(true);
        }
    }

    public void HideLoadingOverlay(System.Action onComplete = null)
    {
        if (spinnerTween != null) spinnerTween.Kill();

        if (autoLoginCanvasGroup != null)
        {
            autoLoginCanvasGroup.DOKill();
            autoLoginCanvasGroup.DOFade(0f, 0.2f).SetUpdate(true).OnComplete(() =>
            {
                if (autoLoginLoadingPanel != null) autoLoginLoadingPanel.SetActive(false);
                onComplete?.Invoke();
            });
        }
        else
        {
            if (autoLoginLoadingPanel != null) autoLoginLoadingPanel.SetActive(false);
            onComplete?.Invoke();
        }
    }

    private IEnumerator AutoLoginRoutine()
    {
        yield return new WaitForSeconds(1.0f);
        Login();
    }

    private void UpdateStatus(string message, Color? notifyColor = null)
    {
        Debug.Log($"[PlayFabLogin] Status: {message}");

        if (autoLoginStatusText != null && autoLoginLoadingPanel != null && autoLoginLoadingPanel.activeSelf)
        {
            autoLoginStatusText.SetTextSafe(message);
        }

        if (LobbyNotifyManager.Instance != null)
        {
            Color color = notifyColor ?? Color.white;
            LobbyNotifyManager.Instance.ShowNotify(message, color);
        }
    }

    public void Register()
    {
        if (string.IsNullOrWhiteSpace(registerUsernameInput.text))
        {
            ShakeInputField(registerUsernameInput);
            UpdateStatus("Username required.", Color.yellow);
            return;
        }

        if (string.IsNullOrWhiteSpace(registerEmailInput.text))
        {
            ShakeInputField(registerEmailInput);
            UpdateStatus("Email required.", Color.yellow);
            return;
        }

        if (string.IsNullOrWhiteSpace(registerPasswordInput.text))
        {
            ShakeInputField(registerPasswordInput);
            UpdateStatus("Password required.", Color.yellow);
            return;
        }

        if (registerPasswordInput.text != confirmPasswordInput.text)
        {
            ShakeInputField(registerPasswordInput);
            ShakeInputField(confirmPasswordInput);
            UpdateStatus("Passwords do not match.", Color.red);
            return;
        }

        ShowLoadingOverlay("Creating account...");
        UpdateStatus("Creating account...", Color.cyan);

        RegisterPlayFabUserRequest request = new RegisterPlayFabUserRequest
        {
            Username = registerUsernameInput.text.Trim(),
            Email = registerEmailInput.text.Trim(),
            Password = registerPasswordInput.text,
            RequireBothUsernameAndEmail = true
        };

        PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterSuccess, OnPlayFabError);
    }

    private void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        PlayFabClientAPI.UpdateUserTitleDisplayName(
            new UpdateUserTitleDisplayNameRequest { DisplayName = registerUsernameInput.text.Trim() },
            displayResult => { },
            error => { });

        loginAccountInput.text = registerUsernameInput.text;
        loginPasswordInput.text = registerPasswordInput.text;

        SaveRememberAccount();

        HideLoadingOverlay(() =>
        {
            if (PlayFabLoginUI.Instance != null) PlayFabLoginUI.Instance.SwitchTab(true);
            UpdateStatus("Account created successfully!", Color.green);
        });
    }

    public void Login()
    {
        if (isLoggingIn) return;

        if (string.IsNullOrWhiteSpace(loginAccountInput.text) || string.IsNullOrWhiteSpace(loginPasswordInput.text))
        {
            if (string.IsNullOrWhiteSpace(loginAccountInput.text)) ShakeInputField(loginAccountInput);
            if (string.IsNullOrWhiteSpace(loginPasswordInput.text)) ShakeInputField(loginPasswordInput);

            UpdateStatus("Username and Password required.", Color.yellow);
            if (autoLoginLoadingPanel != null && autoLoginLoadingPanel.activeSelf)
            {
                if (PlayFabLoginUI.Instance != null) PlayFabLoginUI.Instance.SwitchTab(true);
                HideLoadingOverlay();
            }
            return;
        }

        isLoggingIn = true;
        ShowLoadingOverlay("Connecting to server...");
        UpdateStatus("Connecting...", Color.cyan);

        string account = loginAccountInput.text.Trim();

        if (account.Contains("@"))
        {
            LoginWithEmailAddressRequest request = new LoginWithEmailAddressRequest
            {
                Email = account,
                Password = loginPasswordInput.text
            };

            PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnPlayFabError);
        }
        else
        {
            LoginWithPlayFabRequest request = new LoginWithPlayFabRequest
            {
                Username = account,
                Password = loginPasswordInput.text
            };

            PlayFabClientAPI.LoginWithPlayFab(request, OnLoginSuccess, OnPlayFabError);
        }
    }

    private void OnLoginSuccess(LoginResult result)
    {
        ShowLoadingOverlay("Login successful! Loading Cloud Profile...");

        if (loadingProgressSlider != null)
        {
            loadingProgressSlider.DOKill();
            loadingProgressSlider.DOValue(0.9f, 0.4f).SetEase(Ease.OutQuad).SetUpdate(true);
        }

        PlayerPrefs.SetString("PlayFabID", result.PlayFabId);
        PlayerPrefs.SetInt(AutoLoginKey, 1);
        PlayerPrefs.SetString(LastAccountKey, loginAccountInput.text.Trim());
        PlayerPrefs.SetString(LastPasswordKey, loginPasswordInput.text);

        SaveRememberAccount();
        PlayerPrefs.Save();

        if (SaveLoadManager.Instance != null)
        {
            SaveLoadManager.Instance.SetSaveFile(result.PlayFabId);
        }

        CheckAndFixDisplayName();

        System.GC.Collect();
        Resources.UnloadUnusedAssets();

        if (PlayFabDataManager.Instance != null)
        {
            PlayFabDataManager.Instance.LoadCloud((success) =>
            {
                StartCoroutine(LoadMainGameRoutine());
            });
        }
        else
        {
            StartCoroutine(LoadMainGameRoutine());
        }
    }

    private IEnumerator LoadMainGameRoutine()
    {
        GameSceneData sceneData = GameSceneData.Instance;

        string targetUiScene = sceneData != null 
            ? sceneData.GetSceneName(SceneType.UIGame) 
            : "UIGame";

        string targetLoadingScene = sceneData != null 
            ? sceneData.GetSceneName(SceneType.Loading) 
            : "Loading Scene";

        ShowLoadingOverlay("Preparing Realm...");

        AsyncOperation loadLoading = SceneManager.LoadSceneAsync(targetLoadingScene, LoadSceneMode.Additive);
        while (!loadLoading.isDone) yield return null;

        yield return new WaitForSecondsRealtime(0.05f);

        if (LoadingUIManager.Instance != null)
        {
            LoadingUIManager.Instance.SetDestinationName(targetUiScene);
        }

        HideLoadingOverlay();

        AsyncOperation loadTarget = SceneManager.LoadSceneAsync(targetUiScene, LoadSceneMode.Single);
        loadTarget.allowSceneActivation = false;

        float duration = Random.Range(5.0f, 7.0f);
        if (LoadingUIManager.Instance != null)
        {
            yield return StartCoroutine(LoadingUIManager.Instance.TrackProgressRoutine(loadTarget, false, duration));
        }

        loadTarget.allowSceneActivation = true;
        while (!loadTarget.isDone) yield return null;

        Scene loadingScene = SceneManager.GetSceneByName(targetLoadingScene);
        if (loadingScene.isLoaded)
        {
            AsyncOperation unloadLoading = SceneManager.UnloadSceneAsync(loadingScene);
            while (!unloadLoading.isDone) yield return null;
        }
    }

    private void CheckAndFixDisplayName()
    {
        PlayFabClientAPI.GetAccountInfo(
            new GetAccountInfoRequest(),
            result =>
            {
                string displayName = result.AccountInfo.TitleInfo.DisplayName;

                if (!string.IsNullOrEmpty(displayName))
                {
                    PlayerPrefs.SetString("DisplayName", displayName);
                    PlayerPrefs.Save();
                    return;
                }

                string email = loginAccountInput.text.Trim();
                if (!email.Contains("@")) return;

                PlayFabClientAPI.UpdateUserTitleDisplayName(
                    new UpdateUserTitleDisplayNameRequest { DisplayName = email },
                    updateResult =>
                    {
                        PlayerPrefs.SetString("DisplayName", email);
                        PlayerPrefs.Save();
                    },
                    error => { });
            },
            error => { });
    }

    private void SaveRememberAccount()
    {
        bool found = false;

        foreach (SavedAccount account in savedAccounts.accounts)
        {
            if (account.username == loginAccountInput.text)
            {
                account.password = loginPasswordInput.text;
                found = true;
                break;
            }
        }

        if (!found)
        {
            SavedAccount newAccount = new SavedAccount
            {
                username = loginAccountInput.text,
                password = loginPasswordInput.text
            };
            savedAccounts.accounts.Add(newAccount);
        }

        string json = JsonUtility.ToJson(savedAccounts);
        PlayerPrefs.SetString("SavedAccounts", json);
    }

    private void LoadSavedAccounts()
    {
        string json = PlayerPrefs.GetString("SavedAccounts", "");

        if (!string.IsNullOrEmpty(json))
        {
            savedAccounts = JsonUtility.FromJson<SavedAccountList>(json);
        }

        RefreshDropdown();
    }

    private void RefreshDropdown()
    {
        if (savedAccountDropdown == null) return;

        savedAccountDropdown.ClearOptions();

        List<string> options = new List<string> { "Select Account" };

        foreach (SavedAccount account in savedAccounts.accounts)
        {
            options.Add(account.username);
        }

        savedAccountDropdown.AddOptions(options);
        savedAccountDropdown.onValueChanged.RemoveAllListeners();
        savedAccountDropdown.onValueChanged.AddListener(OnAccountSelected);
    }

    private void OnAccountSelected(int index)
    {
        if (index <= 0) return;

        SavedAccount account = savedAccounts.accounts[index - 1];
        loginAccountInput.text = account.username;
        loginPasswordInput.text = account.password;
    }

    private void OnPlayFabError(PlayFabError error)
    {
        isLoggingIn = false;

        PlayerPrefs.SetInt(AutoLoginKey, 0);
        PlayerPrefs.Save();

        string message = error.ErrorMessage;

        HideLoadingOverlay(() =>
        {
            if (PlayFabLoginUI.Instance != null) PlayFabLoginUI.Instance.SwitchTab(true);
            UpdateStatus(message, Color.red);
        });
    }

    private void ShakeInputField(TMP_InputField inputField)
    {
        if (inputField == null) return;
        RectTransform rect = inputField.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.DOKill(true);
            rect.DOShakePosition(0.35f, new Vector3(10f, 0f, 0f), 20, 90f).SetUpdate(true);
        }
    }

    private void OnDestroy()
    {
        if (spinnerTween != null) spinnerTween.Kill();
    }
}