using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum SystemSettingPage
{
    Audio = 0,
    Graphics = 1,
    Controller = 2,
    Language = 3,
    Exit = 4,
    Logout = 5
}

public class SystemSettingsPanel : MonoBehaviour
{
    [Header("Tab Buttons")]
    [SerializeField] private Button btnAudio;
    [SerializeField] private Button btnGraphics;
    [SerializeField] private Button btnController;
    [SerializeField] private Button btnLanguage;
    [SerializeField] private Button btnExit;
    [SerializeField] private Button btnLogout;

    [Header("Setting Pages")]
    [SerializeField] private ScrollRect audioPage;
    [SerializeField] private ScrollRect graphicsPage;
    [SerializeField] private ScrollRect controllerPage;
    [SerializeField] private ScrollRect languagePage;
    [SerializeField] private ScrollRect exitPage;
    [SerializeField] private ScrollRect logoutPage;

    [Header("Parent Menu")]
    [SerializeField] private GameSystemMenu gameSystemMenu;

    [Header("Selected Lines")]
    [SerializeField] private GameObject audioLine;
    [SerializeField] private GameObject graphicsLine;
    [SerializeField] private GameObject controllerLine;
    [SerializeField] private GameObject languageLine;
    [SerializeField] private GameObject exitLine;
    [SerializeField] private GameObject logoutLine;

    [Header("Tab Labels")]
    [SerializeField] private TMP_Text audioLabel;
    [SerializeField] private TMP_Text graphicsLabel;
    [SerializeField] private TMP_Text controllerLabel;
    [SerializeField] private TMP_Text languageLabel;
    [SerializeField] private TMP_Text exitLabel;
    [SerializeField] private TMP_Text logoutLabel;

    [Header("Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = new Color(0.925f, 0.804f, 0.624f, 1f);

    [Header("Opening")]
    [SerializeField] private SystemSettingPage defaultPage = SystemSettingPage.Audio;
    [SerializeField] private bool resetScrollWhenOpenPage = true;

    private bool eventsAdded;
    private SystemSettingPage currentPage;

    public void Open()
    {
        gameObject.SetActive(true);
        AddEvents();
        ShowPage(defaultPage);
    }

    public void Close()
    {
        RemoveEvents();
        CloseAllPages();
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        AddEvents();
        ShowPage(defaultPage);
    }

    private void OnDisable()
    {
        RemoveEvents();
    }

    private void AddEvents()
    {
        if (eventsAdded) return;

        if (btnAudio != null)      btnAudio.onClick.AddListener(ShowAudio);
        if (btnGraphics != null)   btnGraphics.onClick.AddListener(ShowGraphics);
        if (btnController != null) btnController.onClick.AddListener(ShowController);
        if (btnLanguage != null)   btnLanguage.onClick.AddListener(ShowLanguage);
        if (btnExit != null)       btnExit.onClick.AddListener(ShowExit);
        if (btnLogout != null)     btnLogout.onClick.AddListener(ShowLogout);

        eventsAdded = true;
    }

    private void RemoveEvents()
    {
        if (!eventsAdded) return;

        if (btnAudio != null)      btnAudio.onClick.RemoveListener(ShowAudio);
        if (btnGraphics != null)   btnGraphics.onClick.RemoveListener(ShowGraphics);
        if (btnController != null) btnController.onClick.RemoveListener(ShowController);
        if (btnLanguage != null)   btnLanguage.onClick.RemoveListener(ShowLanguage);
        if (btnExit != null)       btnExit.onClick.RemoveListener(ShowExit);
        if (btnLogout != null)     btnLogout.onClick.RemoveListener(ShowLogout);

        eventsAdded = false;
    }

    public void ShowAudio()      => ShowPage(SystemSettingPage.Audio);
    public void ShowGraphics()   => ShowPage(SystemSettingPage.Graphics);
    public void ShowController() => ShowPage(SystemSettingPage.Controller);
    public void ShowLanguage()   => ShowPage(SystemSettingPage.Language);
    public void ShowExit()       => ShowPage(SystemSettingPage.Exit);
    public void ShowLogout()     => ShowPage(SystemSettingPage.Logout);

    public void ShowPage(SystemSettingPage page)
    {
        currentPage = page;

        SetPage(audioPage,      page == SystemSettingPage.Audio);
        SetPage(graphicsPage,   page == SystemSettingPage.Graphics);
        SetPage(controllerPage, page == SystemSettingPage.Controller);
        SetPage(languagePage,   page == SystemSettingPage.Language);
        SetPage(exitPage,       page == SystemSettingPage.Exit);
        SetPage(logoutPage,     page == SystemSettingPage.Logout);

        UpdateVisual(page);
    }

    private void CloseAllPages()
    {
        SetPage(audioPage, false);
        SetPage(graphicsPage, false);
        SetPage(controllerPage, false);
        SetPage(languagePage, false);
        SetPage(exitPage, false);
        SetPage(logoutPage, false);
    }

    private void SetPage(ScrollRect page, bool active)
    {
        if (page == null) return;

        page.gameObject.SetActive(active);

        if (active && resetScrollWhenOpenPage)
        {
            Canvas.ForceUpdateCanvases();
            page.verticalNormalizedPosition = 1f;
            page.horizontalNormalizedPosition = 0f;
        }
    }

    private void UpdateVisual(SystemSettingPage page)
    {
        bool audioSelected      = page == SystemSettingPage.Audio;
        bool graphicsSelected   = page == SystemSettingPage.Graphics;
        bool controllerSelected = page == SystemSettingPage.Controller;
        bool languageSelected   = page == SystemSettingPage.Language;
        bool exitSelected       = page == SystemSettingPage.Exit;
        bool logoutSelected     = page == SystemSettingPage.Logout;

        SetActive(audioLine,      audioSelected);
        SetActive(graphicsLine,   graphicsSelected);
        SetActive(controllerLine, controllerSelected);
        SetActive(languageLine,   languageSelected);
        SetActive(exitLine,       exitSelected);
        SetActive(logoutLine,     logoutSelected);

        SetLabel(audioLabel,      audioSelected);
        SetLabel(graphicsLabel,   graphicsSelected);
        SetLabel(controllerLabel, controllerSelected);
        SetLabel(languageLabel,   languageSelected);
        SetLabel(exitLabel,       exitSelected);
        SetLabel(logoutLabel,     logoutSelected);
    }

    private void SetActive(GameObject target, bool value)
    {
        if (target != null)
            target.SetActive(value);
    }

    private void SetLabel(TMP_Text label, bool selected)
    {
        if (label != null)
            label.color = selected ? selectedColor : normalColor;
    }

    public void CloseGameSystemMenu()
    {
        if (gameSystemMenu != null)
            gameSystemMenu.CloseToGameplay();
        else if (UIManager.Instance != null)
            UIManager.Instance.ChangeMenu(MenuType.GameplayMenu);
    }
}