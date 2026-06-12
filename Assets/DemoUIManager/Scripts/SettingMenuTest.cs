// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;

// public class SettingMenuTest : MenuBase
// {
//     public override MenuType menuType => MenuType.SettingMenuTest;
//     [SerializeField] private Button btn_Back;
//     [SerializeField] private Slider musicVolumeSlider;

//     protected override void LoadComponent()
//     {
//         if (btn_Back == null)
//             btn_Back = transform.Find("SettingPanel/Btn_Back")?.GetComponent<Button>();
//         if (musicVolumeSlider == null)
//             musicVolumeSlider = transform.Find("SettingPanel/Music")?.GetComponent<Slider>();
//     }

//     protected override void LoadComponentRuntime()
//     {

//     }

//     public override void Open(object data = null)
//     {
//         base.Open(data);
//         btn_Back.onClick.AddListener(OnBtnBackClick);
//         musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
//     }


//     public override void Close()
//     {
//         base.Close();
//         btn_Back.onClick.RemoveListener(OnBtnBackClick);
//         musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
//     }

//     private void OnBtnBackClick()
//     {
//         UIManager.Instance.ChangeMenu(UIManager.Instance.PreviousMenuType);
//     }

//     private void OnMusicVolumeChanged(float arg0)
//     {
//         EventManager.Notify(GameEvent.OnMusicVolumeChangedTest, arg0);
//     }

// }