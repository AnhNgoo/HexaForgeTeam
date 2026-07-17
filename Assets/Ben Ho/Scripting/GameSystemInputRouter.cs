using UnityEngine;
using UnityEngine.InputSystem;

public class GameSystemInputRouter : MonoBehaviour
{
    private void Update()
    {
        if (Keyboard.current == null || UIManager.Instance == null)
            return;

        MenuType current = UIManager.Instance.CurrentMenuType;

        if (current == MenuType.GameSystemMenu)
            return;

        if (current != MenuType.GameplayMenu && current != MenuType.None)
            return;

        if (Keyboard.current.mKey.wasPressedThisFrame)
            OpenGameSystem(GameSystemTab.Map);

        if (Keyboard.current.iKey.wasPressedThisFrame)
            OpenGameSystem(GameSystemTab.Inventory);

        if (Keyboard.current.pKey.wasPressedThisFrame)
            OpenGameSystem(GameSystemTab.PlayerState);

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
            OpenGameSystem(GameSystemTab.System);
    }

    private void OpenGameSystem(GameSystemTab tab)
    {
        UIManager.Instance.OpenOverlayMenu(MenuType.GameSystemMenu, tab);
    }
}