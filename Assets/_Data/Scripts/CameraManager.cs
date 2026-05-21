using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Sirenix.OdinInspector;

public enum CameraType
{
    None = -1,
    Normal = 0,
    LockTarget = 1,
    // Thêm các loại camera khác nếu cần
}
public class CameraManager : Singleton<CameraManager>
{
    [SerializeField] private List<CameraRig> cameras = new List<CameraRig>();
    [SerializeField] private CameraType normalCameraId = CameraType.Normal;
    [SerializeField] private CameraType lockTargetCameraId = CameraType.LockTarget;
    [SerializeField] private AvoidObstacleForCamera avoidObstacleForCamera;
    [SerializeField] private CameraShake cameraShake;
    private readonly Dictionary<CameraType, CameraRig> cameraLookup = new Dictionary<CameraType, CameraRig>();
    private CameraRig currentCamera;

    [System.Serializable]
    public class CameraRig
    {
        public CameraType id;
        public CinemachineVirtualCamera virtualCamera;
        public CinemachineImpulseSource impulseSource;
        public MobileCamera mobileCamera;
        public bool syncRotationFromPrevious = true;
    }

    protected override void LoadComponent()
    {
        base.LoadComponent();
        if (cameras == null || cameras.Count == 0)
        {
            cameras = new List<CameraRig>();
            AddCameraRigFromChild(normalCameraId, "NormalCamera");
            AddCameraRigFromChild(lockTargetCameraId, "LockTargetCamera");
        }
        BuildCameraLookup();

        if (avoidObstacleForCamera == null)
            avoidObstacleForCamera = GetComponent<AvoidObstacleForCamera>();
        if (cameraShake == null)
            cameraShake = GetComponent<CameraShake>();
    }

    // Hàm chính để chuyển đổi camera
    public void SetCamera(CameraType cameraId, Transform followTarget, Transform lookAtTarget)
    {
        if (!cameraLookup.TryGetValue(cameraId, out CameraRig targetCamera))
        {
            Debug.LogWarning($"[{nameof(CameraManager)}] Camera id '{cameraId}' not found.");
            return;
        }

        if (targetCamera.syncRotationFromPrevious && currentCamera != null && currentCamera != targetCamera)
            SyncRotation(currentCamera, targetCamera);

        SetActiveCamera(targetCamera);
        currentCamera = targetCamera;
        SetupCamera(targetCamera, followTarget, lookAtTarget);
    }

    private void SetupCamera(CameraRig camera, Transform followTarget, Transform lookAtTarget)
    {
        if (camera == null) return;
        if (camera.virtualCamera != null || lookAtTarget != null || followTarget != null)
        {
            camera.virtualCamera.Follow = followTarget;
            camera.virtualCamera.LookAt = lookAtTarget;
        }
        if (avoidObstacleForCamera != null)
        {
            avoidObstacleForCamera.Init(camera.virtualCamera);
        }
        if (cameraShake != null)
        {
            cameraShake.SetImpulseSource(camera.impulseSource);
        }
    }

    // Thêm camera rig mới vào danh sách và xây dựng lại lookup
    private void BuildCameraLookup()
    {
        cameraLookup.Clear();
        foreach (CameraRig rig in cameras)
        {
            if (rig == null || rig.id == CameraType.None)
                continue;

            ResolveRig(rig);
            if (!cameraLookup.ContainsKey(rig.id))
                cameraLookup.Add(rig.id, rig);
        }
    }

    //Đảm bảo rằng các thành phần cần thiết của camera rig đã được gán, nếu không sẽ tự động tìm kiếm trong virtual camera
    private void ResolveRig(CameraRig rig)
    {
        if (rig.virtualCamera == null)
            return;

        if (rig.impulseSource == null)
            rig.impulseSource = rig.virtualCamera.GetComponent<CinemachineImpulseSource>();
        if (rig.mobileCamera == null)
            rig.mobileCamera = rig.virtualCamera.GetComponent<MobileCamera>();
    }

    //Thêm camera rig từ child của CameraManager nếu có
    private void AddCameraRigFromChild(CameraType cameraType, string childName)
    {
        Transform child = transform.Find(childName);
        if (child == null)
            return;

        CinemachineVirtualCamera virtualCamera = child.GetComponent<CinemachineVirtualCamera>();
        if (virtualCamera == null)
            return;

        CameraRig rig = new CameraRig
        {
            id = cameraType,
            virtualCamera = virtualCamera,
            impulseSource = virtualCamera.GetComponent<CinemachineImpulseSource>(),
            mobileCamera = virtualCamera.GetComponent<MobileCamera>()
        };
        cameras.Add(rig);
    }

    //Set camera active dựa trên camera rig được chọn, các camera khác sẽ bị vô hiệu hóa
    private void SetActiveCamera(CameraRig activeCamera)
    {
        foreach (CameraRig rig in cameras)
        {
            if (rig?.virtualCamera == null)
                continue;

            //Nếu là camera được chọn thì set active 
            rig.virtualCamera.gameObject.SetActive(rig == activeCamera);
        }
    }

    private void SyncRotation(CameraRig fromCamera, CameraRig toCamera)
    {
        if (fromCamera?.virtualCamera == null || toCamera?.virtualCamera == null)
            return;

        //Lấy component POV của camera mới để đồng bộ góc quay
        CinemachinePOV toPov = toCamera.virtualCamera.GetCinemachineComponent<CinemachinePOV>();
        if (toPov == null)
            return;

        //Lấy góc quay hiện tại của camera hiện tại
        CinemachinePOV fromPov = fromCamera.virtualCamera.GetCinemachineComponent<CinemachinePOV>();
        float horizontal;
        float vertical;

        if (fromPov != null) //Nêu camera cũ có POV thì đồng bộ theo góc quay của POV
        {
            horizontal = fromPov.m_HorizontalAxis.Value;
            vertical = fromPov.m_VerticalAxis.Value;
        }
        else //Ngược lại thì đồng bộ theo góc quay của camera
        {
            Vector3 euler = fromCamera.virtualCamera.transform.rotation.eulerAngles;
            horizontal = euler.y;
            vertical = euler.x > 180f ? euler.x - 360f : euler.x;
        }

        //Đồng bộ góc quay cho camera mới
        toPov.m_HorizontalAxis.Value = horizontal;
        toPov.m_VerticalAxis.Value = Mathf.Clamp(vertical, -70f, 70f);
        toCamera.mobileCamera?.SetAxis(toPov.m_HorizontalAxis.Value, toPov.m_VerticalAxis.Value);
    }
}
