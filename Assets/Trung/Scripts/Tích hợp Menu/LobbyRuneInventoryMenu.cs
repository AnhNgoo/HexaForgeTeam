using UnityEngine;

public class LobbyRuneInventoryMenu : MenuBase
{
    public override MenuType menuType =>
        MenuType.LobbyRuneInventoryMenu;

    public override void Open(object data = null)
    {
        base.Open(data);

        if (LobbyHUDTopBar.Instance != null)
        {
            LobbyHUDTopBar.Instance.ShowCurrencyOnly();
        }

        if (RuneInventoryUI.Instance != null)
        {
            RuneInventoryUI.Instance.OpenInventory();
        }
    }

    public override void Close()
    {
        if (RuneInventoryUI.Instance != null)
        {
            RuneInventoryUI.Instance.CloseInventory();
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