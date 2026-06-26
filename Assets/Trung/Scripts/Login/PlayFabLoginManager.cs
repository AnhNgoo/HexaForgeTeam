using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

using PlayFab;
using PlayFab.ClientModels;

public class PlayFabLoginManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject registerPanel;

    [Header("Login")]
    [SerializeField] private TMP_Dropdown savedAccountDropdown;
    [SerializeField] private TMP_InputField loginAccountInput;
    [SerializeField] private TMP_InputField loginPasswordInput;
    [SerializeField] private Toggle rememberToggle;

    [Header("Register")]
    [SerializeField] private TMP_InputField registerUsernameInput;
    [SerializeField] private TMP_InputField registerEmailInput;
    [SerializeField] private TMP_InputField registerPasswordInput;
    [SerializeField] private TMP_InputField confirmPasswordInput;

    [Header("Status")]
    [SerializeField] private TMP_Text statusText;
    [Header("Scene")]
[SerializeField] private string loadingSceneName = "LoandScene";

    private SavedAccountList savedAccounts =
        new SavedAccountList();

    private void Start()
{
    if (rememberToggle != null)
    {
        rememberToggle.isOn = false;
    }

    LoadSavedAccounts();
}

    private void UpdateStatus(string message)
{
    if (statusText != null)
    {
        statusText.text = message;
    }

#if UNITY_EDITOR
    Debug.Log("[Đăng nhập] " + message);
#endif
}

    #region Panels

    public void OpenRegisterPanel()
    {
        loginPanel.SetActive(false);
        registerPanel.SetActive(true);

        UpdateStatus("");
    }

    public void OpenLoginPanel()
    {
        loginPanel.SetActive(true);
        registerPanel.SetActive(false);

        UpdateStatus("");
    }

    #endregion

    #region Register

    public void Register()
    {
        if (string.IsNullOrWhiteSpace(
            registerUsernameInput.text))
        {
            UpdateStatus("Vui lòng nhập tên tài khoản");
            return;
        }

        if (string.IsNullOrWhiteSpace(
            registerEmailInput.text))
        {
            UpdateStatus("Vui lòng nhập email");
            return;
        }

        if (string.IsNullOrWhiteSpace(
            registerPasswordInput.text))
        {
            UpdateStatus(
                "Vui lòng nhập mật khẩu");
            return;
        }

        if (registerPasswordInput.text !=
            confirmPasswordInput.text)
        {
            UpdateStatus(
                "Mật khẩu không khớp");
            return;
        }

        UpdateStatus("Đang tạo tài khoản...");

        RegisterPlayFabUserRequest request =
            new RegisterPlayFabUserRequest
            {
                Username =
                    registerUsernameInput.text,

                Email =
                    registerEmailInput.text,

                Password =
                    registerPasswordInput.text,

                RequireBothUsernameAndEmail =
                    true
            };

        PlayFabClientAPI.RegisterPlayFabUser(
            request,
            OnRegisterSuccess,
            OnPlayFabError);
    }

    private void OnRegisterSuccess(
    RegisterPlayFabUserResult result)
{
    PlayFabClientAPI.UpdateUserTitleDisplayName(
        new UpdateUserTitleDisplayNameRequest
        {
            DisplayName =
                registerUsernameInput.text
        },
        displayResult =>
        {
            Debug.Log(
                $"DisplayName set: {registerUsernameInput.text}");
        },
        error =>
        {
            Debug.LogError(
                error.GenerateErrorReport());
        });

    loginPanel.SetActive(true);
    registerPanel.SetActive(false);

    UpdateStatus("Tạo tài khoản thành công");
}

    #endregion

    #region Login

    public void Login()
{
    if (string.IsNullOrWhiteSpace(
        loginAccountInput.text))
    {
        UpdateStatus("Vui lòng nhập tài khoản hoặc email");

        return;
    }

    if (string.IsNullOrWhiteSpace(
        loginPasswordInput.text))
    {
        UpdateStatus("Vui lòng nhập mật khẩu");

        return;
    }

    UpdateStatus("Đang đăng nhập...");

    string account =
        loginAccountInput.text.Trim();

    if (account.Contains("@"))
    {
        LoginWithEmailAddressRequest request =
            new LoginWithEmailAddressRequest
            {
                Email = account,
                Password =
                    loginPasswordInput.text
            };

        PlayFabClientAPI.LoginWithEmailAddress(
            request,
            OnLoginSuccess,
            OnPlayFabError);
    }
    else
    {
        LoginWithPlayFabRequest request =
            new LoginWithPlayFabRequest
            {
                Username = account,
                Password =
                    loginPasswordInput.text
            };

        PlayFabClientAPI.LoginWithPlayFab(
            request,
            OnLoginSuccess,
            OnPlayFabError);
    }
}

    #endregion

    #region Remember Account

    private void SaveRememberAccount()
    {
        if (!rememberToggle.isOn)
        {
            return;
        }

        bool found = false;

        foreach (SavedAccount account
            in savedAccounts.accounts)
        {
            if (account.username ==
    loginAccountInput.text)
            {
                account.password =
                    loginPasswordInput.text;

                found = true;

                break;
            }
        }

        if (!found)
        {
            SavedAccount newAccount =
                new SavedAccount();

            newAccount.username =
    loginAccountInput.text;

            newAccount.password =
                loginPasswordInput.text;

            savedAccounts.accounts.Add(
                newAccount);
        }

        string json =
            JsonUtility.ToJson(
                savedAccounts);

        PlayerPrefs.SetString(
            "SavedAccounts",
            json);
    }

    private void LoadSavedAccounts()
    {
        string json =
            PlayerPrefs.GetString(
                "SavedAccounts",
                "");

        if (!string.IsNullOrEmpty(json))
        {
            savedAccounts =
                JsonUtility.FromJson<SavedAccountList>(
                    json);
        }

        RefreshDropdown();
    }

    private void RefreshDropdown()
    {
        if (savedAccountDropdown == null)
        {
            return;
        }

        savedAccountDropdown.ClearOptions();

        List<string> options =
            new List<string>();

        options.Add("Select Account");

        foreach (SavedAccount account
            in savedAccounts.accounts)
        {
            options.Add(
                account.username);
        }

        savedAccountDropdown.AddOptions(
            options);

        savedAccountDropdown.onValueChanged
            .RemoveAllListeners();

        savedAccountDropdown.onValueChanged
            .AddListener(
                OnAccountSelected);
    }

    private void OnAccountSelected(
        int index)
    {
        if (index <= 0)
        {
            return;
        }

        SavedAccount account =
            savedAccounts.accounts[
                index - 1];

        loginAccountInput.text =
    account.username;

        loginPasswordInput.text =
            account.password;
    }

    #endregion

    private void OnPlayFabError(
    PlayFabError error)
{
    string message =
        "Có lỗi xảy ra";

    switch (error.Error)
    {
        case PlayFabErrorCode.InvalidEmailAddress:
            message = "Email không hợp lệ";
            break;

        case PlayFabErrorCode.EmailAddressNotAvailable:
            message = "Email đã được sử dụng";
            break;

        case PlayFabErrorCode.UsernameNotAvailable:
            message = "Tên tài khoản đã tồn tại";
            break;

        case PlayFabErrorCode.InvalidUsername:
            message = "Tên tài khoản không hợp lệ";
            break;

        case PlayFabErrorCode.InvalidPassword:
            message = "Mật khẩu không hợp lệ";
            break;

        case PlayFabErrorCode.AccountNotFound:
            message = "Không tìm thấy tài khoản";
            break;

        case PlayFabErrorCode.InvalidParams:
            message = "Thông tin nhập vào không hợp lệ";
            break;

        case PlayFabErrorCode.InvalidUsernameOrPassword:
            message = "Sai tài khoản hoặc mật khẩu";
            break;

        default:
            message = error.ErrorMessage;
            break;
    }

    UpdateStatus(message);

    Debug.LogError(
        "[PlayFab] " +
        error.GenerateErrorReport());
}
    private void OnLoginSuccess(LoginResult result)
{
    UpdateStatus("Đăng nhập thành công");

    PlayerPrefs.SetString(
        "PlayFabID",
        result.PlayFabId);

    SaveRememberAccount();

    PlayerPrefs.Save();

    Debug.Log(
        $"[Đăng nhập] Thành công - PlayFabID: {result.PlayFabId}");
    CheckAndFixDisplayName();

    UnityEngine.SceneManagement.SceneManager
        .LoadScene(loadingSceneName);
}
private void CheckAndFixDisplayName()
{
    PlayFabClientAPI.GetAccountInfo(
        new GetAccountInfoRequest(),
        result =>
        {
            string displayName =
                result.AccountInfo.TitleInfo.DisplayName;

            if (!string.IsNullOrEmpty(displayName))
{
    PlayerPrefs.SetString(
        "DisplayName",
        displayName);

    PlayerPrefs.Save();

    return;
}

            string email =
                loginAccountInput.text.Trim();

            if (!email.Contains("@"))
            {
                return;
            }

            PlayFabClientAPI.UpdateUserTitleDisplayName(
                new UpdateUserTitleDisplayNameRequest
                {
                    DisplayName = email
                    
                },
                updateResult =>
                {
                    PlayerPrefs.SetString(
    "DisplayName",
    email);

PlayerPrefs.Save();
                    Debug.Log(
                        $"DisplayName updated to email: {email}");
                },
                error =>
                {
                    Debug.LogError(
                        error.GenerateErrorReport());
                });
        },
        error =>
        {
            Debug.LogError(
                error.GenerateErrorReport());
        });
}
}