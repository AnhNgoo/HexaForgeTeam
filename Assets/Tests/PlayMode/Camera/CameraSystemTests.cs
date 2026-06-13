using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace DuskBlade.Tests
{
    public class CameraSystemTests : RuntimeSystemTestBase
    {
        protected override string ExportName => "Camera";

        [UnityTest, Category("Camera"), Category("Tự động"), Description("CAM-001: Kiểm tra tìm được Camera prefab hoặc camera gameplay thật.")]
        public IEnumerator CAM_001_TimDuocCameraThat() { return RunUnity("CAM-001", "Tìm được Camera thật", "Tìm thấy Camera prefab thật hoặc Camera trong gameplay scene.", "High", c => FindCamera(c)); }
        [UnityTest, Category("Camera"), Category("Tự động"), Description("CAM-002: Kiểm tra Camera prefab instantiate được.")]
        public IEnumerator CAM_002_CameraPrefabInstantiateDuoc() { return RunUnity("CAM-002", "Camera prefab instantiate được", "Camera prefab thật instantiate được không lỗi đỏ.", "High", c => InstantiateCamera(c)); }
        [UnityTest, Category("Camera"), Category("Tự động"), Description("CAM-003: Kiểm tra Camera có component render hoặc virtual camera.")]
        public IEnumerator CAM_003_CameraCoComponentHopLe() { return RunUnity("CAM-003", "Camera có component hợp lệ", "Camera có Unity Camera hoặc CinemachineVirtualCamera.", "High", c => CameraComponent(c)); }
        [UnityTest, Category("Camera"), Category("Tự động"), Description("CAM-004: Kiểm tra MainCamera tag hợp lệ nếu có Camera component.")]
        public IEnumerator CAM_004_MainCameraTagHopLe() { return RunUnity("CAM-004", "MainCamera tag hợp lệ", "Camera gameplay có tag MainCamera hoặc Camera.main truy cập được.", "Medium", c => MainCamera(c)); }
        [UnityTest, Category("Camera"), Category("Tự động"), Description("CAM-005: Kiểm tra CameraManager thật tồn tại nếu project dùng.")]
        public IEnumerator CAM_005_CameraManagerTonTai() { return RunUnity("CAM-005", "CameraManager tồn tại", "Có class hoặc component CameraManager thật trong project.", "Medium", c => CameraManager(c)); }
        [UnityTest, Category("Camera"), Category("Tự động"), Description("CAM-006: Kiểm tra script camera mobile/obstacle nếu project dùng.")]
        public IEnumerator CAM_006_ScriptCameraHoTroTonTai() { return RunUnity("CAM-006", "Script camera hỗ trợ tồn tại", "Có MobileCamera/AvoidObstacleForCamera/CameraShake nếu hệ thống camera dùng.", "Low", c => SupportScripts(c)); }
        [UnityTest, Category("Camera"), Category("Tự động"), Description("CAM-007: Kiểm tra thông số Camera component hợp lệ.")]
        public IEnumerator CAM_007_ThongSoCameraHopLe() { return RunUnity("CAM-007", "Thông số Camera hợp lệ", "FOV/clip plane của Camera thật hợp lệ.", "Medium", c => Settings(c)); }
        [UnityTest, Category("Camera"), Category("Tự động"), Description("CAM-008: Kiểm tra load gameplay scene có Camera không lỗi đỏ.")]
        public IEnumerator CAM_008_LoadSceneCoCamera() { return RunUnity("CAM-008", "Gameplay scene có Camera", "Load gameplay scene thật và tìm thấy Camera chính không lỗi đỏ.", "High", c => SceneCamera(c)); }
        [UnityTest, Category("Camera"), Category("Tự động"), Description("CAM-009: Kiểm tra khoảng cách Camera-Player hợp lý nếu scene có Player.")]
        public IEnumerator CAM_009_KhoangCachCameraPlayerHopLy() { return RunUnity("CAM-009", "Khoảng cách Camera-Player hợp lý", "Camera không trùng Player hoặc quá xa Player.", "Medium", c => Distance(c)); }
        [UnityTest, Category("Camera"), Category("Tự động"), Description("CAM-010: Kiểm tra Camera không bị disable ngay sau spawn/load.")]
        public IEnumerator CAM_010_CameraKhongTuDisable() { return RunUnity("CAM-010", "Camera không tự disable", "Camera thật vẫn active sau vài frame.", "High", c => Active(c)); }
        [UnityTest, Category("Camera"), Category("Tự động"), Description("CAM-011: Kiểm tra Camera chạy 60 frame không Error/Exception.")]
        public IEnumerator CAM_011_CameraChay60FrameKhongLoi() { return RunUnity("CAM-011", "Camera chạy 60 frame không lỗi đỏ", "Camera thật chạy 60 frame không lỗi đỏ.", "High", c => Sixty(c)); }

        private IEnumerator FindCamera(Ctx c) { GameObject p = FindCameraPrefab(); c.Actual = p ? "Camera prefab=" + p.name + "." : "Không tìm thấy Camera prefab thật."; Assert.IsNotNull(p); yield break; }
        private IEnumerator InstantiateCamera(Ctx c) { StartWatcher(); GameObject prefab = FindCameraPrefab(); GameObject go = InstantiatePrefab(prefab, new Vector3(0f, 3f, -6f), "_RuntimeTest"); yield return null; c.Actual = $"Camera prefab={prefab.name}, active={go.activeInHierarchy}, Error/Exception={ErrorCount()}."; AssertNoErrors("Instantiate Camera không được lỗi đỏ."); }
        private IEnumerator CameraComponent(Ctx c) { GameObject go = InstantiatePrefab(FindCameraPrefab(), new Vector3(0f, 3f, -6f), "_RuntimeTest"); yield return null; int unity = go.GetComponentsInChildren<Camera>(true).Length; int cine = CountComponents(go, "CinemachineVirtualCamera"); c.Actual = $"Unity Camera={unity}, CinemachineVirtualCamera={cine}."; Assert.IsTrue(unity > 0 || cine > 0); }
        private IEnumerator MainCamera(Ctx c) { GameObject go = InstantiatePrefab(FindCameraPrefab(), new Vector3(0f, 3f, -6f), "_RuntimeTest"); yield return null; Camera camera = go.GetComponentInChildren<Camera>(true); c.Actual = $"Camera tag={(camera ? camera.tag : "null")}, Camera.main={(Camera.main != null)}."; Assert.IsTrue(Camera.main != null || (camera != null && camera.CompareTag("MainCamera"))); }
        private IEnumerator CameraManager(Ctx c) { GameObject prefab = TestPrefabFinder.FindPrefabWithComponent("CameraManager"); bool type = FindType("CameraManager") != null; c.Actual = $"CameraManager class={type}, prefab={(prefab ? prefab.name : "không tìm thấy")}."; Assert.IsTrue(type || prefab != null); yield break; }
        private IEnumerator SupportScripts(Ctx c) { bool mobile = FindType("MobileCamera") != null; bool obstacle = FindType("AvoidObstacleForCamera") != null; bool shake = FindType("CameraShake") != null; c.Actual = $"MobileCamera={mobile}, AvoidObstacleForCamera={obstacle}, CameraShake={shake}."; Assert.IsTrue(mobile || obstacle || shake); yield break; }
        private IEnumerator Settings(Ctx c) { GameObject go = InstantiatePrefab(FindCameraPrefab(), new Vector3(0f, 3f, -6f), "_RuntimeTest"); yield return null; Camera camera = go.GetComponentInChildren<Camera>(true); Assert.IsNotNull(camera); c.Actual = $"FOV={N(camera.fieldOfView)}, near={N(camera.nearClipPlane)}, far={N(camera.farClipPlane)}."; Assert.Greater(camera.fieldOfView, 1f); Assert.Greater(camera.farClipPlane, camera.nearClipPlane); }
        private IEnumerator SceneCamera(Ctx c) { StartWatcher(); yield return LoadGameplayScene(c); yield return null; int count = UnityEngine.Object.FindObjectsOfType<Camera>(true).Length; c.Actual += $"Camera trong scene={count}, Camera.main={(Camera.main != null)}, Error/Exception={ErrorCount()}."; Assert.Greater(count, 0); AssertNoErrors("Load scene camera không được lỗi đỏ."); }
        private IEnumerator Distance(Ctx c) { yield return LoadGameplayScene(c); yield return null; Camera camera = Camera.main ?? UnityEngine.Object.FindObjectOfType<Camera>(); GameObject player = GameObject.FindWithTag("Player"); Assert.IsNotNull(camera); Assert.IsNotNull(player); float distance = Vector3.Distance(camera.transform.position, player.transform.position); c.Actual += $"Camera={camera.name}, Player={player.name}, khoảng cách={N(distance)}."; Assert.Greater(distance, 0.5f); Assert.Less(distance, 120f); }
        private IEnumerator Active(Ctx c) { GameObject go = InstantiatePrefab(FindCameraPrefab(), new Vector3(0f, 3f, -6f), "_RuntimeTest"); for (int i = 0; i < 10; i++) yield return null; Camera camera = go.GetComponentInChildren<Camera>(true); c.Actual = $"Camera root active={go.activeInHierarchy}, Camera enabled={(camera != null && camera.enabled)}."; Assert.IsTrue(go.activeInHierarchy); if (camera != null) Assert.IsTrue(camera.enabled); }
        private IEnumerator Sixty(Ctx c) { StartWatcher(); GameObject go = InstantiatePrefab(FindCameraPrefab(), new Vector3(0f, 3f, -6f), "_RuntimeTest"); for (int i = 0; i < 60; i++) yield return null; c.Actual = $"Camera={go.name}, frame=60, Error/Exception={ErrorCount()}."; AssertNoErrors("Camera chạy 60 frame không được lỗi đỏ."); }

        private GameObject FindCameraPrefab() { return TestPrefabFinder.FindPrefabWithComponent("CameraManager") ?? TestPrefabFinder.FindPrefabWithComponent("MobileCamera") ?? TestPrefabFinder.FindPrefabWithComponent("CinemachineVirtualCamera") ?? TestPrefabFinder.FindCameraPrefab(); }
        private int CountComponents(GameObject root, params string[] names) { int count = 0; foreach (Component component in root.GetComponentsInChildren<Component>(true)) { if (component == null) continue; foreach (string name in names) if (component.GetType().Name == name) { count++; break; } } return count; }
    }
}
