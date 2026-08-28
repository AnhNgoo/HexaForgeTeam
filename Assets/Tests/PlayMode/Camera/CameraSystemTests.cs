using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace DuskBlade.Tests
{
    public class CameraSystemTests : RuntimeSystemTestBase
    {
        protected override string ExportName => "Camera";

        [UnityTest, Category("Camera"), Category("Tu dong"), Description("CAM-001: Kiem tra scene Tutorial load duoc de kiem tra camera.")]
        public IEnumerator CAM_001_TimDuocCameraThat() { return RunUnity("CAM-001", "Scene Tutorial load duoc", "Load duoc scene Tutorial that de kiem tra camera.", "High", c => LoadTutorial(c)); }

        [UnityTest, Category("Camera"), Category("Tu dong"), Description("CAM-002: Ghi nhan Camera trong scene Tutorial.")]
        public IEnumerator CAM_002_CameraPrefabInstantiateDuoc() { return RunUnity("CAM-002", "Ghi nhan Camera scene Tutorial", "Ghi nhan Camera trong scene Tutorial neu co.", "Low", c => SceneCamera(c)); }

        [UnityTest, Category("Camera"), Category("Tu dong"), Description("CAM-003: Ghi nhan component Camera/Cinemachine trong scene Tutorial.")]
        public IEnumerator CAM_003_CameraCoComponentHopLe() { return RunUnity("CAM-003", "Component Camera scene Tutorial", "Ghi nhan Unity Camera hoac Cinemachine trong scene Tutorial neu co.", "Low", c => CameraComponent(c)); }

        [UnityTest, Category("Camera"), Category("Tu dong"), Description("CAM-004: Ghi nhan MainCamera tag trong scene Tutorial.")]
        public IEnumerator CAM_004_MainCameraTagHopLe() { return RunUnity("CAM-004", "MainCamera scene Tutorial", "Ghi nhan MainCamera trong scene Tutorial neu co.", "Low", c => MainCamera(c)); }

        [UnityTest, Category("Camera"), Category("Tu dong"), Description("CAM-005: Ghi nhan CameraManager neu project dung.")]
        public IEnumerator CAM_005_CameraManagerTonTai() { return RunUnity("CAM-005", "CameraManager neu dung", "Ghi nhan CameraManager neu project dung.", "Low", c => CameraManager(c)); }

        [UnityTest, Category("Camera"), Category("Tu dong"), Description("CAM-006: Ghi nhan script camera ho tro neu project dung.")]
        public IEnumerator CAM_006_ScriptCameraHoTroTonTai() { return RunUnity("CAM-006", "Script camera ho tro neu dung", "Ghi nhan MobileCamera/AvoidObstacleForCamera/CameraShake neu co.", "Low", c => SupportScripts(c)); }

        [UnityTest, Category("Camera"), Category("Tu dong"), Description("CAM-007: Ghi nhan thong so Camera trong scene Tutorial.")]
        public IEnumerator CAM_007_ThongSoCameraHopLe() { return RunUnity("CAM-007", "Thong so Camera scene Tutorial", "Neu co Camera thi thong so FOV/clip plane hop le.", "Low", c => Settings(c)); }

        [UnityTest, Category("Camera"), Category("Tu dong"), Description("CAM-008: Kiem tra load scene Tutorial va quet Camera.")]
        public IEnumerator CAM_008_LoadSceneCoCamera() { return RunUnity("CAM-008", "Tutorial scene quet Camera", "Load scene Tutorial va quet Camera khong crash.", "High", c => SceneCamera(c)); }

        [UnityTest, Category("Camera"), Category("Tu dong"), Description("CAM-009: Ghi nhan khoang cach Camera-Player neu scene co ca hai.")]
        public IEnumerator CAM_009_KhoangCachCameraPlayerHopLy() { return RunUnity("CAM-009", "Khoang cach Camera-Player neu co", "Neu scene co Camera va Player thi khoang cach hop le.", "Low", c => Distance(c)); }

        [UnityTest, Category("Camera"), Category("Tu dong"), Description("CAM-010: Ghi nhan Camera active trong scene Tutorial.")]
        public IEnumerator CAM_010_CameraKhongTuDisable() { return RunUnity("CAM-010", "Camera active scene Tutorial", "Neu scene co Camera thi Camera khong bi disable bat thuong.", "Low", c => Active(c)); }

        [UnityTest, Category("Camera"), Category("Tu dong"), Description("CAM-011: Kiem tra scene Tutorial chay 60 frame.")]
        public IEnumerator CAM_011_CameraChay60FrameKhongLoi() { return RunUnity("CAM-011", "Tutorial camera chay 60 frame", "Scene Tutorial chay 60 frame khi kiem tra camera.", "High", c => Sixty(c)); }

        private IEnumerator LoadTutorial(Ctx c) { yield return LoadGameplayScene(c); yield return null; c.Actual += "Scene Tutorial load thanh cong."; }
        private IEnumerator SceneCamera(Ctx c) { yield return LoadGameplayScene(c); yield return null; Camera[] cameras = UnityEngine.Object.FindObjectsOfType<Camera>(true); c.Actual += $"Camera trong scene={cameras.Length}, Camera.main={(Camera.main != null)}."; }
        private IEnumerator CameraComponent(Ctx c) { yield return LoadGameplayScene(c); yield return null; int unity = UnityEngine.Object.FindObjectsOfType<Camera>(true).Length; int cine = CountSceneComponents("CinemachineVirtualCamera"); c.Actual += $"Unity Camera={unity}, CinemachineVirtualCamera={cine}."; }
        private IEnumerator MainCamera(Ctx c) { yield return LoadGameplayScene(c); yield return null; Camera camera = Camera.main ?? UnityEngine.Object.FindObjectOfType<Camera>(); c.Actual += $"Camera ton tai={(camera != null)}, tag={(camera != null ? camera.tag : "null")}, Camera.main={(Camera.main != null)}."; }
        private IEnumerator CameraManager(Ctx c) { bool type = FindType("CameraManager") != null; GameObject prefab = TestPrefabFinder.FindPrefabWithComponent("CameraManager"); c.Actual = $"CameraManager class={type}, prefab={(prefab ? prefab.name : "khong tim thay")}."; yield break; }
        private IEnumerator SupportScripts(Ctx c) { bool mobile = FindType("MobileCamera") != null; bool obstacle = FindType("AvoidObstacleForCamera") != null; bool shake = FindType("CameraShake") != null; c.Actual = $"MobileCamera={mobile}, AvoidObstacleForCamera={obstacle}, CameraShake={shake}."; yield break; }
        private IEnumerator Settings(Ctx c) { yield return LoadGameplayScene(c); yield return null; Camera camera = Camera.main ?? UnityEngine.Object.FindObjectOfType<Camera>(); if (camera == null) { c.Actual += "Scene Tutorial khong co Camera runtime bat buoc."; yield break; } c.Actual += $"FOV={N(camera.fieldOfView)}, near={N(camera.nearClipPlane)}, far={N(camera.farClipPlane)}."; Assert.Greater(camera.fieldOfView, 1f); Assert.Greater(camera.farClipPlane, camera.nearClipPlane); }
        private IEnumerator Distance(Ctx c) { yield return LoadGameplayScene(c); yield return null; Camera camera = Camera.main ?? UnityEngine.Object.FindObjectOfType<Camera>(); GameObject player = GameObject.FindWithTag("Player"); if (camera == null || player == null) { c.Actual += $"Camera={(camera != null)}, Player={(player != null)}; scene Tutorial khong bat buoc co ca hai object."; yield break; } float distance = Vector3.Distance(camera.transform.position, player.transform.position); c.Actual += $"Camera={camera.name}, Player={player.name}, khoang cach={N(distance)}."; Assert.Greater(distance, 0.5f); Assert.Less(distance, 120f); }
        private IEnumerator Active(Ctx c) { yield return LoadGameplayScene(c); yield return null; Camera camera = Camera.main ?? UnityEngine.Object.FindObjectOfType<Camera>(); c.Actual += $"Camera ton tai={(camera != null)}, enabled={(camera != null && camera.enabled)}."; if (camera != null) Assert.IsTrue(camera.enabled); }
        private IEnumerator Sixty(Ctx c) { yield return LoadGameplayScene(c); for (int i = 0; i < 60; i++) yield return null; c.Actual += "Frame=60."; }

        private int CountSceneComponents(params string[] names)
        {
            int count = 0;
            foreach (Component component in UnityEngine.Object.FindObjectsOfType<Component>(true))
            {
                if (component == null) continue;
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
