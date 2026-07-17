using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Sirenix.OdinInspector;
using UnityEngine.SceneManagement;
using System.Linq;

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
    GraphicsMenu = 16,     // Đảm bảo có dòng này
    ControllerMenu = 17,   // Đảm bảo có dòng này
    GameSystemMenu = 18,
    AchievementMenu = 19,

    LobbyCharacterMenu = 100,
    LobbyInventoryMenu = 101,
    LobbyAchievementMenu = 102,
    LobbyGachaMenu = 103,
    LobbyAccountLevelMenu = 104,
    LobbyDialogueMenu = 105,
    LobbyLeaderboardMenu = 106,
}

public class UIManager : Singleton<UIManager>
{
    [SerializeField] GameObject canvas;
    [SerializeField] string canvasPath = "Canvas";
    [ShowInInspector] public MenuType CurrentMenuType { get; private set; }
    [ShowInInspector] public MenuType PreviousMenuType { get; private set; }
    [SerializeField] List<MenuData> menus = new List<MenuData>();

    [ShowInInspector] public MenuBase CurrentMenu { get; private set; }

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
        foreach (MenuBase menu in menuList)
        {
            Transform parent =
                menu.transform.parent;

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
        }
    }


    //Chuyển đổi menu
    public void ChangeMenu(MenuType menuType, object data = null)
    {
        menus.RemoveAll(m => m == null || m.menuBase == null);

        var menuData = menus.FirstOrDefault(m => m.menuType == menuType);
        if (menuData == null || menuData.menuBase == null)
        {
            LoadMenus();
            menuData = menus.FirstOrDefault(m => m.menuType == menuType);
            if (menuData == null || menuData.menuBase == null) return;
        }

        PreviousMenuType = CurrentMenu?.menuType ?? MenuType.None;

        if (CurrentMenu != null)
            CurrentMenu.Close();

        CurrentMenu = menuData.menuBase;
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
        menus.RemoveAll(m => m == null || m.menuBase == null);

        foreach (var menu in menus)
        {
            if (menu?.menuBase != null)
            {
                menu.menuBase.Open();
                menu.menuBase.Close();
            }
        }
    }

    public void OpenOverlayMenu(MenuType menuType, object data = null)
    {
        menus.RemoveAll(m => m == null || m.menuBase == null);

        var menuData = menus.FirstOrDefault(m => m.menuType == menuType);
        if (menuData == null || menuData.menuBase == null)
        {
            LoadMenus();
            menuData = menus.FirstOrDefault(m => m.menuType == menuType);
            if (menuData == null || menuData.menuBase == null)
            {
                Debug.LogError("Overlay menu not found: " + menuType);
                return;
            }
        }

        PreviousMenuType = CurrentMenuType;
        CurrentMenu = menuData.menuBase;
        CurrentMenu.Open(data);
        CurrentMenu.transform.SetAsLastSibling();
        CurrentMenuType = menuType;
    }
}
