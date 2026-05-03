# Game Architecture Guide

Tài liệu này mô tả các thành phần cơ bản và cách dùng trong dự án. Mỗi phần gồm: công dụng, dùng làm gì, và các bước sử dụng rõ ràng.

---

## LoadComponents - Tự động gán component

**Công dụng**

- Tự động gán các component vào biến serialized, tránh kéo tay nhiều lần.
- Hoạt động cả ở Editor (OnValidate) và Runtime (Awake).

**Dùng làm gì**

- Tạo một quy trình load component thống nhất cho mỗi script.

**Cách sử dụng**

- Bước 1: Kế thừa `LoadComponents`.
- Bước 2: Override `LoadComponent()` để gán reference trong Editor.
- Bước 3: Override `LoadComponentRuntime()` để gán reference khi chạy game.
- Bước 4: Bấm nút "Load Components In Edit Mode" trên Inspector nếu cần.

**Ví dụ**

```csharp
using UnityEngine;

public class HealthBar : LoadComponents
{
	[SerializeField] private Transform fill;

	protected override void LoadComponent()
	{
		if (fill == null)
			fill = transform.Find("Fill");
	}

	protected override void LoadComponentRuntime()
	{
		if (fill == null)
			fill = transform.Find("Fill");
	}
}
```

---

## Singleton - Một instance duy nhất

**Công dụng**

- Đảm bảo chỉ có 1 instance cho các Manager.

**Dùng làm gì**

- Dùng cho `UIManager`, `EventManager`, `ObjectPooling`, ...

**Cách sử dụng**

- Bước 1: Kế thừa `Singleton<T>`.
- Bước 2: Sử dụng `ClassName.Instance` để truy cập.
- Bước 3 (tùy chọn): Bật `isDontDestroyOnLoad` nếu cần giữ qua scene.

**Ví dụ**

```csharp
public class AudioManager : Singleton<AudioManager>
{
	public void PlaySfx(string key) { }
}

// Gọi từ nơi khác
AudioManager.Instance.PlaySfx("Click");
```

---

## EventManager - Phát/nhận sự kiện

**Công dụng**

- Giao tiếp giữa các hệ thống qua sự kiện, không cần tham chiếu trực tiếp.

**Dùng làm gì**

- Subscribe / Unsubscribe / Notify sự kiện có dữ liệu đi kèm.

**Cách sử dụng**

- Bước 1: Thêm event vào `GameEvent` enum.
- Bước 2: Subscribe trong `OnEnable()`.
- Bước 3: Unsubscribe trong `OnDisable()`.
- Bước 4: Gọi `Notify()` khi cần phát sự kiện.

**Ví dụ**

```csharp
public enum GameEvent
{
	None = 0,
	StartGame = 1,
	PlayerDied = 2,
}

public class GameFlow : MonoBehaviour
{
	private void OnEnable()
	{
		EventManager.Instance.Subscribe(GameEvent.StartGame, OnStartGame);
	}

	private void OnDisable()
	{
		EventManager.Instance.Unsubscribe(GameEvent.StartGame, OnStartGame);
	}

	private void OnStartGame(object data)
	{
		// handle
	}

	private void Start()
	{
		EventManager.Instance.Notify(GameEvent.StartGame);
	}
}
```

---

## UIManager - Quản lý menu UI

**Công dụng**

- Quản lý mở / đóng menu theo `MenuType`.

**Dùng làm gì**

- `ChangeMenu()` để chuyển menu, `CloseAllMenus()` để đóng tất cả.

**Cách sử dụng**

- Bước 1: Đảm bảo có GameObject `Canvas` trong scene (đường dẫn mặc định: `Canvas`).
- Bước 2: Mỗi menu kế thừa `MenuBase` và đặt dưới Canvas.
- Bước 3: Thêm type vào `MenuType` enum.
- Bước 4: Gọi `UIManager.Instance.ChangeMenu(MenuType.YourMenu)` để mở.

**Ví dụ tạo menu mới: MainMenu**

- Bước 1: Tạo file `MainMenu.cs`.
- Bước 2: Kế thừa `MenuBase`.
- Bước 3: Thêm `MainMenu` vào `MenuType` enum.
- Bước 4: Đặt object `MainMenu` dưới Canvas.
- Bước 5: Gọi `ChangeMenu(MenuType.MainMenu)` để mở menu.

