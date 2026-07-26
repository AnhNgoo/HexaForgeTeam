using UnityEngine;

public class LobbyGachaMenu : MenuBase
{
    [SerializeField] private GameObject gachaPanelRoot; 

    public override MenuType menuType =>
        MenuType.LobbyGachaMenu;

    public override void Open(object data = null)
    {
        base.Open(data);

        if (LobbyHUDTopBar.Instance != null)
        {
            LobbyHUDTopBar.Instance.ShowCurrencyOnly();
        }

        if (gachaPanelRoot != null)
        {
            gachaPanelRoot.SetActive(true);
        }
    }

    public override void Close()
    {
        if (GachaManager.Instance != null && GachaManager.Instance.IsRollActive())
        {
            GachaManager.Instance.SkipAllGachaAnimations();
        }

        if (GachaManager.Instance != null)
        {
            GachaManager.Instance.CloseResultPanel();
        }

        if (gachaPanelRoot != null)
        {
            gachaPanelRoot.SetActive(false);
        }

        gameObject.SetActive(false);

        if (LobbyHUDTopBar.Instance != null)
        {
            LobbyHUDTopBar.Instance.ShowFullHUD();
        }
    }

    public bool HandleEscapeKey()
    {
        if (GachaManager.Instance != null && GachaManager.Instance.IsRollActive())
        {
            GachaManager.Instance.SkipAllGachaAnimations();
            return true;
        }

        if (GachaUI.Instance != null && GachaUI.Instance.IsResultPanelActive())
        {
            GachaManager.Instance.CloseResultPanel();
            return true;
        }

        return false;
    }

    protected override void LoadComponent()
    {
    }

    protected override void LoadComponentRuntime()
    {
    }
}