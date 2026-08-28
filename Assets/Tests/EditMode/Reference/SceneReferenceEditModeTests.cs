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

        [Test, Description("SCR-001: Kiem tra project co scene Tutorial that.")]
        public void SCR_001_CoSceneGameplay()
        {
            Run("SCR-001", "Co scene Tutorial that", "Tim thay scene Tutorial trong Assets.", "High", c =>
            {
                string[] scenes = FindScenePaths();
                string selected = SelectTutorialScene(scenes);
                c.Actual = $"Scene tim thay={scenes.Length}, Tutorial={selected}.";
                Assert.IsFalse(string.IsNullOrEmpty(selected), "Khong tim thay scene Tutorial that trong Assets.");
            });
        }

        [Test, Description("SCR-002: Kiem tra co scene duoc bat trong Build Settings.")]
        public void SCR_002_SceneBuildSettings()
        {
            Run("SCR-002", "Scene duoc bat trong Build Settings", "Co it nhat mot scene enabled trong Build Settings de GameCI/PlayMode load duoc.", "High", c =>
            {
                int enabled = 0;
                bool tutorialEnabled = false;
                foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
                {
                    if (!scene.enabled) continue;
                    enabled++;
                    if (scene.path.IndexOf("Tutorial", System.StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        tutorialEnabled = true;
                    }
                }

                c.Actual = $"Scene enabled trong Build Settings={enabled}, Tutorial enabled={tutorialEnabled}.";
                Assert.IsTrue(tutorialEnabled || enabled > 0, "Khong co scene enabled trong Build Settings.");
            });
        }

        [Test, Description("SCR-003: Kiem tra scene Tutorial mo duoc o EditMode.")]
        public void SCR_003_OpenSceneDuoc()
        {
            Run("SCR-003", "Scene Tutorial mo duoc", "Scene Tutorial mo duoc bang EditorSceneManager.", "High", c =>
            {
                Scene scene = OpenGameplayScene();
                c.Actual = $"Scene da mo={scene.path}, root={scene.rootCount}.";
                Assert.IsTrue(scene.IsValid(), "Scene mo ra khong hop le.");
            });
        }

        [Test, Description("SCR-004: Kiem tra scene Tutorial khong co Missing Script tren root/object con.")]
        public void SCR_004_SceneKhongMissingScript()
        {
            Run("SCR-004", "Scene Tutorial khong Missing Script", "Object trong scene Tutorial khong co Missing Script.", "High", c =>
            {
                Scene scene = OpenGameplayScene();
                int missing = 0;
                foreach (GameObject root in scene.GetRootGameObjects()) missing += CountMissingScripts(root);
                c.Actual = $"Scene={scene.path}, Missing Script={missing}.";
                Assert.AreEqual(0, missing, "Scene Tutorial co Missing Script.");
            });
        }

        [Test, Description("SCR-005: Kiem tra scene Tutorial co Camera neu dung.")]
        public void SCR_005_SceneCoCamera()
        {
            Run("SCR-005", "Scene Tutorial co Camera neu dung", "Neu scene Tutorial co Camera thi reference hop le.", "Low", c =>
            {
                Scene scene = OpenGameplayScene();
                int cameras = 0;
                foreach (GameObject root in scene.GetRootGameObjects()) cameras += root.GetComponentsInChildren<Camera>(true).Length;
                c.Actual = $"Scene={scene.path}, Camera={cameras}.";
            });
        }

        [Test, Description("SCR-006: Kiem tra scene Tutorial co Collider neu dung.")]
        public void SCR_006_SceneCoCollider()
        {
            Run("SCR-006", "Scene Tutorial co Collider neu dung", "Neu scene Tutorial co Collider thi reference hop le.", "Low", c =>
            {
                Scene scene = OpenGameplayScene();
                int colliders = 0;
                foreach (GameObject root in scene.GetRootGameObjects()) colliders += root.GetComponentsInChildren<Collider>(true).Length;
                c.Actual = $"Scene={scene.path}, Collider={colliders}.";
            });
        }

        [Test, Description("SCR-007: Kiem tra scene Tutorial co Canvas/HUD neu dung.")]
        public void SCR_007_CanvasHud()
        {
            Run("SCR-007", "Scene Tutorial co Canvas/HUD neu dung", "Neu scene Tutorial co Canvas/HUD thi reference hop le.", "Low", c =>
            {
                Scene scene = OpenGameplayScene();
                int canvas = 0;
                foreach (GameObject root in scene.GetRootGameObjects()) canvas += root.GetComponentsInChildren<Canvas>(true).Length;
                c.Actual = $"Scene={scene.path}, Canvas={canvas}.";
            });
        }

        [Test, Description("SCR-008: Kiem tra scene Tutorial co object tutorial.")]
        public void SCR_008_PlayerHoacSpawnPoint()
        {
            Run("SCR-008", "Scene co object Tutorial", "Scene Tutorial co TutorialZones, TutorialSystem hoac object tutorial lien quan.", "Medium", c =>
            {
                Scene scene = OpenGameplayScene();
                int found = 0;
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    found += CountNamedChildren(root, "Tutorial", "TutorialZones", "TutorialSystem", "Player", "Kael", "Spawn", "StartPoint");
                }

                c.Actual = $"Scene={scene.path}, object tutorial/player/spawn khop={found}.";
                Assert.Greater(found, 0, "Scene Tutorial khong co object tutorial ro rang.");
            });
        }

        private Scene OpenGameplayScene()
        {
            string[] scenes = FindScenePaths();
            Assert.Greater(scenes.Length, 0, "Khong tim thay scene that trong Assets.");
            string selected = SelectTutorialScene(scenes);
            Assert.IsFalse(string.IsNullOrEmpty(selected), "Khong tim thay scene Tutorial that trong Assets.");
            return EditorSceneManager.OpenScene(selected, OpenSceneMode.Single);
        }

        private string SelectTutorialScene(string[] scenes)
        {
            foreach (string scene in scenes)
            {
                if (scene == "Assets/_Data/Scenes/Tutorial.unity") return scene;
            }

            foreach (string scene in scenes)
            {
                if (scene.IndexOf("Tutorial", System.StringComparison.OrdinalIgnoreCase) >= 0) return scene;
            }

            return null;
        }

        private int CountNamedChildren(GameObject root, params string[] tokens)
        {
            int count = 0;
            foreach (Transform transform in root.GetComponentsInChildren<Transform>(true))
            {
                foreach (string token in tokens)
                {
                    if (transform.name.IndexOf(token, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        count++;
                        break;
                    }
                }
            }

            return count;
        }
    }
}
