using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    private void Start()
    {
        UIManager.Instance.CloseAllMenus();
        UIManager.Instance.ChangeMenu(MenuType.TitleMenu);
    }
}