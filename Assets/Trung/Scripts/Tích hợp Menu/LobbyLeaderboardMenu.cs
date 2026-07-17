public class LobbyLeaderboardMenu : MenuBase
{
    public override MenuType menuType =>
        MenuType.LobbyLeaderboardMenu;

    public override void Open(object data = null)
    {
        base.Open(data);

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

        base.Close();
    }

    protected override void LoadComponent()
    {
    }

    protected override void LoadComponentRuntime()
    {
    }
}