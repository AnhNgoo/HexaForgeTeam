using TMPro;
using UnityEngine;

public enum SystemSettingPage
{
    Audio = 0,
    Graphics = 1,
    Controller = 2
}

public class SystemSettingsPanel : MonoBehaviour
{
    [Header("Setting Pages")]
    [SerializeField] private SettingMenu audioMenu;
    [SerializeField] private GraphicsMenu graphicsMenu;
    [SerializeField] private ControllerMenu controllerMenu;

    [Header("Parent Menu")]
    [SerializeField] private GameSystemMenu gameSystemMenu;

    [Header("Selected Lines")]
    [SerializeField] private GameObject audioLine;
    [SerializeField] private GameObject graphicsLine;
    [SerializeField] private GameObject controllerLine;

    [Header("Tab Labels")]
    [SerializeField] private TMP_Text audioLabel;
    [SerializeField] private TMP_Text graphicsLabel;
    [SerializeField] private TMP_Text controllerLabel;

    [Header("Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor =
        new Color(0.925f, 0.804f, 0.624f, 1f);

    [Header("Opening")]
    [SerializeField] private SystemSettingPage defaultPage =
        SystemSettingPage.Audio;

    private SystemSettingPage currentPage;
    private bool hasCurrentPage;
    private bool isClosing;
    private bool openedByGameSystem;

    public void Open()
    {
        openedByGameSystem = true;
        gameObject.SetActive(true);
        ShowPage(defaultPage);
        openedByGameSystem = false;
    }

    public void Close()
    {
        CloseAllPages();
        hasCurrentPage = false;
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (!openedByGameSystem)
            ShowPage(defaultPage);
    }

    private void OnDisable()
    {
        CloseAllPages();
        hasCurrentPage = false;
    }

    public void ShowAudio()
    {
        ShowPage(SystemSettingPage.Audio);
    }

    public void ShowGraphics()
    {
        ShowPage(SystemSettingPage.Graphics);
    }

    public void ShowController()
    {
        ShowPage(SystemSettingPage.Controller);
    }

    public void ShowPage(SystemSettingPage page)
    {
        if (isClosing)
            return;

        if (hasCurrentPage &&
            currentPage == page &&
            IsPageActive(page))
        {
            UpdateVisual(page);
            return;
        }

        CloseAllPages();

        currentPage = page;
        hasCurrentPage = true;

        switch (page)
        {
            case SystemSettingPage.Audio:
                if (audioMenu != null)
                    audioMenu.Open();
                break;

            case SystemSettingPage.Graphics:
                if (graphicsMenu != null)
                    graphicsMenu.Open();
                break;

            case SystemSettingPage.Controller:
                if (controllerMenu != null)
                    controllerMenu.Open();
                break;
        }

        UpdateVisual(page);
    }

    private bool IsPageActive(SystemSettingPage page)
    {
        switch (page)
        {
            case SystemSettingPage.Audio:
                return audioMenu != null &&
                       audioMenu.gameObject.activeSelf;

            case SystemSettingPage.Graphics:
                return graphicsMenu != null &&
                       graphicsMenu.gameObject.activeSelf;

            case SystemSettingPage.Controller:
                return controllerMenu != null &&
                       controllerMenu.gameObject.activeSelf;
        }

        return false;
    }

    private void CloseAllPages()
    {
        if (isClosing)
            return;

        isClosing = true;

        if (audioMenu != null && audioMenu.gameObject.activeSelf)
            audioMenu.Close();

        if (graphicsMenu != null && graphicsMenu.gameObject.activeSelf)
            graphicsMenu.Close();

        if (controllerMenu != null && controllerMenu.gameObject.activeSelf)
            controllerMenu.Close();

        isClosing = false;
    }

    private void UpdateVisual(SystemSettingPage page)
    {
        bool audioSelected = page == SystemSettingPage.Audio;
        bool graphicsSelected = page == SystemSettingPage.Graphics;
        bool controllerSelected = page == SystemSettingPage.Controller;

        SetActive(audioLine, audioSelected);
        SetActive(graphicsLine, graphicsSelected);
        SetActive(controllerLine, controllerSelected);

        SetLabel(audioLabel, audioSelected);
        SetLabel(graphicsLabel, graphicsSelected);
        SetLabel(controllerLabel, controllerSelected);
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