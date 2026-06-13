using NUnit.Framework;
using UnityEngine;

namespace DuskBlade.Tests
{
    [Category("Player"), Category("Reference"), Category("EditMode")]
    public class PlayerReferenceEditModeTests : ReferenceEditModeTestBase
    {
        protected override string ExportName => "PlayerReferenceEditMode";

        [Test, Description("PR-001: Kiểm tra Player prefab tồn tại trong project.")]
        public void PR_001_PlayerPrefabTonTai() { Run("PR-001", "Player prefab tồn tại", "Tìm thấy Player prefab thật trong project.", "High", c => { GameObject p = FindPlayerPrefab(); c.Actual = p ? "Prefab=" + p.name : "Không tìm thấy Player prefab."; AssertPrefab(p, "Không tìm thấy Player prefab thật trong project."); }); }
        [Test, Description("PR-002: Kiểm tra Player prefab không Missing Script.")]
        public void PR_002_PlayerKhongMissingScript() { Run("PR-002", "Player không Missing Script", "Player prefab không có component Missing Script.", "High", c => { GameObject p = RequirePlayer(); int missing = CountMissingScripts(p); c.Actual = $"Prefab={p.name}, GameObject={CountChildren(p)}, Missing Script={missing}."; Assert.AreEqual(0, missing, "Player prefab có Missing Script."); }); }
        [Test, Description("PR-003: Kiểm tra Player có CharacterBase.")]
        public void PR_003_CoCharacterBase() { Required("PR-003", "Player có CharacterBase", "CharacterBase", "Player prefab có CharacterBase.", "High"); }
        [Test, Description("PR-004: Kiểm tra Player có CharacterMovement.")]
        public void PR_004_CoCharacterMovement() { Required("PR-004", "Player có CharacterMovement", "CharacterMovement", "Player prefab có CharacterMovement.", "High"); }
        [Test, Description("PR-005: Kiểm tra Player có CharacterCombat.")]
        public void PR_005_CoCharacterCombat() { Required("PR-005", "Player có CharacterCombat", "CharacterCombat", "Player prefab có CharacterCombat.", "High"); }
        [Test, Description("PR-006: Kiểm tra Player có CharacterSkill.")]
        public void PR_006_CoCharacterSkill() { Required("PR-006", "Player có CharacterSkill", "CharacterSkill", "Player prefab có CharacterSkill.", "High"); }
        [Test, Description("PR-007: Kiểm tra Player có CharacterLockTarget.")]
        public void PR_007_CoCharacterLockTarget() { Required("PR-007", "Player có CharacterLockTarget", "CharacterLockTarget", "Player prefab có CharacterLockTarget.", "Medium"); }
        [Test, Description("PR-008: Kiểm tra Player có Collider hoặc CharacterController.")]
        public void PR_008_CoColliderHoacCharacterController() { Run("PR-008", "Player có Collider hoặc CharacterController", "Player có collider hoặc CharacterController hợp lệ.", "High", c => { GameObject p = RequirePlayer(); int colliders = p.GetComponentsInChildren<Collider>(true).Length; CharacterController cc = p.GetComponentInChildren<CharacterController>(true); c.Actual = $"Collider={colliders}, CharacterController={(cc ? "có" : "không")}."; Assert.IsTrue(colliders > 0 || cc != null, "Player không có Collider hoặc CharacterController."); }); }
        [Test, Description("PR-009: Kiểm tra Player có Renderer enabled.")]
        public void PR_009_CoRenderer() { Run("PR-009", "Player có Renderer", "Player có Renderer enabled để người chơi nhìn thấy nhân vật.", "Medium", c => { GameObject p = RequirePlayer(); int renderers = CountEnabledRenderers(p); c.Actual = $"Renderer enabled={renderers}."; Assert.Greater(renderers, 0, "Player không có Renderer enabled."); }); }
        [Test, Description("PR-010: Kiểm tra material Player không null.")]
        public void PR_010_MaterialKhongNull() { Run("PR-010", "Player material không null", "Renderer của Player không có material null.", "Medium", c => { GameObject p = RequirePlayer(); int nullMat = CountNullMaterials(p); c.Actual = $"Material null={nullMat}."; Assert.AreEqual(0, nullMat, "Player có material null."); }); }
        [Test, Description("PR-011: Kiểm tra Animator Player hợp lệ nếu có.")]
        public void PR_011_AnimatorHopLe() { Run("PR-011", "Player Animator hợp lệ", "Animator nếu có thì RuntimeAnimatorController không null.", "Medium", c => { GameObject p = RequirePlayer(); int animators = p.GetComponentsInChildren<Animator>(true).Length; int nullCtrl = CountNullAnimatorControllers(p); c.Actual = $"Animator={animators}, controller null={nullCtrl}."; Assert.AreEqual(0, nullCtrl, "Player có Animator nhưng controller null."); }); }
        [Test, Description("PR-012: Kiểm tra AudioSource Player hợp lệ nếu có.")]
        public void PR_012_AudioHopLe() { Run("PR-012", "Player Audio reference hợp lệ", "AudioSource playOnAwake nếu có thì clip không null.", "Low", c => { GameObject p = RequirePlayer(); int sources = p.GetComponentsInChildren<AudioSource>(true).Length; int nullClip = CountNullAudioClips(p); c.Actual = $"AudioSource={sources}, clip null khi playOnAwake={nullClip}."; Assert.AreEqual(0, nullClip, "Player AudioSource playOnAwake có clip null."); }); }
        [Test, Description("PR-013: Kiểm tra Player có CharacterInput hoặc input component thật.")]
        public void PR_013_InputComponentTonTai() { Optional("PR-013", "Player có input component thật", "CharacterInput", "CharacterInput được ghi nhận nếu Player dùng input qua event/joystick.", "Medium"); }
        [Test, Description("PR-014: Kiểm tra Player có component animation chính nếu project dùng.")]
        public void PR_014_AnimationComponentTonTai() { Optional("PR-014", "Player có component animation chính", "CharacterAnimation", "CharacterAnimation được ghi nhận nếu prefab dùng animation.", "Low"); }
        [Test, Description("PR-015: Kiểm tra Player không thiếu reference dữ liệu chính nếu đọc được.")]
        public void PR_015_DataReferenceHopLe() { Run("PR-015", "Player data/reference chính hợp lệ", "CharacterBase và các component chính đọc được trên prefab.", "Medium", c => { GameObject p = RequirePlayer(); int core = CountCore(p); c.Actual = $"Core component tìm thấy={core}/5."; Assert.GreaterOrEqual(core, 4, "Player thiếu nhiều component chính."); }); }

        private GameObject RequirePlayer() { GameObject p = FindPlayerPrefab(); AssertPrefab(p, "Không tìm thấy Player prefab thật."); return p; }
        private void Required(string id, string title, string component, string expected, string severity) { Run(id, title, expected, severity, c => { GameObject p = RequirePlayer(); Component found = FindComponent(p, component); c.Actual = found ? "Tìm thấy " + found.GetType().Name : "Không tìm thấy " + component; Assert.IsNotNull(found, "Player thiếu component " + component + "."); }); }
        private void Optional(string id, string title, string component, string expected, string severity) { Run(id, title, expected, severity, c => { GameObject p = RequirePlayer(); Component found = FindComponent(p, component); c.Actual = found ? "Tìm thấy " + found.GetType().Name : "Không tìm thấy " + component + ", ghi nhận nếu project không dùng."; }); }
        private int CountCore(GameObject p) { int count = 0; string[] names = { "CharacterBase", "CharacterMovement", "CharacterCombat", "CharacterSkill", "CharacterLockTarget" }; foreach (string name in names) if (FindComponent(p, name) != null) count++; return count; }
    }
}
