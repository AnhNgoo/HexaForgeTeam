using UnityEngine;

public class LobbyRunResultSummaryMenu : MenuBase
{
    public override MenuType menuType => MenuType.LobbyRunResultSummaryMenu;

    protected override void LoadComponent()
    {
    }

    protected override void LoadComponentRuntime()
    {
    }

    public override void Open(object data = null)
    {
        base.Open(data);

        if (gameObject != null)
        {
            gameObject.SetActive(true);
        }
    }

    public override void Close()
    {
        base.Close();

        if (gameObject != null)
        {
            gameObject.SetActive(false);
        }
    }
}