# DemoUIManager - Hướng dẫn sử dụng

Tài liệu này hướng dẫn cách setup và sử dụng UIManager để điều hướng menu, đóng mở menu, và truyền sự kiện giữa menu và các thành phần khác trong scene. Nội dung được viết theo kiểu kịch bản để có thể dùng làm audio lồng tiếng cho video.

## 1) Tổng quan hệ thống
- UIManager là trung tâm quản lý menu.
- Mỗi menu là một GameObject trong Canvas, gắn script kế thừa từ MenuBase.
- Khi cần thêm menu mới, tạo script mới (ví dụ: MainMenuTest) và gắn vào GameObject menu đó.
- Bên trong mỗi menu có thể có các panel, button, slider tùy ý.

## 2) Cách setup UIManager trong scene
Bước 1: Tạo GameObject UIManager
- Tạo một GameObject rỗng trong scene, đặt tên: UIManager.
- Gắn script UIManager vào GameObject đó.

Bước 2: Tạo Canvas
- Tạo một Canvas (UI -> Canvas) và đặt trong UIManager.
- Đặt tên Canvas đúng theo biến canvasPath trong UIManager (mặc định là "Canvas").

Bước 3: Tạo các menu con
- Trong Canvas, tạo các GameObject menu (ví dụ: MainMenuTest, SettingMenuTest).
- Gắn script MenuBase kế thừa (ví dụ MainMenuTest.cs, SettingMenuTest.cs) cho mỗi menu.
- Mỗi menu có thể chứa các panel, button, slider,... theo ý muốn.

Bước 4: Kiểm tra LoadMenus
- UIManager sẽ tự động tìm MenuBase trong Canvas và lưu vào danh sách menu.
- Chỉ cần đảm bảo menu là con trực tiếp hoặc con cháu của Canvas.

## 3) Chuyển menu và đóng tất cả menu
### Chuyển menu
Dùng UIManager.Instance.ChangeMenu để chuyển menu:
- Có thể truyền timescale (mặc định 1).
- Có thể truyền data (object) nếu muốn.

Ví dụ:
```csharp
UIManager.Instance.ChangeMenu(MenuType.MainMenuTest);
UIManager.Instance.ChangeMenu(MenuType.SettingMenuTest, timeScale: 1, data: null);
UIManager.Instance.ChangeMenu(UIManager.Instance.PreviousMenuType);
```

### Đóng tất cả menu
Dùng UIManager.Instance.CloseAllMenus();

Ví dụ:
```csharp
UIManager.Instance.CloseAllMenus();
```

## 4) Cách điều hướng menu (kịch bản hướng dẫn)
Bước 1: Khai báo button cần nhấn
- Tạo biến Button và gán trong Inspector hoặc tìm bằng transform.Find trong LoadComponent.

Bước 2: Đăng ký sự kiện trong Open và hủy đăng ký trong Close
- Trong Open: AddListener cho button.
- Trong Close: RemoveListener cho button.

Bước 3: Trong hàm xử lý, gọi ChangeMenu
- Khi user bấm button, gọi UIManager.Instance.ChangeMenu.

Ví dụ:
```csharp
public override void Open(object data = null)
{
    base.Open(data);
    btn_Setting.onClick.AddListener(OnBtnSettingClick);
}

public override void Close()
{
    base.Close();
    btn_Setting.onClick.RemoveListener(OnBtnSettingClick);
}

private void OnBtnSettingClick()
{
    UIManager.Instance.ChangeMenu(MenuType.SettingMenuTest);
}
```

## 5) Gọi hàm từ menu đến nơi khác
Dùng EventManager.Notify để gửi sự kiện từ menu ra nơi khác.

Ví dụ:
```csharp
private void OnTestEvent()
{
    EventManager.Notify(GameEvent.OnBtn_TestEventFromMenuToOther);
}
```

## 6) Gọi hàm từ menu đến nơi khác có truyền tham số data
Bạn có thể truyền dữ liệu theo event:

Ví dụ:
```csharp
private void OnMusicVolumeChanged(float value)
{
    EventManager.Notify(GameEvent.OnMusicVolumeChangedTest, value);
}
```

Nơi nhận sự kiện sẽ ép kiểu và xử lý:
```csharp
private void OnMusicVolumeChanged(object obj)
{
    if (obj is float volume)
    {
        Debug.Log($"Music Volume Changed: {volume}");
    }
}
```

## 7) Gọi hàm từ nơi khác đến menu
Nơi khác (ví dụ: DemoInitUI) có thể gửi sự kiện cho menu:

Ví dụ:
```csharp
EventManager.Notify(GameEvent.OnTestEventFromOtherToMenu);
```

Trong menu, đăng ký và nhận sự kiện:
```csharp
private void Start()
{
    EventManager.Subscribe(GameEvent.OnTestEventFromOtherToMenu, OnTestEventFromOtherToMenu);
}

private void OnDestroy()
{
    EventManager.Unsubscribe(GameEvent.OnTestEventFromOtherToMenu, OnTestEventFromOtherToMenu);
}

private void OnTestEventFromOtherToMenu(object obj)
{
    Debug.Log("Test Event From Other To Menu Triggered");
}
```

## 8) Lời khuyên khi làm video hướng dẫn
- Mở đầu: giải thích UIManager là trung tâm điều hướng menu.
- Thân: hướng dẫn setup trong scene và sử dụng ChangeMenu, CloseAllMenus.
- Kết: demo gọi sự kiện từ menu ra ngoài và từ ngoài vào menu, có cả truyền data.

Chúc bạn và đội nhóm triển khai suôn sẻ.
