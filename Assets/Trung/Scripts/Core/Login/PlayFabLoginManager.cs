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
    [SerializeField] private string loadingSceneName = "Loading Scene";

    private SavedAccountList savedAccounts = new SavedAccountList();

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
            statusText.SetTextSafe(message);
        }
    }

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

    public void Register()
    {
        if (string.IsNullOrWhiteSpace(registerUsernameInput.text))
        {
            UpdateStatus("Username required.");
            if (LobbyNotifyManager.Instance != null) LobbyNotifyManager.Instance.ShowNotify("Username required.", Color.yellow);
            Debug.LogWarning("<color=#FFFF66><b>[ĐĂNG KÝ]</b> Thất bại - Thiếu tên tài khoản.</color>");
            return;
        }

        if (string.IsNullOrWhiteSpace(registerEmailInput.text))
        {
            UpdateStatus("Email required.");
            if (LobbyNotifyManager.Instance != null) LobbyNotifyManager.Instance.ShowNotify("Email required.", Color.yellow);
            Debug.LogWarning("<color=#FFFF66><b>[ĐĂNG KÝ]</b> Thất bại - Thiếu email đăng ký.</color>");
            return;
        }

        if (string.IsNullOrWhiteSpace(registerPasswordInput.text))
        {
            UpdateStatus("Password required.");
            if (LobbyNotifyManager.Instance != null) LobbyNotifyManager.Instance.ShowNotify("Password required.", Color.yellow);
            Debug.LogWarning("<color=#FFFF66><b>[ĐĂNG KÝ]</b> Thất bại - Thiếu mật khẩu đăng ký.</color>");
            return;
        }

        if (registerPasswordInput.text != confirmPasswordInput.text)
        {
            UpdateStatus("Passwords do not match.");
            if (LobbyNotifyManager.Instance != null) LobbyNotifyManager.Instance.ShowNotify("Passwords do not match.", Color.red);
            Debug.LogWarning("<color=#FF3333><b>[ĐĂNG KÝ]</b> Xử lý dừng - Mật khẩu nhập lại không trùng khớp.</color>");
            return;
        }

        UpdateStatus("Creating account...");
        if (LobbyNotifyManager.Instance != null) LobbyNotifyManager.Instance.ShowNotify("Creating account...", Color.cyan);
        Debug.Log("<color=#00FFCC><b>[ĐĂNG KÝ]</b> Gửi yêu cầu khởi tạo tài khoản mới lên hệ thống PlayFab...</color>");

        RegisterPlayFabUserRequest request = new RegisterPlayFabUserRequest
        {
            Username = registerUsernameInput.text,
            Email = registerEmailInput.text,
            Password = registerPasswordInput.text,
            RequireBothUsernameAndEmail = true
        };

        PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterSuccess, OnPlayFabError);
    }

    private void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        Debug.Log($"<color=#00FFCC><b>[ĐĂNG KÝ]</b> Tài khoản tạo thành công. ID hệ thống: {result.PlayFabId}. Bắt đầu gán DisplayName...</color>");

        PlayFabClientAPI.UpdateUserTitleDisplayName(
            new UpdateUserTitleDisplayNameRequest
            {
                DisplayName = registerUsernameInput.text
            },
            displayResult => {
                Debug.Log($"<color=#00FFCC><b>[ĐĂNG KÝ]</b> Đã gán DisplayName chuẩn theo Username thành công: {registerUsernameInput.text}</color>");
            },
            error => {
                Debug.LogError($"<color=#FF3333><b>[ĐĂNG KÝ LỖI DISPLAYNAME]</b> {error.GenerateErrorReport()}</color>");
            });

        loginPanel.SetActive(true);
        registerPanel.SetActive(false);

        UpdateStatus("Account created successfully.");
        if (LobbyNotifyManager.Instance != null) LobbyNotifyManager.Instance.ShowNotify("Registration complete!", Color.green);
    }

    public void Login()
    {
        if (string.IsNullOrWhiteSpace(loginAccountInput.text))
        {
            UpdateStatus("Username or Email required.");
            if (LobbyNotifyManager.Instance != null) LobbyNotifyManager.Instance.ShowNotify("Username or Email required.", Color.yellow);
            Debug.LogWarning("<color=#FFFF66><b>[ĐĂNG NHẬP]</b> Thất bại - Trường tên tài khoản/email để trống.</color>");
            return;
        }

        if (string.IsNullOrWhiteSpace(loginPasswordInput.text))
        {
            UpdateStatus("Password required.");
            if (LobbyNotifyManager.Instance != null) LobbyNotifyManager.Instance.ShowNotify("Password required.", Color.yellow);
            Debug.LogWarning("<color=#FFFF66><b>[ĐĂNG NHẬP]</b> Thất bại - Trường mật khẩu để trống.</color>");
            return;
        }

        UpdateStatus("Connecting...");
        if (LobbyNotifyManager.Instance != null) LobbyNotifyManager.Instance.ShowNotify("Connecting...", Color.cyan);

        string account = loginAccountInput.text.Trim();
        Debug.Log($"<color=#00FFCC><b>[ĐĂNG NHẬP]</b> Bắt đầu gửi thông tin xác thực cho tài khoản: {account}</color>");

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

    private void SaveRememberAccount()
    {
        if (!rememberToggle.isOn)
        {
            return;
        }

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
            SavedAccount newAccount = new SavedAccount();
            newAccount.username = loginAccountInput.text;
            newAccount.password = loginPasswordInput.text;
            savedAccounts.accounts.Add(newAccount);
        }

        string json = JsonUtility.ToJson(savedAccounts);
        PlayerPrefs.SetString("SavedAccounts", json);
        Debug.Log("<color=#FFFF66><b>[GHI NHỚ TÀI KHOẢN]</b> Đã cập nhật thông tin đăng nhập cục bộ thành công.</color>");
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
        if (savedAccountDropdown == null)
        {
            return;
        }

        savedAccountDropdown.ClearOptions();

        List<string> options = new List<string>();
        options.Add("Select Account");

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
        if (index <= 0)
        {
            return;
        }

        SavedAccount account = savedAccounts.accounts[index - 1];
        loginAccountInput.text = account.username;
        loginPasswordInput.text = account.password;
    }

    private void OnPlayFabError(PlayFabError error)
    {
        string message = "An error occurred.";

        switch (error.Error)
        {
            case PlayFabErrorCode.InvalidEmailAddress:
                message = "Invalid email address.";
                break;
            case PlayFabErrorCode.EmailAddressNotAvailable:
                message = "Email address already in use.";
                break;
            case PlayFabErrorCode.UsernameNotAvailable:
                message = "Username already exists.";
                break;
            case PlayFabErrorCode.InvalidUsername:
                message = "Invalid username.";
                break;
            case PlayFabErrorCode.InvalidPassword:
                message = "Invalid password layout.";
                break;
            case PlayFabErrorCode.AccountNotFound:
                message = "Account not found.";
                break;
            case PlayFabErrorCode.InvalidParams:
                message = "Invalid parameters entered.";
                break;
            case PlayFabErrorCode.InvalidUsernameOrPassword:
                message = "Wrong username or password.";
                break;
            default:
                message = error.ErrorMessage;
                break;
        }

        UpdateStatus(message);
        if (LobbyNotifyManager.Instance != null) LobbyNotifyManager.Instance.ShowNotify(message, Color.red);

        Debug.LogError($"<color=#FF3333><b>[PLAYFAB ERROR]</b> Kết nối thất bại: {error.GenerateErrorReport()}</color>");
    }

    private void OnLoginSuccess(LoginResult result)
    {
        UpdateStatus("Login successful.");
        if (LobbyNotifyManager.Instance != null) LobbyNotifyManager.Instance.ShowNotify("Login successful!", Color.green);

        PlayerPrefs.SetString("PlayFabID", result.PlayFabId);
        SaveRememberAccount();
        PlayerPrefs.Save();

        Debug.Log($"<color=#00FFCC><b>[ĐĂNG NHẬP]</b> Thành công - Kích hoạt phiên làm việc PlayFabID: {result.PlayFabId}</color>");

        if (SaveLoadManager.Instance != null)
        {
            SaveLoadManager.Instance.SetSaveFile(result.PlayFabId);
        }

        CheckAndFixDisplayName();

        if (PlayFabDataManager.Instance != null)
        {
            Debug.Log("<color=#00FFCC><b>[ĐĂNG NHẬP]</b> Gọi luồng LoadCloud từ PlayFabDataManager để xử lý dữ liệu...</color>");
            PlayFabDataManager.Instance.LoadCloud();
        }
        else
        {
            Debug.LogWarning("<color=#FFFF66><b>[ĐĂNG NHẬP WARNING]</b> Không tìm thấy PlayFabDataManager. Khởi chạy khẩn cấp bằng SceneManager Additive.</color>");
            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(loadingSceneName, UnityEngine.SceneManagement.LoadSceneMode.Additive);
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
                    Debug.Log($"<color=#00FFCC><b>[ĐỒNG BỘ DISPLAYNAME]</b> Đã đồng bộ DisplayName hiện tại về thiết bị: {displayName}</color>");
                    return;
                }

                string email = loginAccountInput.text.Trim();

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
                        PlayerPrefs.SetString("DisplayName", email);
                        PlayerPrefs.Save();
                        Debug.Log($"<color=#00FFCC><b>[ĐỒNG BỘ DISPLAYNAME]</b> Tài khoản trống DisplayName -> Đã tự động cập nhật theo Email thành công: {email}</color>");
                    },
                    error =>
                    {
                        Debug.LogError($"<color=#FF3333><b>[ĐỒNG BỘ DISPLAYNAME LỖI]</b> {error.GenerateErrorReport()}</color>");
                    });
            },
            error =>
            {
                Debug.LogError($"<color=#FF3333><b>[THÔNG TIN TÀI KHOẢN LỖI]</b> {error.GenerateErrorReport()}</color>");
            });
    }
}