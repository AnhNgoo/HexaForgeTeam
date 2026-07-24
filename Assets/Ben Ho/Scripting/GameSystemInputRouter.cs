using UnityEngine;
using UnityEngine.InputSystem;

public class GameSystemInputRouter : MonoBehaviour
{
    private void Update()
    {
        if (Keyboard.current == null ||
            UIManager.Instance == null)
        {
            return;
        }

        if (Keyboard.current.mKey.wasPressedThisFrame)
        {
            ToggleGameSystem(GameSystemTab.Map);
            return;
        }

        if (Keyboard.current.iKey.wasPressedThisFrame)
        {
            ToggleGameSystem(GameSystemTab.Inventory);
            return;
        }

        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            ToggleGameSystem(GameSystemTab.PlayerState);
            return;
        }

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ToggleGameSystem(GameSystemTab.System);
            return;
        }
    }

    private void ToggleGameSystem(GameSystemTab tab)
    {
        MenuType current =
            UIManager.Instance.CurrentMenuType;

        if (current != MenuType.GameplayMenu &&
            current != MenuType.None &&
            current != MenuType.GameSystemMenu)
        {
            return;
        }

        GameSystemMenu currentGameSystem =
            UIManager.Instance.CurrentMenu as GameSystemMenu;

        if (current == MenuType.GameSystemMenu &&
            currentGameSystem != null &&
            currentGameSystem.CurrentTab == tab)
        {
            UIManager.Instance.ChangeMenu(MenuType.GameplayMenu);
            return;
        }

        if (current == MenuType.GameSystemMenu &&
            currentGameSystem != null)
        {
            currentGameSystem.SelectTab(tab);
            return;
        }

        UIManager.Instance.ChangeMenu(
        MenuType.GameSystemMenu,
        tab);
    }
}