using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace DuskBlade.Tests
{
    [Category("UI"), Category("Reference"), Category("EditMode")]
    public class UIReferenceEditModeTests : ReferenceEditModeTestBase
    {
        protected override string ExportName => "UIReferenceEditMode";

        [Test, Description("UIR-001: Kiểm tra UI/HUD prefab hoặc Canvas tồn tại.")]
        public void UIR_001_UIPrefabTonTai() { Run("UIR-001", "UI/HUD prefab hoặc Canvas tồn tại", "Tìm thấy prefab UI/HUD/Canvas thật nếu project có UI prefab.", "Medium", c => { GameObject ui = FindUiPrefab(); c.Actual = ui ? "UI prefab=" + ui.name : "Không tìm thấy UI/HUD prefab."; Assert.IsNotNull(ui, "Không tìm thấy UI/HUD/Canvas prefab thật."); }); }
        [Test, Description("UIR-002: Kiểm tra UI không Missing Script.")]
        public void UIR_002_UIKhongMissingScript() { Run("UIR-002", "UI không Missing Script", "UI/HUD prefab không Missing Script.", "High", c => { GameObject ui = RequireUi(); int missing = CountMissingScripts(ui); c.Actual = $"UI={ui.name}, GameObject={CountChildren(ui)}, Missing Script={missing}."; Assert.AreEqual(0, missing, "UI prefab có Missing Script."); }); }
        [Test, Description("UIR-003: Kiểm tra Canvas hoặc root UI active.")]
        public void UIR_003_CanvasHoacRootActive() { Run("UIR-003", "Canvas hoặc root UI active", "UI root hoặc Canvas active trong prefab.", "Medium", c => { GameObject ui = RequireUi(); Canvas canvas = ui.GetComponentInChildren<Canvas>(true); c.Actual = $"UI={ui.name}, root active={ui.activeSelf}, Canvas={(canvas ? canvas.name : "null")}."; Assert.IsTrue(ui.activeSelf || canvas != null, "UI root inactive và không có Canvas."); }); }
        [Test, Description("UIR-004: Kiểm tra các Button chính tồn tại nếu project có.")]
        public void UIR_004_ButtonChinhTonTai() { Run("UIR-004", "Button chính tồn tại nếu project có", "Ghi nhận Button Attack/Jump/Dodge/Skill/Lock nếu có.", "Low", c => { GameObject ui = RequireUi(); int found = CountNamedChildren(ui, "Attack", "Jump", "Dodge", "Skill", "Lock", "Btn_"); c.Actual = $"UI={ui.name}, Button/action name khớp={found}."; }); }
        [Test, Description("UIR-005: Kiểm tra Text/TMP_Text hiển thị HP nếu project có.")]
        public void UIR_005_TextHP() { TextToken("UIR-005", "Text HP nếu có", "HP", "Text/TMP_Text HP được ghi nhận nếu UI có."); }
        [Test, Description("UIR-006: Kiểm tra Text/TMP_Text hiển thị Gold nếu project có.")]
        public void UIR_006_TextGold() { TextToken("UIR-006", "Text Gold nếu có", "Gold", "Text/TMP_Text Gold được ghi nhận nếu UI có."); }
        [Test, Description("UIR-007: Kiểm tra Text/TMP_Text hiển thị Level nếu project có.")]
        public void UIR_007_TextLevel() { TextToken("UIR-007", "Text Level nếu có", "Level", "Text/TMP_Text Level được ghi nhận nếu UI có."); }
        [Test, Description("UIR-008: Kiểm tra Button có onClick listener nếu project đã gán sẵn.")]
        public void UIR_008_ButtonOnClickListener() { Run("UIR-008", "Button có onClick listener", "Button nếu có thì số listener được ghi nhận.", "Low", c => { GameObject ui = RequireUi(); Button[] buttons = ui.GetComponentsInChildren<Button>(true); int listeners = 0; foreach (Button button in buttons) listeners += button.onClick.GetPersistentEventCount(); c.Actual = $"Button={buttons.Length}, persistent onClick listener={listeners}."; }); }
        [Test, Description("UIR-009: Kiểm tra UI Animator reference hợp lệ nếu có.")]
        public void UIR_009_UIAnimatorHopLe() { Run("UIR-009", "UI Animator reference hợp lệ", "Animator UI nếu có thì controller không null.", "Low", c => { GameObject ui = RequireUi(); int animators = ui.GetComponentsInChildren<Animator>(true).Length; int nullCtrl = CountNullAnimatorControllers(ui); c.Actual = $"Animator={animators}, controller null={nullCtrl}."; Assert.AreEqual(0, nullCtrl, "UI có Animator nhưng controller null."); }); }
        [Test, Description("UIR-010: Kiểm tra UI AudioSource/AudioClip reference hợp lệ nếu có.")]
        public void UIR_010_UIAudioHopLe() { Run("UIR-010", "UI AudioSource/AudioClip hợp lệ", "AudioSource playOnAwake nếu có thì clip không null.", "Low", c => { GameObject ui = RequireUi(); int sources = ui.GetComponentsInChildren<AudioSource>(true).Length; int nullClip = CountNullAudioClips(ui); c.Actual = $"AudioSource={sources}, clip null khi playOnAwake={nullClip}."; Assert.AreEqual(0, nullClip, "UI AudioSource playOnAwake có clip null."); }); }

        private GameObject FindUiPrefab() { return TestPrefabFinder.FindPrefabWithComponent("HUDMenuTest") ?? TestPrefabFinder.FindHudOrUiPrefab() ?? FindPrefabByName("HUD", "UI", "Canvas", "Menu"); }
        private GameObject RequireUi() { GameObject ui = FindUiPrefab(); Assert.IsNotNull(ui, "Không tìm thấy UI/HUD prefab."); return ui; }
        private void TextToken(string id, string title, string token, string expected) { Run(id, title, expected, "Low", c => { GameObject ui = RequireUi(); int found = CountNamedChildren(ui, token); c.Actual = $"UI={ui.name}, object name chứa {token}={found}."; }); }
        private int CountNamedChildren(GameObject root, params string[] tokens) { int count = 0; foreach (Transform t in root.GetComponentsInChildren<Transform>(true)) foreach (string token in tokens) if (t.name.IndexOf(token, System.StringComparison.OrdinalIgnoreCase) >= 0) { count++; break; } return count; }
    }
}
