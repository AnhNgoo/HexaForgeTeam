using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleMenu : MenuBase
{
    public override MenuType menuType => MenuType.TitleMenu;

    [SerializeField] private Button btn_Play;
    [SerializeField] private Button btn_Credits;
    [SerializeField] private Button btn_Settings;
    [SerializeField] private Button btn_Logout;

    private void Start()
    {
        EventManager.Subscribe(GameEvent.OnTestEventFromOtherToMenu, OnTestEventFromOtherToMenu);

        // Đăng ký sự kiện Nút bấm 1 lần duy nhất ở Start để tránh trùng lặp Event
        if (btn_Play != null)
        {
            btn_Play.onClick.RemoveAllListeners();
            btn_Play.onClick.AddListener(OnPlayButtonClicked);
        }

        if (btn_Credits != null)
        {
            btn_Credits.onClick.RemoveAllListeners();
            btn_Credits.onClick.AddListener(OnCreditsButtonClicked);
        }

        if (btn_Settings != null)
        {
            btn_Settings.onClick.RemoveAllListeners();
            btn_Settings.onClick.AddListener(OnSettingsButtonClicked);
        }

        if (btn_Logout != null)
        {
            btn_Logout.onClick.RemoveAllListeners();
            btn_Logout.onClick.AddListener(OnLogoutButtonClicked);
        }
    }

    private void OnDestroy()
    {
        EventManager.Unsubscribe(GameEvent.OnTestEventFromOtherToMenu, OnTestEventFromOtherToMenu);
    }

    protected override void LoadComponent()
    {
        if (btn_Play == null)
            btn_Play = transform.Find("Btn_Play")?.GetComponent<Button>();
        if (btn_Credits == null)
            btn_Credits = transform.Find("Btn_Credits")?.GetComponent<Button>();
        if (btn_Settings == null)
            btn_Settings = transform.Find("Btn_Settings")?.GetComponent<Button>();
        if (btn_Logout == null)
            btn_Logout = transform.Find("Btn_Logout")?.GetComponent<Button>();
    }

    protected override void LoadComponentRuntime()
    {
    }

    public override void Open(object data = null)
    {
        base.Open(data);
    }

    public override void Close()
    {
        base.Close();
    }

    // Button callback stubs
    private void OnPlayButtonClicked()
    {
        Time.timeScale = 1f;

        GameSaveData saveData = SaveLoadManager.Instance?.SaveData;

        // BẮT BỘC DÙNG GetSceneName(...) ĐỂ LẤY ĐÚNG CONFIG OVERRIDE CÁ NHÂN (VD: LobbyMainGameTrung)
        string targetScene = (saveData != null && saveData.isTutorialCompleted)
            ? GameSceneData.Instance.GetSceneName(SceneType.LobbyMain)
            : GameSceneData.Instance.GetSceneName(SceneType.Tutorial);

        Debug.Log($"<color=#00FFCC>[TitleMenu Play] Đang chuyển hướng tới Scene: {targetScene}</color>");

        StartCoroutine(PlayTransitionRoutine(targetScene));
    }

    private IEnumerator PlayTransitionRoutine(string targetSceneName)
    {
        string loadingSceneName = GameSceneData.Instance != null 
            ? GameSceneData.Instance.GetSceneName(SceneType.Loading) 
            : "Loading Scene";

        // 1. Load Additive Loading Scene
        AsyncOperation loadLoading = SceneManager.LoadSceneAsync(loadingSceneName, LoadSceneMode.Additive);
        while (!loadLoading.isDone) yield return null;

        yield return new WaitForSecondsRealtime(0.1f);

        if (LoadingUIManager.Instance != null)
        {
            LoadingUIManager.Instance.SetDestinationName(targetSceneName);
        }

        // 2. Load Scene mục tiêu ngầm
        AsyncOperation loadTarget = SceneManager.LoadSceneAsync(targetSceneName, LoadSceneMode.Single);
        loadTarget.allowSceneActivation = false;

        if (LoadingUIManager.Instance != null)
        {
            yield return StartCoroutine(LoadingUIManager.Instance.TrackProgressRoutine(loadTarget));
        }

        // 3. Kích hoạt Scene mục tiêu
        loadTarget.allowSceneActivation = true;
        while (!loadTarget.isDone) yield return null;
    }

    private void OnLogoutButtonClicked()
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
        string loginSceneName = GameSceneData.Instance != null 
            ? GameSceneData.Instance.GetSceneName(SceneType.Login) 
            : "Login Scene";
            
        string loadingSceneName = GameSceneData.Instance != null 
            ? GameSceneData.Instance.GetSceneName(SceneType.Loading) 
            : "Loading Scene";

        AsyncOperation loadLoading = SceneManager.LoadSceneAsync(loadingSceneName, LoadSceneMode.Additive);
        while (!loadLoading.isDone) yield return null;

        yield return new WaitForSecondsRealtime(0.1f);

        if (LoadingUIManager.Instance != null)
        {
            LoadingUIManager.Instance.SetDestinationName(loginSceneName);
        }

        AsyncOperation loadLogin = SceneManager.LoadSceneAsync(loginSceneName, LoadSceneMode.Single);
        loadLogin.allowSceneActivation = false;

        if (LoadingUIManager.Instance != null)
        {
            yield return StartCoroutine(LoadingUIManager.Instance.TrackProgressRoutine(loadLogin));
        }

        loadLogin.allowSceneActivation = true;
        while (!loadLogin.isDone) yield return null;
    }

    private void OnHelpButtonClicked()
    {
        HelpMenuData.BackMenu = MenuType.TitleMenu;
        UIManager.Instance.ChangeMenu(MenuType.HelpMenu);
    }

    private void OnCreditsButtonClicked()
    {
    }

    private void OnSettingsButtonClicked()
    {
        SettingMenuData.BackMenu = MenuType.TitleMenu;
        UIManager.Instance.ChangeMenu(MenuType.SettingMenu);
    }

    private void OnLanguageButtonClicked()
    {
        UIManager.Instance.ChangeMenu(MenuType.LanguageMenu);
    }

    private void OnAchievementButtonClicked()
    {
        UIManager.Instance.ChangeMenu(MenuType.AchievementMenu);
    }

    private void OnTestEvent()
    {
        EventManager.Notify(GameEvent.OnBtn_TestEventFromMenuToOther);
    }

    private void OnTestEventFromOtherToMenu(object obj)
    {
        Debug.Log("Test event from other to menu Triggered");
    }
}