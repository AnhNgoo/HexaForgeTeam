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

        [Test, Description("SCR-001: Kiem tra project co scene Run Game that.")]
        public void SCR_001_CoSceneGameplay()
        {
            Run(
                "SCR-001",
                "Co scene Run Game that",
                "Tim thay scene Run Game trong Assets.",
                "High",
                c =>
                {
                    string selected = TestSceneConfig.RunScenePath;

                    Assert.IsFalse(
                        string.IsNullOrEmpty(selected),
                        "TestSceneConfig.RunScenePath khong duoc de trong."
                    );

                    SceneAsset sceneAsset =
                        AssetDatabase.LoadAssetAtPath<SceneAsset>(selected);

                    Assert.IsNotNull(
                        sceneAsset,
                        "Khong tim thay scene Run Game: " + selected
                    );

                    c.Actual = $"Scene tim thay={selected}.";
                }
            );
        }

        [Test, Description("SCR-002: Kiem tra co scene duoc bat trong Build Settings.")]
        public void SCR_002_SceneBuildSettings()
        {
            Run(
                "SCR-002",
                "Scene duoc bat trong Build Settings",
                "Co it nhat mot scene enabled trong Build Settings de GameCI/PlayMode load duoc.",
                "High",
                c =>
                {
                    int enabled = 0;
                    bool runSceneEnabled = false;

                    foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
                    {
                        if (!scene.enabled)
                            continue;

                        enabled++;

                        if (string.Equals(
                            scene.path,
                            TestSceneConfig.RunScenePath,
                            System.StringComparison.OrdinalIgnoreCase))
                        {
                            runSceneEnabled = true;
                        }
                    }

                    c.Actual =
                        $"Scene enabled trong Build Settings={enabled}, " +
                        $"RunScene enabled={runSceneEnabled}.";

                    Assert.IsTrue(
                        runSceneEnabled || enabled > 0,
                        "Khong co scene enabled trong Build Settings."
                    );
                }
            );
        }

        [Test, Description("SCR-003: Kiem tra scene Run Game mo duoc o EditMode.")]
        public void SCR_003_OpenSceneDuoc()
        {
            Run(
                "SCR-003",
                "Scene Run Game mo duoc",
                "Scene Run Game mo duoc bang EditorSceneManager.",
                "High",
                c =>
                {
                    Scene scene = OpenGameplayScene();

                    c.Actual =
                        $"Scene da mo={scene.path}, root={scene.rootCount}.";

                    Assert.IsTrue(
                        scene.IsValid(),
                        "Scene mo ra khong hop le."
                    );
                }
            );
        }

        [Test, Description("SCR-004: Kiem tra scene Run Game khong co Missing Script tren root/object con.")]
        public void SCR_004_SceneKhongMissingScript()
        {
            Run(
                "SCR-004",
                "Scene Run Game khong Missing Script",
                "Object trong scene Run Game khong co Missing Script.",
                "High",
                c =>
                {
                    Scene scene = OpenGameplayScene();

                    int missing = 0;

                    foreach (GameObject root in scene.GetRootGameObjects())
                    {
                        missing += CountMissingScripts(root);
                    }

                    c.Actual =
                        $"Scene={scene.path}, Missing Script={missing}.";

                    Assert.AreEqual(
                        0,
                        missing,
                        "Scene Run Game co Missing Script."
                    );
                }
            );
        }

        [Test, Description("SCR-005: Kiem tra scene Run Game co Camera neu dung.")]
        public void SCR_005_SceneCoCamera()
        {
            Run(
                "SCR-005",
                "Scene Run Game co Camera neu dung",
                "Neu scene Run Game co Camera thi reference hop le.",
                "Low",
                c =>
                {
                    Scene scene = OpenGameplayScene();

                    int cameras = 0;

                    foreach (GameObject root in scene.GetRootGameObjects())
                    {
                        cameras +=
                            root.GetComponentsInChildren<Camera>(true).Length;
                    }

                    c.Actual =
                        $"Scene={scene.path}, Camera={cameras}.";
                }
            );
        }

        [Test, Description("SCR-006: Kiem tra scene Run Game co Collider neu dung.")]
        public void SCR_006_SceneCoCollider()
        {
            Run(
                "SCR-006",
                "Scene Run Game co Collider neu dung",
                "Neu scene Run Game co Collider thi reference hop le.",
                "Low",
                c =>
                {
                    Scene scene = OpenGameplayScene();

                    int colliders = 0;

                    foreach (GameObject root in scene.GetRootGameObjects())
                    {
                        colliders +=
                            root.GetComponentsInChildren<Collider>(true).Length;
                    }

                    c.Actual =
                        $"Scene={scene.path}, Collider={colliders}.";
                }
            );
        }

        [Test, Description("SCR-007: Kiem tra scene Run Game co Canvas/HUD neu dung.")]
        public void SCR_007_CanvasHud()
        {
            Run(
                "SCR-007",
                "Scene Run Game co Canvas/HUD neu dung",
                "Neu scene Run Game co Canvas/HUD thi reference hop le.",
                "Low",
                c =>
                {
                    Scene scene = OpenGameplayScene();

                    int canvas = 0;

                    foreach (GameObject root in scene.GetRootGameObjects())
                    {
                        canvas +=
                            root.GetComponentsInChildren<Canvas>(true).Length;
                    }

                    c.Actual =
                        $"Scene={scene.path}, Canvas={canvas}.";
                }
            );
        }

        [Test, Description("SCR-008: Kiem tra scene Run Game co object player/spawn.")]
        public void SCR_008_PlayerHoacSpawnPoint()
        {
            Run(
                "SCR-008",
                "Scene co object Player/Spawn",
                "Scene Run Game co Player, Kael, Spawn hoac StartPoint.",
                "Medium",
                c =>
                {
                    Scene scene = OpenGameplayScene();

                    int found = 0;

                    foreach (GameObject root in scene.GetRootGameObjects())
                    {
                        found += CountNamedChildren(
                            root,
                            "Player",
                            "Kael",
                            "Spawn",
                            "StartPoint"
                        );
                    }

                    c.Actual =
                        $"Scene={scene.path}, object player/spawn khop={found}.";

                    Assert.Greater(
                        found,
                        0,
                        "Scene Run Game khong co object Player/Spawn ro rang."
                    );
                }
            );
        }

        private Scene OpenGameplayScene()
        {
            string selected = TestSceneConfig.RunScenePath;

            Assert.IsFalse(
                string.IsNullOrEmpty(selected),
                "TestSceneConfig.RunScenePath khong duoc de trong."
            );

            SceneAsset sceneAsset =
                AssetDatabase.LoadAssetAtPath<SceneAsset>(selected);

            Assert.IsNotNull(
                sceneAsset,
                "Khong tim thay scene Run Game: " + selected
            );

            return EditorSceneManager.OpenScene(
                selected,
                OpenSceneMode.Single
            );
        }

        private int CountNamedChildren(
            GameObject root,
            params string[] tokens)
        {
            int count = 0;

            foreach (
                Transform transform
                in root.GetComponentsInChildren<Transform>(true))
            {
                foreach (string token in tokens)
                {
                    if (
                        transform.name.IndexOf(
                            token,
                            System.StringComparison.OrdinalIgnoreCase
                        ) >= 0)
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