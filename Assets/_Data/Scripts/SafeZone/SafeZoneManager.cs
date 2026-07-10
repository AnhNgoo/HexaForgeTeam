using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using System;

public class SafeZoneManager : Singleton<SafeZoneManager>
{
    [SerializeField] private SafeZone safeZone;
    public SafeZone SafeZone => safeZone;
    [SerializeField][InlineEditor()] private SafeZoneData safeZoneData;
    [SerializeField] private List<Transform> targetCenterPoints = new List<Transform>();
    [SerializeField] private bool autoStartOnPlay = true;
    [SerializeField] private float resetAfterBossDelay = 1f;
    public bool IsSafeZoneCompleted { get; private set; } = false;
    public bool IsActiveSafeZone { get; private set; } = false; //Khi kích hoạt vật thể mới bị bo đốt

    public int CurrentPhaseIndex { get; private set; }
    private Transform currentTargetCenterPoint;
    public event Action<int, Vector3> OnSafeZonePhaseCompleted;

    public void Start()
    {
        if (autoStartOnPlay)
            StartSafeZoneFlow().Forget();
    }

    public async UniTaskVoid StartSafeZoneFlow(bool skipDelay = false)
    {
        CreateSafeZone();

        if (safeZoneData == null || safeZoneData.safeZoneStats == null)
            return;

        currentTargetCenterPoint = GetTargetCenterPoint();

        if (currentTargetCenterPoint == null)
        {
            Debug.LogWarning("[SafeZone] Không tìm thấy TargetCenterPoint.");
            return;
        }

        for (int i = 0; i < safeZoneData.safeZoneStats.Count; i++)
        {
            await ShrinkSafeZoneTurn(safeZoneData.safeZoneStats[i], skipDelay);
        }

        IsSafeZoneCompleted = true;
        OnSafeZonePhaseCompleted?.Invoke(CurrentPhaseIndex, safeZone.CurrentCenterPoint);
    }

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
        StartSafeZoneFlow().Forget();
    }

    [Button("Debug: Shrink Now")]
    public void DebugShrinkNow()
    {
        StartSafeZoneFlow(true).Forget();
    }

    private async UniTask ShrinkSafeZoneTurn(SafeZoneStat stat, bool skipDelay)
    {
        if (safeZone == null) return;
        if (safeZone.IsShrinking) return;

        IsSafeZoneCompleted = false;
        IsActiveSafeZone = true;

        if (!skipDelay)
            await UniTask.Delay(TimeSpan.FromSeconds(stat.timeDelay));

        CurrentPhaseIndex++;

        safeZone.ShrinkSafeZone(
            currentTargetCenterPoint.position,
            stat.radius,
            stat.shrinkDuration
        );

        while (safeZone.IsShrinking)
            await UniTask.Yield();
    }

    public async void ResetSafeZoneAfterBossDead()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(resetAfterBossDelay));

        CurrentPhaseIndex = 0;
        IsSafeZoneCompleted = false;

        if (safeZone != null)
            ObjectPooling.Instance.ReturnToPool(PoolType.SafeZone, safeZone.gameObject);

        safeZone = null;
        targetCenterPoints.Clear();

        StartSafeZoneFlow().Forget();
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
