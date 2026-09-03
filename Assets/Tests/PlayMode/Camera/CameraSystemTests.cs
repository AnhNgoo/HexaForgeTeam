using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace DuskBlade.Tests
{
    public class CameraSystemTests : RuntimeSystemTestBase
    {
        protected override string ExportName => "Camera";

        // =========================================================
        // CAM-001
        // =========================================================

        [UnityTest, Category("Camera"), Category("Tu dong"),
         Description("CAM-001: Kiem tra scene RunGame(2) load duoc.")]
        public IEnumerator CAM_001_TimDuocCameraThat()
        {
            return RunUnity(
                "CAM-001",
                "Scene RunGame(2) load duoc",
                "Load duoc scene RunGame(2) de kiem tra camera.",
                "High",
                c => LoadRunScene(c)
            );
        }

        // =========================================================
        // CAM-002
        // =========================================================

        [UnityTest, Category("Camera"), Category("Tu dong"),
         Description("CAM-002: Ghi nhan Camera trong scene RunGame(2).")]
        public IEnumerator CAM_002_CameraPrefabInstantiateDuoc()
        {
            return RunUnity(
                "CAM-002",
                "Ghi nhan Camera scene RunGame(2)",
                "Ghi nhan Camera trong scene RunGame(2).",
                "Low",
                c => SceneCamera(c)
            );
        }

        // =========================================================
        // CAM-003
        // =========================================================

        [UnityTest, Category("Camera"), Category("Tu dong"),
         Description("CAM-003: Ghi nhan component Camera/Cinemachine trong scene RunGame(2).")]
        public IEnumerator CAM_003_CameraCoComponentHopLe()
        {
            return RunUnity(
                "CAM-003",
                "Component Camera scene RunGame(2)",
                "Ghi nhan Unity Camera hoac Cinemachine trong scene RunGame(2).",
                "Low",
                c => CameraComponent(c)
            );
        }

        // =========================================================
        // CAM-004
        // =========================================================

        [UnityTest, Category("Camera"), Category("Tu dong"),
         Description("CAM-004: Kiem tra MainCamera tag trong scene RunGame(2).")]
        public IEnumerator CAM_004_MainCameraTagHopLe()
        {
            return RunUnity(
                "CAM-004",
                "MainCamera scene RunGame(2)",
                "Tim thay Camera co tag MainCamera trong scene RunGame(2).",
                "Low",
                c => MainCamera(c)
            );
        }

        // =========================================================
        // CAM-005
        // =========================================================

        [UnityTest, Category("Camera"), Category("Tu dong"),
         Description("CAM-005: Kiem tra CameraManager cua project.")]
        public IEnumerator CAM_005_CameraManagerTonTai()
        {
            return RunUnity(
                "CAM-005",
                "CameraManager ton tai",
                "CameraManager class hoac prefab CameraManager ton tai neu project su dung.",
                "Low",
                c => CameraManager(c)
            );
        }

        // =========================================================
        // CAM-006
        // =========================================================

        [UnityTest, Category("Camera"), Category("Tu dong"),
         Description("CAM-006: Kiem tra cac script camera ho tro.")]
        public IEnumerator CAM_006_ScriptCameraHoTroTonTai()
        {
            return RunUnity(
                "CAM-006",
                "Script camera ho tro ton tai",
                "Ghi nhan MobileCamera/AvoidObstacleForCamera/CameraShake neu project co.",
                "Low",
                c => SupportScripts(c)
            );
        }

        // =========================================================
        // CAM-007
        // =========================================================

        [UnityTest, Category("Camera"), Category("Tu dong"),
         Description("CAM-007: Kiem tra thong so Camera trong scene RunGame(2).")]
        public IEnumerator CAM_007_ThongSoCameraHopLe()
        {
            return RunUnity(
                "CAM-007",
                "Thong so Camera scene RunGame(2)",
                "Camera co FOV va clip plane hop le.",
                "Low",
                c => Settings(c)
            );
        }

        // =========================================================
        // CAM-008
        // =========================================================

        [UnityTest, Category("Camera"), Category("Tu dong"),
         Description("CAM-008: Load scene RunGame(2) va quet Camera.")]
        public IEnumerator CAM_008_LoadSceneCoCamera()
        {
            return RunUnity(
                "CAM-008",
                "RunGame(2) quet Camera",
                "Load scene RunGame(2) va quet Camera khong crash.",
                "High",
                c => SceneCamera(c)
            );
        }

        // =========================================================
        // CAM-009
        // =========================================================

        [UnityTest, Category("Camera"), Category("Tu dong"),
         Description("CAM-009: Kiem tra khoang cach Camera va Player neu Player da spawn.")]
        public IEnumerator CAM_009_KhoangCachCameraPlayerHopLy()
        {
            return RunUnity(
                "CAM-009",
                "Khoang cach Camera-Player",
                "Neu scene co Camera va Player thi khoang cach nam trong pham vi hop ly.",
                "Low",
                c => Distance(c)
            );
        }

        // =========================================================
        // CAM-010
        // =========================================================

        [UnityTest, Category("Camera"), Category("Tu dong"),
         Description("CAM-010: Kiem tra Main Camera dang active trong scene RunGame(2).")]
        public IEnumerator CAM_010_CameraKhongTuDisable()
        {
            return RunUnity(
                "CAM-010",
                "Camera active scene RunGame(2)",
                "Main Camera ton tai va Camera component dang enabled.",
                "Low",
                c => Active(c)
            );
        }

        // =========================================================
        // CAM-011
        // =========================================================

        [UnityTest, Category("Camera"), Category("Tu dong"),
         Description("CAM-011: Kiem tra scene RunGame(2) chay 60 frame.")]
        public IEnumerator CAM_011_CameraChay60FrameKhongLoi()
        {
            return RunUnity(
                "CAM-011",
                "RunGame(2) camera chay 60 frame",
                "Scene RunGame(2) chay 60 frame khi kiem tra camera.",
                "High",
                c => Sixty(c)
            );
        }

        // =========================================================
        // LOAD RUN GAME(2)
        // =========================================================

        private IEnumerator LoadRunScene(Ctx c)
        {
            yield return LoadSceneByPath(
                TestSceneConfig.RunScene2Path,
                c
            );

            yield return null;

            c.Actual +=
                "Scene RunGame(2) load thanh cong.";
        }

        // =========================================================
        // CAM-002 / CAM-008
        // =========================================================

        private IEnumerator SceneCamera(Ctx c)
        {
            yield return LoadSceneByPath(
                TestSceneConfig.RunScene2Path,
                c
            );

            yield return null;

            Camera[] cameras =
                UnityEngine.Object.FindObjectsOfType<Camera>(true);

            Camera mainCamera = FindMainCameraInScene();

            c.Actual +=
                $"Scene RunGame(2), " +
                $"Camera trong scene={cameras.Length}, " +
                $"MainCamera={(mainCamera != null)}.";

            Assert.Greater(
                cameras.Length,
                0,
                "RunGame(2) khong tim thay bat ky Unity Camera nao."
            );

            Assert.IsNotNull(
                mainCamera,
                "RunGame(2) khong tim thay Camera co tag MainCamera."
            );
        }

        // =========================================================
        // CAM-003
        // =========================================================

        private IEnumerator CameraComponent(Ctx c)
        {
            yield return LoadSceneByPath(
                TestSceneConfig.RunScene2Path,
                c
            );

            yield return null;

            int unityCameraCount =
                UnityEngine.Object
                    .FindObjectsOfType<Camera>(true)
                    .Length;

            int cinemachineCount =
                CountSceneComponents(
                    "CinemachineVirtualCamera"
                );

            c.Actual +=
                $"Unity Camera={unityCameraCount}, " +
                $"CinemachineVirtualCamera={cinemachineCount}.";

            Assert.Greater(
                unityCameraCount,
                0,
                "RunGame(2) khong co Unity Camera."
            );
        }

        // =========================================================
        // CAM-004
        // =========================================================

        private IEnumerator MainCamera(Ctx c)
        {
            yield return LoadSceneByPath(
                TestSceneConfig.RunScene2Path,
                c
            );

            yield return null;

            Camera camera = FindMainCameraInScene();

            bool cameraMainExists =
                Camera.main != null;

            c.Actual +=
                $"Camera ton tai={(camera != null)}, " +
                $"Camera.main={cameraMainExists}, " +
                $"tag={(camera != null ? camera.tag : "null")}, " +
                $"active={(camera != null && camera.gameObject.activeInHierarchy)}, " +
                $"enabled={(camera != null && camera.enabled)}.";

            Assert.IsNotNull(
                camera,
                "Khong tim thay Camera co tag MainCamera trong RunGame(2)."
            );

            Assert.AreEqual(
                "MainCamera",
                camera.tag,
                "Camera duoc tim thay khong co tag MainCamera."
            );
        }

        // =========================================================
        // CAM-005
        // =========================================================

        private IEnumerator CameraManager(Ctx c)
        {
            bool type =
                FindType("CameraManager") != null;

            GameObject prefab =
                TestPrefabFinder.FindPrefabWithComponent(
                    "CameraManager"
                );

            c.Actual =
                $"CameraManager class={type}, " +
                $"prefab={(prefab != null ? prefab.name : "khong tim thay")}.";

            yield break;
        }

        // =========================================================
        // CAM-006
        // =========================================================

        private IEnumerator SupportScripts(Ctx c)
        {
            bool mobile =
                FindType("MobileCamera") != null;

            bool obstacle =
                FindType("AvoidObstacleForCamera") != null;

            bool shake =
                FindType("CameraShake") != null;

            c.Actual =
                $"MobileCamera={mobile}, " +
                $"AvoidObstacleForCamera={obstacle}, " +
                $"CameraShake={shake}.";

            yield break;
        }

        // =========================================================
        // CAM-007
        // =========================================================

        private IEnumerator Settings(Ctx c)
        {
            yield return LoadSceneByPath(
                TestSceneConfig.RunScene2Path,
                c
            );

            yield return null;

            Camera camera =
                FindMainCameraInScene();

            Assert.IsNotNull(
                camera,
                "Khong tim thay Main Camera trong RunGame(2)."
            );

            c.Actual =
                $"Camera={camera.name}, " +
                $"FOV={N(camera.fieldOfView)}, " +
                $"near={N(camera.nearClipPlane)}, " +
                $"far={N(camera.farClipPlane)}.";

            Assert.Greater(
                camera.fieldOfView,
                1f,
                "FOV Camera phai lon hon 1."
            );

            Assert.Less(
                camera.fieldOfView,
                180f,
                "FOV Camera khong hop le."
            );

            Assert.Greater(
                camera.nearClipPlane,
                0f,
                "Near Clip Plane phai lon hon 0."
            );

            Assert.Greater(
                camera.farClipPlane,
                camera.nearClipPlane,
                "Far Clip Plane phai lon hon Near Clip Plane."
            );
        }

        // =========================================================
        // CAM-009
        // =========================================================

        private IEnumerator Distance(Ctx c)
        {
            yield return LoadSceneByPath(
                TestSceneConfig.RunScene2Path,
                c
            );

            yield return null;

            Camera camera =
                FindMainCameraInScene();

            GameObject player =
                GameObject.FindWithTag("Player");

            if (camera == null || player == null)
            {
                c.Actual =
                    $"Camera={(camera != null)}, " +
                    $"Player={(player != null)}; " +
                    "Player co the chua spawn tai thoi diem test.";

                yield break;
            }

            float distance =
                Vector3.Distance(
                    camera.transform.position,
                    player.transform.position
                );

            c.Actual =
                $"Camera={camera.name}, " +
                $"Player={player.name}, " +
                $"khoang cach={N(distance)}.";

            Assert.Greater(
                distance,
                0.5f,
                "Camera qua gan Player."
            );

            Assert.Less(
                distance,
                120f,
                "Camera qua xa Player."
            );
        }

        // =========================================================
        // CAM-010
        // =========================================================

        private IEnumerator Active(Ctx c)
        {
            yield return LoadSceneByPath(
                TestSceneConfig.RunScene2Path,
                c
            );

            yield return null;

            Camera camera =
                FindMainCameraInScene();

            Assert.IsNotNull(
                camera,
                "Khong tim thay Main Camera trong RunGame(2)."
            );

            c.Actual =
                $"Camera={camera.name}, " +
                $"active={camera.gameObject.activeInHierarchy}, " +
                $"enabled={camera.enabled}.";

            Assert.IsTrue(
                camera.gameObject.activeInHierarchy,
                "Main Camera GameObject dang inactive."
            );

            Assert.IsTrue(
                camera.enabled,
                "Main Camera component dang disabled."
            );
        }

        // =========================================================
        // CAM-011
        // =========================================================

        private IEnumerator Sixty(Ctx c)
        {
            yield return LoadSceneByPath(
                TestSceneConfig.RunScene2Path,
                c
            );

            for (int i = 0; i < 60; i++)
            {
                yield return null;
            }

            Camera camera =
                FindMainCameraInScene();

            Assert.IsNotNull(
                camera,
                "Sau 60 frame van khong tim thay Main Camera."
            );

            Assert.IsTrue(
                camera.enabled,
                "Sau 60 frame Main Camera dang disabled."
            );

            c.Actual =
                $"Frame=60, Camera={camera.name}, " +
                $"enabled={camera.enabled}.";
        }

        // =========================================================
        // FIND MAIN CAMERA
        // =========================================================

        private Camera FindMainCameraInScene()
        {
            Camera[] cameras =
                UnityEngine.Object.FindObjectsOfType<Camera>(true);

            foreach (Camera camera in cameras)
            {
                if (camera == null)
                    continue;

                if (camera.CompareTag("MainCamera"))
                    return camera;
            }

            return null;
        }

        // =========================================================
        // COUNT COMPONENT
        // =========================================================

        private int CountSceneComponents(params string[] names)
        {
            int count = 0;

            foreach (
                Component component
                in UnityEngine.Object.FindObjectsOfType<Component>(true)
            )
            {
                if (component == null)
                    continue;

                foreach (string name in names)
                {
                    if (component.GetType().Name == name)
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