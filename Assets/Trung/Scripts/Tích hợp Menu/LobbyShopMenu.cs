using UnityEngine;

public class LobbyShopMenu : MenuBase
{
    public override MenuType menuType => MenuType.LobbyShopMenu;

    public override void Open(object data = null)
    {
        base.Open(data);

        if (LobbyHUDTopBar.Instance != null)
        {
            LobbyHUDTopBar.Instance.ShowCurrencyOnly();
        }

        if (LobbyShopUI.Instance != null)
        {
            LobbyShopUI.Instance.OpenShop();
        }
    }

    public override void Close()
    {
        if (LobbyShopUI.Instance != null)
        {
            LobbyShopUI.Instance.HideShopPanel();
        }

        gameObject.SetActive(false);

        if (LobbyHUDTopBar.Instance != null)
        {
            LobbyHUDTopBar.Instance.ShowFullHUD();
        }
    }

    protected override void LoadComponent()
    {
    }

    protected override void LoadComponentRuntime()
    {
    }
}