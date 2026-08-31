using UnityEngine;

public class DefaultLobbyInputMenu : MenuBase
{
    public override MenuType menuType => MenuType.DefaultLobbyInputMenu;

    public override void Open(object data = null)
    {
        base.Open(data);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ChangeMenu(MenuType.LobbyQuestMenu);
            }
        }
    }

    protected override void LoadComponent()
    {
    }

    protected override void LoadComponentRuntime()
    {
    }
}