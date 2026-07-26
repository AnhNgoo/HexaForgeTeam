using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private MenuType startingMenu = MenuType.GameplayMenu;
    // protected override void Awake()
    // {
    //     base.Awake();
    //     UIManager.Instance.InitUI();
    //     UIManager.Instance.ChangeMenu(startingMenu);
    // }

    void Start()
    {
        UIManager.Instance.InitUI();
        UIManager.Instance.ChangeMenu(startingMenu);
    }
}