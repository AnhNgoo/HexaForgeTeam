using UnityEngine;
using UnityEngine.UI;

public class LobbyTutorialMenu : MenuBase
{
    public override MenuType menuType => MenuType.LobbyTutorialMenu;

    [Header("UI Root Panel")]
    [SerializeField] private GameObject tutorialPanelRoot;

    [Header("Close Button")]
    [SerializeField] private Button closeButton;

    protected override void LoadComponent()
    {
        if (tutorialPanelRoot == null) tutorialPanelRoot = transform.Find("TutorialPanel")?.gameObject ?? gameObject;
        if (closeButton == null) closeButton = GetComponentInChildren<Button>();
    }

    protected override void LoadComponentRuntime() { }

    private void Start()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseTutorial);
        }
    }

    public override void Open(object data = null)
    {
        base.Open(data);

        if (tutorialPanelRoot != null) tutorialPanelRoot.SetActive(true);

        if (LobbyHUDTopBar.Instance != null)
        {
            LobbyHUDTopBar.Instance.ShowCurrencyOnly();
        }
    }

    public override void Close()
    {
        if (tutorialPanelRoot != null) tutorialPanelRoot.SetActive(false);

        base.Close();

        if (LobbyHUDTopBar.Instance != null)
        {
            LobbyHUDTopBar.Instance.ShowFullHUD();
        }
    }

    public void CloseTutorial()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ChangeMenu(MenuType.DefaultLobbyInputMenu);
        }

        if (InteractManagerV2.Instance != null)
        {
            InteractManagerV2.Instance.IsBusy = false;
            InteractManagerV2.Instance.ForceRefresh();
        }
    }
}