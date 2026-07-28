using UnityEngine;

public class DefaultLobbyInputMenu : MenuBase
{
    public override MenuType menuType => MenuType.DefaultLobbyInputMenu;

    public override void Open(object data = null)
    {
        base.Open(data);

        if (MouseManager.Instance != null)
        {
            MouseManager.Instance.HideMouse();
        }
    }

    public override void Close()
    {
        base.Close();
    }

    protected override void LoadComponent()
    {
    }

    protected override void LoadComponentRuntime()
    {
    }
}