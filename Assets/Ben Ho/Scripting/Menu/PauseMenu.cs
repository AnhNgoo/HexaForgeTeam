using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MenuBase
{
    public override MenuType menuType => MenuType.PauseMenu;

    [Header("Buttons")]
    [SerializeField] private Button btn_Continue;
    [SerializeField] private Button btn_NewGame;
    [SerializeField] private Button btn_Setting;
    [SerializeField] private Button btn_Exit;

    protected override void LoadComponent()
    {
        if (btn_Continue == null)
            btn_Continue = transform.Find("Btn_Continue")?.GetComponent<Button>();

        if (btn_NewGame == null)
            btn_NewGame = transform.Find("Btn_NewGame")?.GetComponent<Button>();

        if (btn_Setting == null)
            btn_Setting = transform.Find("Btn_Setting")?.GetComponent<Button>();

        if (btn_Exit == null)
            btn_Exit = transform.Find("Btn_Exit")?.GetComponent<Button>();
    }

    protected override void LoadComponentRuntime()
    {

    }

    public override void Open(object data = null)
    {
        base.Open(data);

        if (btn_Continue != null)
        {
            btn_Continue.onClick.RemoveListener(OnContinueButtonClicked);
            btn_Continue.onClick.AddListener(OnContinueButtonClicked);
        }

        if (btn_NewGame != null)
        {
            btn_NewGame.onClick.RemoveListener(OnNewGameButtonClicked);
            btn_NewGame.onClick.AddListener(OnNewGameButtonClicked);
        }

        if (btn_Setting != null)
        {
            btn_Setting.onClick.RemoveListener(OnSettingButtonClicked);
            btn_Setting.onClick.AddListener(OnSettingButtonClicked);
        }

        if (btn_Exit != null)
        {
            btn_Exit.onClick.RemoveListener(OnExitButtonClicked);
            btn_Exit.onClick.AddListener(OnExitButtonClicked);
        }
    }

    public override void Close()
    {
        if (btn_Continue != null)
            btn_Continue.onClick.RemoveListener(OnContinueButtonClicked);

        if (btn_NewGame != null)
            btn_NewGame.onClick.RemoveListener(OnNewGameButtonClicked);

        if (btn_Setting != null)
            btn_Setting.onClick.RemoveListener(OnSettingButtonClicked);

        if (btn_Exit != null)
            btn_Exit.onClick.RemoveListener(OnExitButtonClicked);

        base.Close();
    }

    private void OnContinueButtonClicked()
    {
        Debug.Log("Continue button clicked");

        Time.timeScale = 1f;

        UIManager.Instance.ChangeMenu(MenuType.GameplayMenu);
    }

    private void OnNewGameButtonClicked()
    {
        Debug.Log("New Game button clicked");

        Time.timeScale = 1f;

        LoadingData.TargetMenu = MenuType.GameplayMenu;
        UIManager.Instance.ChangeMenu(MenuType.LoadingMenu);
    }

    private void OnSettingButtonClicked()
    {
        Debug.Log("Setting button clicked");

        SettingMenuData.BackMenu = MenuType.PauseMenu;

        UIManager.Instance.ChangeMenu(MenuType.SettingMenu);
    }

    private void OnExitButtonClicked()
    {
        Debug.Log("Exit button clicked");

        Time.timeScale = 1f;

        UIManager.Instance.ChangeMenu(MenuType.TitleMenu);
    }
}