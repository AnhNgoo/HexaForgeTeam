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
    HUDMenuTest = 1,
    MainMenuTest = 2,
    SettingMenuTest = 3,
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
            menus.Add(new MenuData { menuType = menu.menuType, menuBase = menu });
        }
    }


    /// <summary>
    /// Mở menu mình muốn và đóng tất cả menu đang mở, nếu muốn mở thêm menu mà không đóng menu hiện tại thì chỉ cần gọi menu.Open() của menu đó mà không cần gọi hàm này
    /// </summary>
    /// <param name="menuType"></param>
    /// <param name="timeScale"></param>
    /// <param name="data"></param>
    public void ChangeMenu(MenuType menuType, int timeScale = 1, object data = null)
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

        Time.timeScale = timeScale;
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

    //Đảm bảo các UI được khởi mở hết để đăng ký các sự kiện, sau đó đóng lại
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
}
