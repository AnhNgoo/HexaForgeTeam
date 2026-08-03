using UnityEngine;
using UnityEngine.UI;

public class UIPanelCloseButton : LoadComponents
{
    [SerializeField] private Button button;

    protected override void LoadComponent()
    {
        if (button == null) button = GetComponent<Button>();
    }

    protected override void LoadComponentRuntime() { }

    private void Start()
    {
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(ClosePanel);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            LobbyRuneInventoryMenu runeMenu = GetComponentInParent<LobbyRuneInventoryMenu>();
            if (runeMenu != null && runeMenu.gameObject.activeInHierarchy)
            {
                if (runeMenu.HandleEscapeKey())
                {
                    return;
                }
            }

            LobbyGachaMenu gachaMenu = GetComponentInParent<LobbyGachaMenu>();
            if (gachaMenu == null && LobbyGachaMenu.FindFirstObjectByType<LobbyGachaMenu>() != null)
            {
                gachaMenu = LobbyGachaMenu.FindFirstObjectByType<LobbyGachaMenu>();
            }

            if (gachaMenu != null && gachaMenu.gameObject.activeInHierarchy)
            {
                if (gachaMenu.HandleEscapeKey())
                {
                    return;
                }
            }

            // Bổ sung bắt phím ESC phân tầng riêng cho Shop Menu
            LobbyShopMenu shopMenu = GetComponentInParent<LobbyShopMenu>();
            if (shopMenu == null && LobbyShopMenu.FindFirstObjectByType<LobbyShopMenu>() != null)
            {
                shopMenu = LobbyShopMenu.FindFirstObjectByType<LobbyShopMenu>();
            }

            if (shopMenu != null && shopMenu.gameObject.activeInHierarchy)
            {
                if (shopMenu.HandleEscapeKey())
                {
                    return;
                }
            }

            ClosePanel();
        }
    }

    private void ClosePanel()
    {
        if (UIManager.Instance == null) return;

        UnityEngine.SceneManagement.Scene activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (activeScene.name == "Run Scene")
        {
            UIManager.Instance.ChangeMenu(MenuType.GameplayMenu);
        }
        else
        {
            UIManager.Instance.ChangeMenu(MenuType.DefaultLobbyInputMenu);
        }

        if (InteractManagerV2.Instance != null)
        {
            InteractManagerV2.Instance.IsBusy = false;
            InteractManagerV2.Instance.ForceRefresh();
        }
    }
}