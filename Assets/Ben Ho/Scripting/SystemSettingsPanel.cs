using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum SystemSettingPage
{
    Audio = 0,
    Graphics = 1,
    Controller = 2
}

public class SystemSettingsPanel : MonoBehaviour
{
    [Header("Tab Buttons")]
    [SerializeField] private Button btnAudio;
    [SerializeField] private Button btnGraphics;
    [SerializeField] private Button btnController;

    [Header("Setting Pages")]
    [SerializeField] private ScrollRect audioPage;
    [SerializeField] private ScrollRect graphicsPage;
    [SerializeField] private ScrollRect controllerPage;

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
        if (eventsAdded)
            return;

        if (btnAudio != null)
            btnAudio.onClick.AddListener(ShowAudio);

        if (btnGraphics != null)
            btnGraphics.onClick.AddListener(ShowGraphics);

        if (btnController != null)
            btnController.onClick.AddListener(ShowController);

        eventsAdded = true;
    }

    private void RemoveEvents()
    {
        if (!eventsAdded)
            return;

        if (btnAudio != null)
            btnAudio.onClick.RemoveListener(ShowAudio);

        if (btnGraphics != null)
            btnGraphics.onClick.RemoveListener(ShowGraphics);

        if (btnController != null)
            btnController.onClick.RemoveListener(ShowController);

        eventsAdded = false;
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
        currentPage = page;

        SetPage(audioPage, page == SystemSettingPage.Audio);
        SetPage(graphicsPage, page == SystemSettingPage.Graphics);
        SetPage(controllerPage, page == SystemSettingPage.Controller);

        UpdateVisual(page);
    }

    private void CloseAllPages()
    {
        SetPage(audioPage, false);
        SetPage(graphicsPage, false);
        SetPage(controllerPage, false);
    }

    private void SetPage(ScrollRect page, bool active)
    {
        if (page == null)
            return;

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