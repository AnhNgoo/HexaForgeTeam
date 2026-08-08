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
    }

    public override void Close()
    {
        base.Close();
        btn_Play.onClick.RemoveListener(OnPlayButtonClicked);
        btn_Credits.onClick.RemoveListener(OnCreditsButtonClicked);
        btn_Settings.onClick.RemoveListener(OnSettingsButtonClicked);
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

    // *NOTE - Gợi event từ menu ra nơi khác
    private void OnTestEvent()
    {
        EventManager.Notify(GameEvent.OnBtn_TestEventFromMenuToOther);
    }

    // *NOTE - Nhận event từ nơi khác vào menu
    private void OnTestEventFromOtherToMenu(object obj)
    {
        Debug.Log("Test event from other to menu Triggered");
    }
}
