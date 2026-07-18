using UnityEngine;

public class LobbyLeaderboardMenu : MenuBase
{
    public override MenuType menuType =>
        MenuType.LobbyLeaderboardMenu;

    public override void Open(object data = null)
    {
        base.Open(data);

        // Ẩn cụm Cấp độ tài khoản, chỉ giữ lại cụm Tiền tệ khi mở Bảng Xếp Hạng
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

        base.Close();

        // Hiện lại đầy đủ cả Cấp độ lẫn Tiền tệ khi quay về sảnh trống
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