using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LogoutLabelController : MonoBehaviour
{
    [Header("Label logout trong sidebar + tiêu đề popup")]
    [SerializeField] private TMP_Text[] logoutTexts;

    [Header("Nút xác nhận trong popup logout")]
    [SerializeField] private TMP_Text confirmButtonText;

    [Header("Label 'Language' trong sidebar")]
    [SerializeField] private TMP_Text languageLabel;

    [Header("Dò ngôn ngữ hiện tại (kéo chữ Thoát/Exit vào)")]
    [SerializeField] private TMP_Text detectText;
    [SerializeField] private string detectVI = "Thoát";

    [Header("Tiếng Anh")]
    [SerializeField] private string inGameEN = "Return to Lobby";
    [SerializeField] private string lobbyEN = "Logout";
    [SerializeField] private string languageEN = "Language";
    [SerializeField] private string confirmEN = "Confirm";

    [Header("Tiếng Việt")]
    [SerializeField] private string inGameVI = "Quay lại Sảnh chờ";
    [SerializeField] private string lobbyVI = "Đăng xuất";
    [SerializeField] private string languageVI = "Ngôn ngữ";
    [SerializeField] private string confirmVI = "Xác nhận";

    private void OnEnable() => Apply();
    private void LateUpdate() => Apply();

    private bool IsVietnamese()
    {
        return detectText != null && detectText.text == detectVI;
    }

    private bool IsInLobby()
    {
        return SceneManager.GetActiveScene().name.ToLower().Contains("lobby");
    }

    private void Apply()
    {
        bool vi = IsVietnamese();
        bool inLobby = IsInLobby();

        string logout = vi
            ? (inLobby ? lobbyVI : inGameVI)
            : (inLobby ? lobbyEN : inGameEN);

        SetTexts(logoutTexts, logout);

        if (languageLabel != null)
            SetText(languageLabel, vi ? languageVI : languageEN);

        if (confirmButtonText != null)
            SetText(confirmButtonText, vi ? confirmVI : confirmEN);
    }

    private void SetTexts(TMP_Text[] arr, string value)
    {
        if (arr == null)
            return;

        for (int i = 0; i < arr.Length; i++)
            if (arr[i] != null)
                SetText(arr[i], value);
    }

    private void SetText(TMP_Text t, string value)
    {
        if (t.text != value)
            t.text = value;
    }
}