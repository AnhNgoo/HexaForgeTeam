using UnityEngine;

public class LobbyLeaderboardMenu : MenuBase
{
    public override MenuType menuType =>
        MenuType.LobbyLeaderboardMenu;

    public override void Open(object data = null)
    {
        base.Open(data);

        if (LobbyHUDTopBar.Instance != null)
        {
            LobbyHUDTopBar.Instance.ShowCurrencyOnly();
        }

        LeaderboardUI ui =
            GetComponentInChildren<LeaderboardUI>(true);

        if (ui != null)
        {
            ui.OpenPanel();
        }
    }

    public override void Close()
    {
        LeaderboardUI ui =
            GetComponentInChildren<LeaderboardUI>(true);

        if (ui != null)
        {
            ui.ClosePanel();
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