```csharp
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MenuBase
{
	public override MenuType menuType => MenuType.MainMenu;

	[SerializeField] private Button playButton;

	protected override void LoadComponent()
	{
		if (playButton == null)
			playButton = transform.Find("PlayButton")?.GetComponent<Button>();
	}

	protected override void LoadComponentRuntime()
	{
		if (playButton == null)
			playButton = transform.Find("PlayButton")?.GetComponent<Button>();
	}

	private void OnEnable()
	{
		if (playButton != null)
			playButton.onClick.AddListener(OnPlayClicked);
	}

	private void OnDisable()
	{
		if (playButton != null)
			playButton.onClick.RemoveListener(OnPlayClicked);
	}

	private void OnPlayClicked()
	{
		// Xử lý khi ấn Play
	}
}
```

**Mở menu từ nơi khác**

```csharp
UIManager.Instance.ChangeMenu(MenuType.MainMenu);
```

**MenuBase (nền tảng cho menu)**

- Base class cho tất cả menu, cung cấp `Open()` và `Close()`.
- `Open()` bật menu, `Close()` tắt menu.
- Cách dùng nhanh:
  - Bước 1: Kế thừa `MenuBase`.
  - Bước 2: Override `menuType` để trả về enum tương ứng.
  - Bước 3 (tùy chọn): Override `Open()` và `Close()` để thêm animation.

---

## ObjectPooling - Spawn/return theo pool

**Công dụng**

- Quản lý spawn và return object theo `PoolType`.

**Dùng làm gì**

- `SpawnFromPool()` tạo/lấy object từ pool.
- `ReturnToPool()` trả object về pool.

**Cách sử dụng**

- Bước 1: Tạo 1 enum mới (PoolType) để đại diện cho prefab
- Bước 2: Tạo 1 ScriptableObjects PoolData ngay trong thư mục "Resources/ScriptableObjects/PoolData" (Tạo thư mục nếu chưa có)
- Bước 3: Cấu hình PoolData vừa tạo, chọn đúng enum, gán prefab, parent có thể không cần gán
- Bước 4: Gọi ObjectPooling.Instance.SpawnFromPool để tạo object và ObjectPooling.Instance.ReturnToPool để destroy
- Bước 5: Từ bước này có thể bỏ qua nếu prefab không cần thiết phải implement `IPoolable` (Để gọi các hàm hỗ trợ khi spawn).
- Bước 6: Gọi `SpawnFromPool()` khi cần.
- Bước 7: Gọi `ReturnToPool()` khi không cần nữa.

**PoolData (cấu hình pool dạng asset)**

- `ObjectPooling` tự động load các `PoolData` trong Resources.
- Cách dùng nhanh:
  - Bước 1: Tạo `PoolData` asset qua menu `Create/ScriptableObjects/PoolData`.
  - Bước 2: Đặt asset vào `Assets/Resources/ScriptableObjects/PoolData`.
  - Bước 3: Trong asset, gán `PoolType`, `prefab`, `initialSize`, `maxSize`.

**IPoolable (hỗ trợ khi spawn/return)**

- `OnSpawnFromPool()` được gọi khi spawn.
- `OnReturnToPool()` được gọi khi return.
- Cách dùng nhanh:
  - Bước 1: Implement `IPoolable` trong prefab.
  - Bước 2: Trả về `PoolType` tương ứng.
  - Bước 3: Đặt logic reset trong `OnReturnToPool()`.

**Ví dụ**

```csharp
// Có thể không cần truyền tham số ObjectParent nếu không cần thiết
var obj = ObjectPooling.Instance.SpawnFromPool(
	PoolType.CannonExplosion,
	transform.position,
	Quaternion.identity,
	ObjectParent
);

// return khi xong
ObjectPooling.Instance.ReturnToPool(PoolType.CannonExplosion, obj);
```

```csharp
public class Bullet : MonoBehaviour, IPoolable
{
	public PoolType PoolType => PoolType.Muzzle;

	public void OnSpawnFromPool()
	{
		// reset trạng thái
	}

	public void OnReturnToPool()
	{
		// tắt effect, clear state
	}
}
```

---

## LookAtCamera - Luôn hướng về camera

**Công dụng**

- Làm cho object luôn hướng về camera.

**Dùng làm gì**

- Sử dụng cho UI world-space, billboard, indicator.

**Cách sử dụng**

- Bước 1: Gắn script `LookAtCamera` lên object.
- Bước 2: Đảm bảo camera chính có tag `MainCamera`.
- Bước 3: Object sẽ tự xoay theo camera mỗi frame.

**Lưu ý**

- Nếu thay đổi camera trong runtime, cần cập nhật `cam` (hoặc sửa script để re-assign).
