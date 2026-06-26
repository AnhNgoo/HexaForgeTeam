using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using System;

public class SafeZoneManager : Singleton<SafeZoneManager>
{
    [SerializeField] private SafeZone safeZone;
    public SafeZone SafeZone => safeZone;
    [SerializeField] private SafeZoneData safeZoneData;
    [SerializeField] private List<Transform> targetCenterPoints = new List<Transform>();
    public bool IsSafeZoneCompleted { get; private set; } = false;
    public bool IsActiveSafeZone { get; private set; } = false; //Khi kích hoạt vật thể mới bị bo đốt

    [Button("Step 1: Create Safe Zone")]
    public void CreateSafeZone()
    {
        if (targetCenterPoints.Count > 0) return; // Đã có điểm trung tâm, không cần tạo lại vòng bo

        if (safeZone != null)
        {
            if (safeZone.IsShrinking) return; // Nếu vòng bo đang thu nhỏ thì không được tạo bo

            ObjectPooling.Instance?.ReturnToPool(PoolType.SafeZone, safeZone.gameObject);
            safeZone = null;
        }

        safeZone = ObjectPooling.Instance?
                .SpawnFromPool(PoolType.SafeZone,
                transform.position,
                Quaternion.identity)?.GetComponent<SafeZone>();

        if (safeZone == null) return;

        ResetSafeZone(safeZone);
        GetAllTargetCenterPoint();
        IsActiveSafeZone = true; //Khi tạo vòng bo mới kích hoạt bo đốt
    }

    [Button("Step 2: Start Shrink Safe Zone")]
    public async void ShrinkSafeZone()
    {
        if (safeZone == null) return;
        if (safeZone.IsShrinking) return;
        if (targetCenterPoints.Count == 0) return; // Không có điểm trung tâm nào để thu nhỏ vòng bo
        ResetSafeZone(safeZone);
        IsActiveSafeZone = true; //Khi kích hoạt vật thể mới bị bo đốt

        SafeZoneStat stat = safeZoneData.safeZoneStat;

        await UniTask.Delay(TimeSpan.FromSeconds(stat.timeDelay)); // Delay trước khi bắt đầu thu nhỏ vòng bo

        Vector3 targetCenterPoint = GetTargetCenterPoint().position;

        safeZone.ShrinkSafeZone(targetCenterPoint, stat.radius, stat.shrinkDuration);

        while (safeZone.IsShrinking)
        {
            // Chờ cho đến khi vòng bo hoàn thành việc thu nhỏ trước khi tiếp tục
            await UniTask.Yield();
        }

        //Đánh dấu bo cuối để gọi boss, dùng để check và gọi boss sau này
        IsSafeZoneCompleted = true;
    }

    private void ResetSafeZone(SafeZone safeZone)
    {
        Transform startCenterPoint = GetStartCenterPoint();
        if (startCenterPoint == null || safeZoneData == null) return;

        safeZone.InitSafeZone(startCenterPoint.position, safeZoneData.startRadius);
    }
    private void GetAllTargetCenterPoint()
    {
        // Lấy tất cả các điểm trung tâm của vòng bo để làm điểm spawn
        GameObject[] centerPointObjects = GameObject.FindGameObjectsWithTag("TargetCenterPoint");
        foreach (GameObject obj in centerPointObjects)
        {
            targetCenterPoints.Add(obj.transform);
        }
    }

    private Transform GetTargetCenterPoint()
    {
        // Lấy điểm trung tâm ngẫu nhiên từ danh sách để khởi tạo vòng bo
        if (targetCenterPoints.Count == 0) return null;

        int randomIndex = UnityEngine.Random.Range(0, targetCenterPoints.Count);
        Transform randomCenterPoint = targetCenterPoints[randomIndex];
        targetCenterPoints.RemoveAt(randomIndex); // Loại bỏ điểm đã sử dụng để tránh trùng lặp
        return randomCenterPoint;
    }

    private Transform GetStartCenterPoint()
    {
        // Lấy điểm trung tâm đầu tiên để khởi tạo vòng bo đầu tiên
        GameObject startCenterPointObject = GameObject.FindGameObjectWithTag("StartCenterPoint");
        if (startCenterPointObject != null)
        {
            return startCenterPointObject.transform;
        }
        return null;
    }
}
