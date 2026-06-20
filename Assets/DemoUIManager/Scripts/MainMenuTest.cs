// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;

// public class MainMenuTest : MenuBase
// {
//     public override MenuType menuType => MenuType.MainMenuTest;
//     [SerializeField] private Button btn_Play;
//     [SerializeField] private Button btn_Setting;
//     [SerializeField] private Button btn_Quit;
//     [SerializeField] private Button btn_TestEvent;

//     protected override void LoadComponent()
//     {
//         if (btn_Play == null)
//             btn_Play = transform.Find("Btn_Play")?.GetComponent<Button>();
//         if (btn_Setting == null)
//             btn_Setting = transform.Find("Btn_Setting")?.GetComponent<Button>();
//         if (btn_Quit == null)
//             btn_Quit = transform.Find("Btn_Quit")?.GetComponent<Button>();
//         if (btn_TestEvent == null)
//             btn_TestEvent = transform.Find("Btn_TestEvent")?.GetComponent<Button>();
//     }

//     protected override void LoadComponentRuntime()
//     {

//     }

//     #region Đăng ký và huỷ button hay slider,... khuyên nên đặt trong Open và Close
//     public override void Open(object data = null)
//     {
//         base.Open(data);
//         btn_Play.onClick.AddListener(OnBtnPlayClick);
//         btn_Setting.onClick.AddListener(OnBtnSettingClick);
//         btn_Quit.onClick.AddListener(OnBtnQuitClick);
//         btn_TestEvent.onClick.AddListener(OnTestEvent);
//     }

//     public override void Close()
//     {
//         base.Close();
//         btn_Play.onClick.RemoveListener(OnBtnPlayClick);
//         btn_Setting.onClick.RemoveListener(OnBtnSettingClick);
//         btn_Quit.onClick.RemoveListener(OnBtnQuitClick);
//         btn_TestEvent.onClick.RemoveListener(OnTestEvent);
//     }

//     #endregion
//     #region Đăng ký và huỷ event khuyên nên đặt trong start và ondestroy
//     private void Start()
//     {
//         EventManager.Subscribe(GameEvent.OnTestEventFromOtherToMenu, OnTestEventFromOtherToMenu);
//     }

//     private void OnDestroy()
//     {
//         EventManager.Unsubscribe(GameEvent.OnTestEventFromOtherToMenu, OnTestEventFromOtherToMenu);
//     }
//     private void OnBtnSettingClick()
//     {
//         UIManager.Instance.ChangeMenu(MenuType.SettingMenuTest);
//     }

//     #endregion
//     private void OnBtnPlayClick()
//     {
//         Debug.Log("Play");
//     }

//     private void OnBtnQuitClick()
//     {
//         Application.Quit();
//     }


//     // *NOTE - Gọi event từ menu ra nơi khác
//     private void OnTestEvent()
//     {
//         EventManager.Notify(GameEvent.OnBtn_TestEventFromMenuToOther);
//     }

//     //NOTE - Nhận event từ nơi khác vào menu
//     private void OnTestEventFromOtherToMenu(object obj)
//     {
//         Debug.Log("Test Event From Other To Menu Triggered");
//     }
// }
