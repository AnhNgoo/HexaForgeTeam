using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleMenu : MenuBase
{
    public override MenuType menuType => MenuType.TitleMenu;
    [SerializeField] private string lobbySceneName = "Lobby";
    [SerializeField] private Button btn_Play;
    [SerializeField] private Button btn_Help;
    [SerializeField] private Button btn_Credits;

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
        if (btn_Help == null)
            btn_Help = transform.Find("Btn_Help")?.GetComponent<Button>();
        if (btn_Credits == null)
            btn_Credits = transform.Find("Btn_Credits")?.GetComponent<Button>();
    }

    protected override void LoadComponentRuntime()
    {
        
    }

    public override void Open(object data = null)
    {
        base.Open(data);
        btn_Play.onClick.AddListener(OnPlayButtonClicked);
        btn_Help.onClick.AddListener(OnHelpButtonClicked);
        btn_Credits.onClick.AddListener(OnCreditsButtonClicked);
    }

    public override void Close()
    {
        base.Close();
        btn_Play.onClick.RemoveListener(OnPlayButtonClicked);
        btn_Help.onClick.RemoveListener(OnHelpButtonClicked);
        btn_Credits.onClick.RemoveListener(OnCreditsButtonClicked);
    }

    // Button callback stubs
    private void OnPlayButtonClicked()
    {
        Debug.Log("Play button clicked");

        Time.timeScale = 1f;

        LoadingData.TargetSceneName = "Lobby";
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
