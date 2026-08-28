using UnityEngine;

public class DefaultLobbyInputMenu : MenuBase
{
    public override MenuType menuType => MenuType.DefaultLobbyInputMenu;

    public override void Open(object data = null)
    {
        base.Open(data);
    }

    // private void Update()
    // {
    //     if (InputManager.InputActions.Keyboard.Escape.triggered)
    //     {
    //         UIManager.Instance.ChangeMenu(MenuType.GameSystemMenu);
    //     }
    // }

    protected override void LoadComponent()
    {
    }

    protected override void LoadComponentRuntime()
    {
    }
}