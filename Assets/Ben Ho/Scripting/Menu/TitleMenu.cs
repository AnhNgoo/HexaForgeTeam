using System.Collections;
using System.Collections.Generic;
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
        btn_Play.onClick.AddListener(OnPlayButtonClicked);
        btn_Credits.onClick.AddListener(OnCreditsButtonClicked);
        btn_Settings.onClick.AddListener(OnSettingsButtonClicked);

        if (btn_Logout != null)
            btn_Logout.onClick.AddListener(OnLogoutButtonClicked);
    }

    public override void Close()
    {
        base.Close();
        btn_Play.onClick.RemoveListener(OnPlayButtonClicked);
        btn_Credits.onClick.RemoveListener(OnCreditsButtonClicked);
        btn_Settings.onClick.RemoveListener(OnSettingsButtonClicked);

        if (btn_Logout != null)
            btn_Logout.onClick.RemoveListener(OnLogoutButtonClicked);
    }

    // Button callback stubs
    private void OnPlayButtonClicked()
    {
        Time.timeScale = 1f;

        GameSaveData saveData = SaveLoadManager.Instance?.SaveData;

        LoadingData.TargetSceneName =
            saveData?.isTutorialCompleted == true
                ? GameSceneData.Instance.lobbyMainScene
                : GameSceneData.Instance.tutorialScene;

        UIManager.Instance.ChangeMenu(MenuType.LoadingMenu);
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
        string loginSceneName = GameSceneData.Instance != null ? GameSceneData.Instance.loginScene : "Login Scene";
        string loadingSceneName = GameSceneData.Instance != null ? GameSceneData.Instance.loadingScene : "Loading Scene";

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

        Scene loadingScene = SceneManager.GetSceneByName(loadingSceneName);
        if (loadingScene.isLoaded)
        {
            AsyncOperation unloadLoading = SceneManager.UnloadSceneAsync(loadingScene);
            while (!unloadLoading.isDone) yield return null;
        }
    }

    private void OnHelpButtonClicked()
    {
        HelpMenuData.BackMenu = MenuType.TitleMenu;

        UIManager.Instance.ChangeMenu(MenuType.HelpMenu);
    }

    private void OnCreditsButtonClicked()
    {
        // UIManager.Instance.ChangeMenu(MenuType.CreditsMenu);
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