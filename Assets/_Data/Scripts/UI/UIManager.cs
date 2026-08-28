using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Sirenix.OdinInspector;
using UnityEngine.SceneManagement;
using System.Linq;
using DG.Tweening;

public enum MenuType
{
    None = 0,

    TitleMenu = 1,
    LoadingMenu = 2,
    GameplayMenu = 3,
    InventoryMenu = 4,
    SettingMenu = 5,
    PauseMenu = 6,
    LevelMenu = 7,
    GachaMenu = 8,
    StoreMenu = 9,
    InventoryRuneMenu = 10,
    HelpMenu = 11,
    CharacterMenu = 12,
    HUDMenuTest = 13,
    TrophyMenu = 14,
    LanguageMenu = 15,
    GraphicsMenu = 16,
    ControllerMenu = 17,
    GameSystemMenu = 18,
    AchievementMenu = 19,
    CreditsMenu = 20,
    PlayerStateMenu = 21,
    DormantPowerMenu = 48,

    LobbyCharacterMenu = 100,
    LobbyRuneInventoryMenu = 101,
    LobbyAchievementMenu = 102,
    LobbyGachaMenu = 103,
    LobbyAccountLevelMenu = 104,
    LobbyDialogueMenu = 105,
    LobbyLeaderboardMenu = 106,
    LobbyShopMenu = 107,
    DefaultLobbyInputMenu = 108,
    YouDiedRespawnMenu = 109,
    LobbyRunResultSummaryMenu = 110,
    LobbyBossSelectMenu = 111,
    LobbyTutorialMenu = 112,
}

public class UIManager : Singleton<UIManager>
{
    [SerializeField] private bool autoOpenFirstMenu = false;
    [SerializeField] private MenuType firstMenuToOpen = MenuType.TitleMenu;
    [SerializeField] GameObject canvas;
    [SerializeField] string canvasPath = "Canvas";
    [ShowInInspector] public MenuType CurrentMenuType { get; private set; }
    [ShowInInspector] public MenuType PreviousMenuType { get; private set; }
    [SerializeField] List<MenuData> menus = new List<MenuData>();

    [ShowInInspector] public MenuBase CurrentMenu { get; private set; }

    private Dictionary<MenuType, MenuBase> menuFastLookup = new Dictionary<MenuType, MenuBase>();

    [Serializable]
    public class MenuData
    {
        public MenuType menuType;
        public MenuBase menuBase;
    }

    protected override void LoadComponent()
    {
        base.LoadComponent();
        LoadMenus();
    }

    protected override void Awake()
    {
        base.Awake();
        AutoOpenFirstMenu();
    }
    private void AutoOpenFirstMenu()
    {
        if (autoOpenFirstMenu)
        {
            InitUI();
            ChangeMenu(firstMenuToOpen);
        }
    }
    private void LoadMenus()
    {
        if (canvas == null)
            canvas = transform.Find(canvasPath)?.gameObject;

        if (canvas == null)
            return;

        List<MenuBase> menuList = new List<MenuBase>(canvas.GetComponentsInChildren<MenuBase>(true));

        if (menuList == null || menuList.Count == 0)
            return;

        menus.Clear();
        menuFastLookup.Clear();

        foreach (MenuBase menu in menuList)
        {
            Transform parent = menu.transform.parent;

            bool nestedInsideAnotherMenu =
                parent != null &&
                parent.GetComponentInParent<MenuBase>(true) != null;

            if (nestedInsideAnotherMenu)
                continue;

            menus.Add(new MenuData
            {
                menuType = menu.menuType,
                menuBase = menu
            });

            if (!menuFastLookup.ContainsKey(menu.menuType))
            {
                menuFastLookup.Add(menu.menuType, menu);
            }
        }
    }

    //Chuyển đổi menu
    public void ChangeMenu(MenuType menuType, object data = null)
    {
        menus.RemoveAll(m => m == null || m.menuBase == null);

        MenuBase targetMenu = null;

        // Thử lấy từ Fast Lookup trước
        if (!menuFastLookup.TryGetValue(menuType, out targetMenu) || targetMenu == null)
        {
            var menuData = menus.FirstOrDefault(m => m.menuType == menuType);
            if (menuData == null || menuData.menuBase == null)
            {
                LoadMenus();
                menuData = menus.FirstOrDefault(m => m.menuType == menuType);
                if (menuData == null || menuData.menuBase == null) return;
            }
            targetMenu = menuData.menuBase;
        }

        PreviousMenuType = CurrentMenu?.menuType ?? MenuType.None;

        if (CurrentMenu != null)
            CurrentMenu.Close();

        CurrentMenu = targetMenu;
        if (CurrentMenu != null)
            CurrentMenu.Open(data);

        CurrentMenuType = CurrentMenu.menuType;
    }

    //Đóng tất cả menu
    public void CloseAllMenus()
    {
        menus.RemoveAll(m => m == null || m.menuBase == null);

        foreach (var menu in menus)
        {
            if (menu?.menuBase != null)
                menu.menuBase.Close();
        }
    }

    public void InitUI()
    {
        menus.RemoveAll(
            menu => menu == null || menu.menuBase == null
        );

        foreach (MenuData menu in menus)
        {
            if (menu?.menuBase == null)
                continue;

            menu.menuBase.gameObject.SetActive(false);
        }

        CurrentMenu = null;
        CurrentMenuType = MenuType.None;
        PreviousMenuType = MenuType.None;

        Debug.Log("UIManager initialized. All menus are closed.");
    }
}
