using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DuskBlade.Tests
{
    [Category("Scene"), Category("Reference"), Category("EditMode")]
    public class SceneReferenceEditModeTests : ReferenceEditModeTestBase
    {
        protected override string ExportName => "SceneReferenceEditMode";

        [Test, Description("SCR-001: Kiểm tra project có scene gameplay/map thật.")]
        public void SCR_001_CoSceneGameplay() { Run("SCR-001", "Có scene gameplay/map thật", "Tìm thấy ít nhất một scene trong Assets.", "High", c => { string[] scenes = FindScenePaths(); c.Actual = $"Scene tìm thấy={scenes.Length}."; Assert.Greater(scenes.Length, 0, "Không tìm thấy scene thật trong Assets."); }); }
        [Test, Description("SCR-002: Kiểm tra có scene được bật trong Build Settings.")]
        public void SCR_002_SceneBuildSettings() { Run("SCR-002", "Scene được bật trong Build Settings", "Có ít nhất một scene enabled trong Build Settings để GameCI/PlayMode load được.", "High", c => { int enabled = 0; foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes) if (scene.enabled) enabled++; c.Actual = $"Scene enabled trong Build Settings={enabled}."; Assert.Greater(enabled, 0, "Không có scene enabled trong Build Settings."); }); }
        [Test, Description("SCR-003: Kiểm tra scene gameplay mở được ở EditMode.")]
        public void SCR_003_OpenSceneDuoc() { Run("SCR-003", "Scene gameplay mở được", "Scene thật mở được bằng EditorSceneManager.", "High", c => { Scene scene = OpenGameplayScene(); c.Actual = $"Scene đã mở={scene.path}, root={scene.rootCount}."; Assert.IsTrue(scene.IsValid(), "Scene mở ra không hợp lệ."); }); }
        [Test, Description("SCR-004: Kiểm tra scene không có Missing Script trên root/object con.")]
        public void SCR_004_SceneKhongMissingScript() { Run("SCR-004", "Scene không Missing Script", "Object trong scene gameplay không có Missing Script.", "High", c => { Scene scene = OpenGameplayScene(); int missing = 0; foreach (GameObject root in scene.GetRootGameObjects()) missing += CountMissingScripts(root); c.Actual = $"Scene={scene.path}, Missing Script={missing}."; Assert.AreEqual(0, missing, "Scene có Missing Script."); }); }
        [Test, Description("SCR-005: Kiểm tra scene có Camera.")]
        public void SCR_005_SceneCoCamera() { Run("SCR-005", "Scene có Camera", "Scene gameplay có ít nhất một Camera.", "High", c => { Scene scene = OpenGameplayScene(); int cameras = 0; foreach (GameObject root in scene.GetRootGameObjects()) cameras += root.GetComponentsInChildren<Camera>(true).Length; c.Actual = $"Scene={scene.path}, Camera={cameras}."; Assert.Greater(cameras, 0, "Scene gameplay không có Camera."); }); }
        [Test, Description("SCR-006: Kiểm tra scene có Collider map/nền.")]
        public void SCR_006_SceneCoCollider() { Run("SCR-006", "Scene có Collider", "Scene gameplay có Collider để va chạm map/nền.", "High", c => { Scene scene = OpenGameplayScene(); int colliders = 0; foreach (GameObject root in scene.GetRootGameObjects()) colliders += root.GetComponentsInChildren<Collider>(true).Length; c.Actual = $"Scene={scene.path}, Collider={colliders}."; Assert.Greater(colliders, 0, "Scene gameplay không có Collider."); }); }
        [Test, Description("SCR-007: Kiểm tra scene có Canvas/HUD nếu project dùng.")]
        public void SCR_007_CanvasHud() { Run("SCR-007", "Scene có Canvas/HUD nếu dùng", "Scene có Canvas/HUD nếu project dùng UI runtime.", "Low", c => { Scene scene = OpenGameplayScene(); int canvas = 0; foreach (GameObject root in scene.GetRootGameObjects()) canvas += root.GetComponentsInChildren<Canvas>(true).Length; c.Actual = $"Scene={scene.path}, Canvas={canvas}."; }); }
        [Test, Description("SCR-008: Kiểm tra scene có Player hoặc spawn point thật nếu project dùng.")]
        public void SCR_008_PlayerHoacSpawnPoint() { Run("SCR-008", "Scene có Player hoặc spawn point", "Scene có Player, SpawnPoint hoặc object liên quan vị trí sinh nhân vật.", "Medium", c => { Scene scene = OpenGameplayScene(); int found = 0; foreach (GameObject root in scene.GetRootGameObjects()) found += CountNamedChildren(root, "Player", "Kael", "Spawn", "StartPoint"); c.Actual = $"Scene={scene.path}, object Player/Spawn khớp={found}."; Assert.Greater(found, 0, "Scene không có Player hoặc spawn point rõ ràng."); }); }

        private Scene OpenGameplayScene()
        {
            string[] scenes = FindScenePaths();
            Assert.Greater(scenes.Length, 0, "Không tìm thấy scene thật trong Assets.");
            string selected = scenes[0];
            foreach (string scene in scenes)
            {
                if (scene.IndexOf("GameDemo", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                    scene.IndexOf("LongMap", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                    scene.IndexOf("Map", System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    selected = scene;
                    break;
                }
            }
            return EditorSceneManager.OpenScene(selected, OpenSceneMode.Single);
        }

        private int CountNamedChildren(GameObject root, params string[] tokens) { int count = 0; foreach (Transform t in root.GetComponentsInChildren<Transform>(true)) foreach (string token in tokens) if (t.name.IndexOf(token, System.StringComparison.OrdinalIgnoreCase) >= 0) { count++; break; } return count; }
    }
}
