using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace DuskBlade.Tests
{
    public class UISystemTests : RuntimeSystemTestBase
    {
        protected override string ExportName => "UI";

        [UnityTest, Category("UI"), Category("Tự động"), Description("UI-001: Kiểm tra tìm được UI/HUD prefab thật.")]
        public IEnumerator UI_001_TimDuocUIPrefabThat() { return RunUnity("UI-001", "Tìm được UI/HUD prefab thật", "Tìm được prefab UI/HUD thật trong project.", "High", c => UiPrefab(c)); }
        [UnityTest, Category("UI"), Category("Tự động"), Description("UI-002: Kiểm tra UI prefab instantiate được.")]
        public IEnumerator UI_002_UIPrefabInstantiateDuoc() { return RunUnity("UI-002", "UI prefab instantiate được", "UI prefab thật instantiate được không lỗi đỏ.", "High", c => UiInstantiate(c)); }
        [UnityTest, Category("UI"), Category("Tự động"), Description("UI-003: Kiểm tra UI không có Missing Script.")]
        public IEnumerator UI_003_UIKhongMissingScript() { return RunUnity("UI-003", "UI không Missing Script", "UI prefab và object con không có Missing Script.", "High", c => Missing(c)); }
        [UnityTest, Category("UI"), Category("Tự động"), Description("UI-004: Kiểm tra HUDMenuTest hoặc menu HUD thật tồn tại.")]
        public IEnumerator UI_004_HUDMenuThatTonTai() { return RunUnity("UI-004", "HUD menu thật tồn tại", "UI có HUDMenuTest/MenuBase hoặc object HUD thật.", "Medium", c => Hud(c)); }
        [UnityTest, Category("UI"), Category("Tự động"), Description("UI-005: Kiểm tra Joystick tồn tại trên HUD.")]
        public IEnumerator UI_005_JoystickTonTai() { return RunUnity("UI-005", "Joystick tồn tại trên HUD", "HUD có Joystick thật để điều khiển Player.", "High", c => Joystick(c)); }
        [UnityTest, Category("UI"), Category("Tự động"), Description("UI-006: Kiểm tra các nút thao tác chính trên HUD.")]
        public IEnumerator UI_006_NutThaoTacChinhTonTai() { return RunUnity("UI-006", "Nút thao tác chính tồn tại", "HUD có Dodge/Jump/Attack/Lock/Skill.", "High", c => Buttons(c)); }
        [UnityTest, Category("UI"), Category("Tự động"), Description("UI-007: Kiểm tra nút HUD dùng EventTouch thật nếu project dùng.")]
        public IEnumerator UI_007_ButtonDungEventTouch() { return RunUnity("UI-007", "Button HUD dùng EventTouch", "Các nút HUD có EventTouch hoặc component tương tác thật.", "Medium", c => EventTouch(c)); }
        [UnityTest, Category("UI"), Category("Tự động"), Description("UI-008: Kiểm tra UIManager thật khởi tạo được.")]
        public IEnumerator UI_008_UIManagerKhoiTaoDuoc() { return RunUnity("UI-008", "UIManager thật khởi tạo được", "UIManager thật khởi tạo trong PlayMode không lỗi đỏ.", "Medium", c => UiManager(c)); }
        [UnityTest, Category("UI"), Category("Tự động"), Description("UI-009: Kiểm tra mở/đóng HUD không lỗi đỏ nếu gọi được method thật.")]
        public IEnumerator UI_009_MoDongHUDKhongLoi() { return RunUnity("UI-009", "Mở/đóng HUD không lỗi đỏ", "Gọi Open/Close trên HUD thật không lỗi đỏ.", "Medium", c => OpenClose(c)); }
        [UnityTest, Category("UI"), Category("Tự động"), Description("UI-010: Kiểm tra UI chạy 60 frame không Error/Exception.")]
        public IEnumerator UI_010_UIChay60FrameKhongLoi() { return RunUnity("UI-010", "UI chạy 60 frame không lỗi đỏ", "UI prefab thật chạy 60 frame không lỗi đỏ.", "High", c => Sixty(c)); }

        private IEnumerator UiPrefab(Ctx c) { GameObject p = FindUiPrefab(); c.Actual = p ? "UI prefab=" + p.name + "." : "Không tìm thấy UI/HUD prefab thật."; Assert.IsNotNull(p); yield break; }
        private IEnumerator UiInstantiate(Ctx c) { StartWatcher(); GameObject prefab = FindUiPrefab(); GameObject ui = InstantiatePrefab(prefab, Vector3.zero, "_RuntimeTest"); yield return null; c.Actual = $"UI prefab={prefab.name}, active={ui.activeInHierarchy}, Error/Exception={ErrorCount()}."; AssertNoErrors("Instantiate UI không được lỗi đỏ."); }
        private IEnumerator Missing(Ctx c) { GameObject ui = InstantiatePrefab(FindUiPrefab(), Vector3.zero, "_RuntimeTest"); yield return null; int missing = CountMissing(ui); c.Actual = $"UI={ui.name}, Missing Script={missing}."; Assert.AreEqual(0, missing); }
        private IEnumerator Hud(Ctx c) { GameObject ui = InstantiatePrefab(FindUiPrefab(), Vector3.zero, "_RuntimeTest"); yield return null; Component hud = TestReflectionHelper.FindComponentByClassName(ui, "HUDMenuTest"); int names = CountNamed(ui, "HUD", "Menu", "Canvas"); c.Actual = $"HUDMenuTest={(hud != null)}, tên HUD/Menu/Canvas={names}."; Assert.IsTrue(hud != null || names > 0); }
        private IEnumerator Joystick(Ctx c) { GameObject ui = InstantiatePrefab(FindUiPrefab(), Vector3.zero, "_RuntimeTest"); yield return null; int comp = CountComponents(ui, "Joystick", "FixedJoystick", "FloatingJoystick", "DynamicJoystick", "VariableJoystick"); int names = CountNamed(ui, "Joystick"); c.Actual = $"Joystick component={comp}, object tên Joystick={names}."; Assert.IsTrue(comp > 0 || names > 0); }
        private IEnumerator Buttons(Ctx c) { GameObject ui = InstantiatePrefab(FindUiPrefab(), Vector3.zero, "_RuntimeTest"); yield return null; int found = CountNamed(ui, "Btn_Dodge", "Btn_Jump", "Btn_Attack", "Btn_LockTarget", "Btn_Skill_1", "Btn_Skill_2", "Attack", "Skill"); c.Actual = $"Nút thao tác khớp tên={found}."; Assert.GreaterOrEqual(found, 4); }
        private IEnumerator EventTouch(Ctx c) { GameObject ui = InstantiatePrefab(FindUiPrefab(), Vector3.zero, "_RuntimeTest"); yield return null; int touches = CountComponents(ui, "EventTouch"); c.Actual = $"EventTouch={touches}."; Assert.GreaterOrEqual(touches, 1); }
        private IEnumerator UiManager(Ctx c) { StartWatcher(); Component manager = CreateRealComponent("UIManager", "Test_UIManager"); yield return null; c.Actual = $"Component={manager.GetType().Name}, Error/Exception={ErrorCount()}."; AssertNoErrors("UIManager khởi tạo không được lỗi đỏ."); }
        private IEnumerator OpenClose(Ctx c) { StartWatcher(); GameObject ui = InstantiatePrefab(FindUiPrefab(), Vector3.zero, "_RuntimeTest"); yield return null; Component hud = TestReflectionHelper.FindComponentByClassName(ui, "HUDMenuTest"); Assert.IsNotNull(hud, "Không tìm thấy HUDMenuTest thật."); bool open = TryInvoke(hud, "Open", (object)null); yield return null; bool close = TryInvoke(hud, "Close"); c.Actual = $"HUD={hud.GetType().Name}, Open={open}, Close={close}, Error/Exception={ErrorCount()}."; Assert.IsTrue(open && close); AssertNoErrors("Open/Close HUD không được lỗi đỏ."); }
        private IEnumerator Sixty(Ctx c) { StartWatcher(); GameObject ui = InstantiatePrefab(FindUiPrefab(), Vector3.zero, "_RuntimeTest"); for (int i = 0; i < 60; i++) yield return null; c.Actual = $"UI={ui.name}, frame=60, Error/Exception={ErrorCount()}."; AssertNoErrors("UI chạy 60 frame không được lỗi đỏ."); }

        private GameObject FindUiPrefab() { return TestPrefabFinder.FindPrefabWithComponent("HUDMenuTest") ?? TestPrefabFinder.FindPrefabWithComponent("UIManager") ?? TestPrefabFinder.FindHudOrUiPrefab(); }
        private int CountMissing(GameObject root) { int missing = 0; foreach (Transform child in root.GetComponentsInChildren<Transform>(true)) foreach (Component c in child.GetComponents<Component>()) if (c == null) missing++; return missing; }
        private int CountNamed(GameObject root, params string[] tokens) { int count = 0; foreach (Transform t in root.GetComponentsInChildren<Transform>(true)) foreach (string token in tokens) if (t.name.IndexOf(token, System.StringComparison.OrdinalIgnoreCase) >= 0) { count++; break; } return count; }
        private int CountComponents(GameObject root, params string[] names) { int count = 0; foreach (Component c in root.GetComponentsInChildren<Component>(true)) { if (c == null) continue; foreach (string name in names) if (c.GetType().Name == name) { count++; break; } } return count; }
    }
}
