using UnityEngine;

public class LobbyAchievementMenu : MenuBase
{
    public override MenuType menuType =>
        MenuType.LobbyAchievementMenu;

    public override void Open(object data = null)
    {
        base.Open(data);

        // Ẩn cụm Cấp độ tài khoản, chỉ giữ lại cụm Tiền tệ khi mở bảng Thành Tựu
        if (LobbyHUDTopBar.Instance != null)
        {
            LobbyHUDTopBar.Instance.ShowCurrencyOnly();
        }

        if (AchievementManager.Instance != null)
        {
            AchievementManager.Instance.OpenPanel();
        }
    }

    public override void Close()
    {
        if (AchievementManager.Instance != null)
        {
            AchievementManager.Instance.ClosePanel();
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