using UnityEngine;

public class LobbyDialogueMenu : MenuBase
{
    [SerializeField]
    private GameObject root;

    public override MenuType menuType =>
        MenuType.LobbyDialogueMenu;

    public override void Open(object data = null)
    {
        if (root != null)
        {
            root.SetActive(true);
        }
    }

    public override void Close()
    {
        if (root != null)
        {
            root.SetActive(false);
        }
    }

    protected override void LoadComponent()
    {

    }

    protected override void LoadComponentRuntime()
    {

    }
}