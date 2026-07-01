using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private MenuType startingMenu = MenuType.GameplayMenu;
    private void Start()
    {
        UIManager.Instance.CloseAllMenus();
        UIManager.Instance.ChangeMenu(startingMenu);
    }
}