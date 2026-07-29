using UnityEngine;

public class LobbyAchievementMenu : MenuBase
{
    public override MenuType menuType =>
        MenuType.LobbyAchievementMenu;

    public override void Open(object data = null)
    {
        base.Open(data);

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