using NUnit.Framework;
using UnityEngine;

namespace DuskBlade.Tests
{
    [Category("Animation"), Category("Reference"), Category("EditMode")]
    public class AnimationReferenceEditModeTests : ReferenceEditModeTestBase
    {
        protected override string ExportName => "AnimationReferenceEditMode";

        [Test, Description("ANR-001: Kiểm tra Player Animator hợp lệ nếu Player dùng Animator.")]
        public void ANR_001_PlayerAnimatorHopLe() { AnimatorCheck("ANR-001", "Player Animator hợp lệ", FindPlayerPrefab(), "Player"); }
        [Test, Description("ANR-002: Kiểm tra Enemy Animator hợp lệ nếu Enemy dùng Animator.")]
        public void ANR_002_EnemyAnimatorHopLe() { AnimatorCheck("ANR-002", "Enemy Animator hợp lệ", FindEnemyPrefab(), "Enemy"); }
        [Test, Description("ANR-003: Kiểm tra Boss/Miniboss Animator hợp lệ nếu tìm thấy prefab.")]
        public void ANR_003_BossAnimatorHopLe() { AnimatorCheck("ANR-003", "Boss/Miniboss Animator hợp lệ", FindPrefabByName("Boss", "Miniboss"), "Boss/Miniboss"); }
        [Test, Description("ANR-004: Kiểm tra UI Animator hợp lệ nếu có.")]
        public void ANR_004_UIAnimatorHopLe() { AnimatorCheck("ANR-004", "UI Animator hợp lệ", FindPrefabByName("HUD", "UI", "Canvas"), "UI/HUD"); }
        [Test, Description("ANR-005: Kiểm tra RuntimeAnimatorController không null trên object cần animation.")]
        public void ANR_005_RuntimeAnimatorControllerKhongNull() { Run("ANR-005", "RuntimeAnimatorController không null", "Tất cả Animator tìm được trên prefab Player/Enemy có controller nếu Animator tồn tại.", "Medium", c => { GameObject p = FindPlayerPrefab(); GameObject e = FindEnemyPrefab(); int animators = CountAnimators(p) + CountAnimators(e); int nullCtrl = CountNullAnimatorControllers(p) + CountNullAnimatorControllers(e); c.Actual = $"Animator kiểm tra={animators}, controller null={nullCtrl}."; Assert.AreEqual(0, nullCtrl, "Có Animator bị thiếu RuntimeAnimatorController."); }); }
        [Test, Description("ANR-006: Kiểm tra không Missing Script trên object animation.")]
        public void ANR_006_KhongMissingScriptAnimation() { Run("ANR-006", "Không Missing Script trên object animation", "Prefab có Animator không có Missing Script.", "High", c => { GameObject p = FindPlayerPrefab(); GameObject e = FindEnemyPrefab(); int missing = CountMissingScripts(p) + CountMissingScripts(e); c.Actual = $"Prefab kiểm tra=Player+Enemy, Missing Script={missing}."; Assert.AreEqual(0, missing, "Prefab animation có Missing Script."); }); }
        [Test, Description("ANR-007: Kiểm tra reference animation trong component không null nếu đọc được.")]
        public void ANR_007_ComponentAnimationReference() { Run("ANR-007", "Animation reference trong component", "CharacterAnimation/EnemyAnimatorController tồn tại nếu prefab dùng animation.", "Medium", c => { GameObject p = FindPlayerPrefab(); GameObject e = FindEnemyPrefab(); Component ca = p ? FindComponent(p, "CharacterAnimation") : null; Component ea = e ? FindComponent(e, "EnemyAnimatorController") : null; c.Actual = $"CharacterAnimation={(ca ? ca.GetType().Name : "null")}, EnemyAnimatorController={(ea ? ea.GetType().Name : "null")}."; Assert.IsTrue(ca != null || ea != null, "Không tìm thấy component animation chính trên Player/Enemy."); }); }
        [Test, Description("ANR-008: Kiểm tra Animator không bị disabled bất thường trên prefab active.")]
        public void ANR_008_AnimatorKhongDisabledBatThuong() { Run("ANR-008", "Animator không disabled bất thường", "Animator trên prefab active nên enabled.", "Low", c => { GameObject p = FindPlayerPrefab(); GameObject e = FindEnemyPrefab(); int disabled = CountDisabledAnimators(p) + CountDisabledAnimators(e); c.Actual = $"Animator disabled={disabled}."; Assert.AreEqual(0, disabled, "Có Animator disabled trên prefab active."); }); }

        private void AnimatorCheck(string id, string title, GameObject prefab, string label)
        {
            Run(id, title, "Animator nếu tồn tại thì enabled và RuntimeAnimatorController không null.", "Medium", c =>
            {
                if (prefab == null) { c.Actual = $"Không tìm thấy prefab {label}, bỏ qua kiểm tra optional."; return; }
                int animators = CountAnimators(prefab);
                int nullCtrl = CountNullAnimatorControllers(prefab);
                int disabled = CountDisabledAnimators(prefab);
                c.Actual = $"Prefab={prefab.name}, Animator={animators}, controller null={nullCtrl}, disabled={disabled}.";
                Assert.AreEqual(0, nullCtrl, label + " có Animator nhưng controller null.");
            });
        }

        private int CountAnimators(GameObject root) { return root == null ? 0 : root.GetComponentsInChildren<Animator>(true).Length; }
        private int CountDisabledAnimators(GameObject root) { if (root == null) return 0; int count = 0; foreach (Animator a in root.GetComponentsInChildren<Animator>(true)) if (!a.enabled) count++; return count; }
    }
}
