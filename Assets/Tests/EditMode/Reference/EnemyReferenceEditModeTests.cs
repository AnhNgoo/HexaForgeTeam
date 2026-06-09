using NUnit.Framework;
using UnityEngine;

namespace DuskBlade.Tests
{
    [Category("Enemy"), Category("Reference"), Category("EditMode")]
    public class EnemyReferenceEditModeTests : ReferenceEditModeTestBase
    {
        protected override string ExportName => "EnemyReferenceEditMode";

        [Test, Description("ER-001: Kiểm tra Enemy prefab tồn tại trong project.")]
        public void ER_001_EnemyPrefabTonTai() { Run("ER-001", "Enemy prefab tồn tại", "Tìm thấy Enemy prefab thật trong project.", "High", c => { GameObject e = FindEnemyPrefab(); c.Actual = e ? "Prefab=" + e.name : "Không tìm thấy Enemy prefab."; AssertPrefab(e, "Không tìm thấy Enemy prefab thật trong project."); }); }
        [Test, Description("ER-002: Kiểm tra Enemy prefab không Missing Script.")]
        public void ER_002_EnemyKhongMissingScript() { Run("ER-002", "Enemy không Missing Script", "Enemy prefab không có component Missing Script.", "High", c => { GameObject e = RequireEnemy(); int missing = CountMissingScripts(e); c.Actual = $"Prefab={e.name}, GameObject={CountChildren(e)}, Missing Script={missing}."; Assert.AreEqual(0, missing, "Enemy prefab có Missing Script."); }); }
        [Test, Description("ER-003: Kiểm tra Enemy có EnemyBase.")]
        public void ER_003_CoEnemyBase() { Required("ER-003", "Enemy có EnemyBase", "EnemyBase", "Enemy có EnemyBase.", "High"); }
        [Test, Description("ER-004: Kiểm tra Enemy có EnemyHealth.")]
        public void ER_004_CoEnemyHealth() { Required("ER-004", "Enemy có EnemyHealth", "EnemyHealth", "Enemy có EnemyHealth.", "High"); }
        [Test, Description("ER-005: Kiểm tra Enemy có EnemyDamageReceiver.")]
        public void ER_005_CoEnemyDamageReceiver() { Required("ER-005", "Enemy có EnemyDamageReceiver", "EnemyDamageReceiver", "Enemy có EnemyDamageReceiver.", "High"); }
        [Test, Description("ER-006: Kiểm tra Enemy có EnemyDetection.")]
        public void ER_006_CoEnemyDetection() { Required("ER-006", "Enemy có EnemyDetection", "EnemyDetection", "Enemy có EnemyDetection.", "High"); }
        [Test, Description("ER-007: Kiểm tra Enemy có EnemyLocomotion.")]
        public void ER_007_CoEnemyLocomotion() { Required("ER-007", "Enemy có EnemyLocomotion", "EnemyLocomotion", "Enemy có EnemyLocomotion.", "High"); }
        [Test, Description("ER-008: Kiểm tra Enemy có EnemyCombat.")]
        public void ER_008_CoEnemyCombat() { Required("ER-008", "Enemy có EnemyCombat", "EnemyCombat", "Enemy có EnemyCombat.", "High"); }
        [Test, Description("ER-009: Kiểm tra Enemy có EnemyHitbox nếu project dùng.")]
        public void ER_009_CoEnemyHitbox() { Optional("ER-009", "Enemy có EnemyHitbox nếu dùng", "EnemyHitbox", "EnemyHitbox được ghi nhận nếu prefab dùng hitbox.", "Medium"); }
        [Test, Description("ER-010: Kiểm tra Enemy có EnemyStateMachine nếu project dùng.")]
        public void ER_010_CoEnemyStateMachine() { Optional("ER-010", "Enemy có EnemyStateMachine nếu dùng", "EnemyStateMachine", "EnemyStateMachine được ghi nhận nếu prefab dùng state.", "Medium"); }
        [Test, Description("ER-011: Kiểm tra Enemy có Collider hoặc CharacterController.")]
        public void ER_011_ColliderHopLe() { Run("ER-011", "Enemy có Collider hoặc CharacterController", "Enemy có component va chạm hợp lệ.", "High", c => { GameObject e = RequireEnemy(); int colliders = e.GetComponentsInChildren<Collider>(true).Length; CharacterController cc = e.GetComponentInChildren<CharacterController>(true); c.Actual = $"Collider={colliders}, CharacterController={(cc ? "có" : "không")}."; Assert.IsTrue(colliders > 0 || cc != null, "Enemy không có Collider hoặc CharacterController."); }); }
        [Test, Description("ER-012: Kiểm tra Enemy Renderer/material hợp lệ.")]
        public void ER_012_RendererMaterialHopLe() { Run("ER-012", "Enemy Renderer/material hợp lệ", "Enemy có Renderer enabled và material không null.", "Medium", c => { GameObject e = RequireEnemy(); int enabled = CountEnabledRenderers(e); int nullMat = CountNullMaterials(e); c.Actual = $"Renderer enabled={enabled}, material null={nullMat}."; Assert.Greater(enabled, 0, "Enemy không có Renderer enabled."); Assert.AreEqual(0, nullMat, "Enemy có material null."); }); }
        [Test, Description("ER-013: Kiểm tra Enemy Animator/RuntimeAnimatorController hợp lệ nếu có.")]
        public void ER_013_AnimatorHopLe() { Run("ER-013", "Enemy Animator hợp lệ", "Animator nếu có thì RuntimeAnimatorController không null.", "Medium", c => { GameObject e = RequireEnemy(); int animators = e.GetComponentsInChildren<Animator>(true).Length; int nullCtrl = CountNullAnimatorControllers(e); c.Actual = $"Animator={animators}, controller null={nullCtrl}."; Assert.AreEqual(0, nullCtrl, "Enemy có Animator nhưng controller null."); }); }
        [Test, Description("ER-014: Kiểm tra Enemy AudioSource/AudioClip reference hợp lệ nếu có.")]
        public void ER_014_AudioHopLe() { Run("ER-014", "Enemy Audio reference hợp lệ", "AudioSource playOnAwake nếu có thì clip không null.", "Low", c => { GameObject e = RequireEnemy(); int sources = e.GetComponentsInChildren<AudioSource>(true).Length; int nullClip = CountNullAudioClips(e); c.Actual = $"AudioSource={sources}, clip null khi playOnAwake={nullClip}."; Assert.AreEqual(0, nullClip, "Enemy AudioSource playOnAwake có clip null."); }); }
        [Test, Description("ER-015: Kiểm tra EnemyData hoặc data reference không null nếu project dùng.")]
        public void ER_015_EnemyDataKhongNull() { Run("ER-015", "EnemyData không null", "EnemyBase.Data không null nếu project dùng EnemyData.", "High", c => { GameObject e = RequireEnemy(); Component baseComp = FindComponent(e, "EnemyBase"); object data; bool ok = baseComp != null && TestReflectionHelper.TryGetValue(baseComp, "Data", out data) && data != null; c.Actual = $"EnemyBase={(baseComp ? baseComp.GetType().Name : "null")}, EnemyData={(ok ? data.ToString() : "null/không đọc được")}."; Assert.IsTrue(ok, "EnemyData null hoặc không đọc được."); }); }

        private GameObject RequireEnemy() { GameObject e = FindEnemyPrefab(); AssertPrefab(e, "Không tìm thấy Enemy prefab thật."); return e; }
        private void Required(string id, string title, string component, string expected, string severity) { Run(id, title, expected, severity, c => { GameObject e = RequireEnemy(); Component found = FindComponent(e, component); c.Actual = found ? "Tìm thấy " + found.GetType().Name : "Không tìm thấy " + component; Assert.IsNotNull(found, "Enemy thiếu component " + component + "."); }); }
        private void Optional(string id, string title, string component, string expected, string severity) { Run(id, title, expected, severity, c => { GameObject e = RequireEnemy(); Component found = FindComponent(e, component); c.Actual = found ? "Tìm thấy " + found.GetType().Name : "Không tìm thấy " + component + ", ghi nhận nếu project không dùng."; }); }
    }
}
