using UnityEngine;

public class LobbyMenuOpener : MonoBehaviour
{
    [SerializeField] private MenuType targetMenu;

    public void OnInteract()
    {
        if (LobbyUIOverlayManager.Instance != null)
        {
            LobbyUIOverlayManager.Instance.OpenMenu(targetMenu);
            return;
        }

        if (UIManager.Instance != null)
            UIManager.Instance.ChangeMenu(targetMenu);
    }
}