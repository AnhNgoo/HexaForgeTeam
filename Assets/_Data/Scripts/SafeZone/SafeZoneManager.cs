using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SafeZoneManager : Singleton<SafeZoneManager>
{
    [Header("Scene Mode")]
    [SerializeField] private bool isTutorialMode;
    public bool IsTutorialMode => isTutorialMode;
    [SerializeField] private PoolType normalSafeZonePool = PoolType.SafeZone;
    [SerializeField] private PoolType tutorialSafeZonePool = PoolType.TutorialSafeZone;
    private PoolType activeSafeZonePool;

    [SerializeField] private SafeZone safeZone;
    public SafeZone SafeZone => safeZone;
    [SerializeField][InlineEditor()] private SafeZoneData safeZoneData;
    public SafeZoneData SafeZoneData => safeZoneData;
    [SerializeField] private List<Transform> targetCenterPoints = new List<Transform>();
    [SerializeField] private bool autoStartOnPlay = true;
    [SerializeField] private float resetAfterBossDelay = 1f;
    public bool IsSafeZoneCompleted { get; private set; } = false; // Khi vòng bo đã hoàn tất tất cả các phase, không còn phase nào nữa
    public bool IsActiveSafeZone { get; private set; } = false; //Khi kích hoạt vật thể mới bị bo đốt
    public bool IsFinalSafeZone =>
        safeZoneData?.safeZoneStats != null &&
        safeZoneData.safeZoneStats.Count > 0 &&
        CurrentPhaseIndex >= safeZoneData.safeZoneStats.Count;

    public int CurrentPhaseIndex { get; private set; } // Khi vòng bo đang ở phase nào, bắt đầu từ 0, khi hoàn tất phase cuối cùng thì CurrentPhaseIndex = safeZoneData.safeZoneStats.Count
    private Transform currentTargetCenterPoint;
    private readonly System.Random pointRandom = new System.Random(Guid.NewGuid().GetHashCode());
    [ReadOnly, SerializeField] private List<Transform> usedTargetCenterPoints = new();
    public event Action<int, Transform> OnSafeZonePhaseCompleted;

    private void Start()
    {
        if (!autoStartOnPlay)
            return;

        if (isTutorialMode)
        {
            CreateSafeZone();
            return;
        }

        StartSafeZoneFlow().Forget();
        EventManager.Subscribe(GameEvent.OnReturnToLobby, ClearSafeZone);
    }

    private void OnDestroy()
    {
        EventManager.Unsubscribe(GameEvent.OnReturnToLobby, ClearSafeZone);
    }

    public async UniTaskVoid StartSafeZoneFlow(bool skipDelay = false)
    {
        EventManager.Notify(GameEvent.OnStartSafeZone);
        CreateSafeZone();

        if (safeZoneData == null || safeZoneData.safeZoneStats == null)
            return;

        currentTargetCenterPoint = GetTargetCenterPoint();

        if (currentTargetCenterPoint == null)
        {
            // Some lightweight scenes (including automated-test scenes) do not
            // contain phase points.  Keeping the initial centre is a valid
            // stationary safe zone and avoids aborting the whole game flow.
            currentTargetCenterPoint = GetStartCenterPoint();
            if (currentTargetCenterPoint == null)
            {
                return;
            }
        }

        for (int i = 0; i < safeZoneData.safeZoneStats.Count; i++)
        {
            await ShrinkSafeZoneTurn(safeZoneData.safeZoneStats[i], skipDelay);
        }

        IsSafeZoneCompleted = true;
        OnSafeZonePhaseCompleted?.Invoke(CurrentPhaseIndex, currentTargetCenterPoint);
        EventManager.Notify(GameEvent.OnFinalSafeZoneCompleted);
    }

    private void ClearSafeZone(object data = null)
    {
        if (safeZone != null)
        {
            ObjectPooling.Instance?.ReturnToPool(activeSafeZonePool, safeZone.gameObject);
            safeZone = null;
        }

        targetCenterPoints.Clear();
        usedTargetCenterPoints.Clear();
        CurrentPhaseIndex = 0;
        IsSafeZoneCompleted = false;
        IsActiveSafeZone = false;
    }

    [Button("Step 1: Create Safe Zone")]
    public void CreateSafeZone()
    {
        if (safeZone != null)
            return;

        activeSafeZonePool = isTutorialMode ? tutorialSafeZonePool : normalSafeZonePool;

        safeZone = ObjectPooling.Instance?.SpawnFromPool(activeSafeZonePool, transform.position, Quaternion.identity)?.GetComponent<SafeZone>();

        if (safeZone == null)
        return;

        ResetSafeZone(safeZone);
        GetAllTargetCenterPoint();
        IsActiveSafeZone = true;
    }

    [Button("Step 2: Start Shrink Safe Zone")]
    public void ShrinkSafeZone()
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

        safeZone?.ShrinkSafeZone(
            currentTargetCenterPoint.position,
            stat.radius,
            stat.shrinkDuration
        );

        while (safeZone != null && safeZone.IsShrinking)
            await UniTask.Yield();
    }

    public async void ResetSafeZoneAfterBossDead()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(resetAfterBossDelay));

        CurrentPhaseIndex = 0;
        IsSafeZoneCompleted = false;

        if (safeZone != null)
            ObjectPooling.Instance.ReturnToPool(activeSafeZonePool, safeZone.gameObject);

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
        GameObject[] centerPointObjects =
            GameObject.FindGameObjectsWithTag("TargetCenterPoint");

        foreach (GameObject obj in centerPointObjects)
        {
            if (!usedTargetCenterPoints.Contains(obj.transform))
                targetCenterPoints.Add(obj.transform);
        }
    }

    private Transform GetTargetCenterPoint()
    {
        if (targetCenterPoints.Count == 0)
            return null;

        int randomIndex = pointRandom.Next(targetCenterPoints.Count);
        Transform selectedPoint = targetCenterPoints[randomIndex];

        targetCenterPoints.RemoveAt(randomIndex);
        usedTargetCenterPoints.Add(selectedPoint);

        Debug.Log(
            $"[SafeZone] Random chọn {selectedPoint.name}, " +
            $"còn lại {targetCenterPoints.Count} điểm."
        );

        return selectedPoint;
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

    /// <summary>
    /// Kiểm tra đối tượng truyền vào có trong vòng bo hay không, nếu không có vòng bo thì mặc định là true
    /// </summary>
    public bool CheckObjectInSafeZone(Transform obj)
    {
        if (safeZone == null || obj == null)
            return true;

        return safeZone.Contains(obj.position);
    }

    /// <summary>
    /// Kiểm tra đối tượng truyền vào có trong vòng bo hay không và có cách rìa bo khoảng cách truyền vào hay không, nếu không có vòng bo thì mặc định là true
    /// </summary>
    public bool CheckObjectInSafeZone(Transform obj, float distanceFromEdge)
    {
        if (safeZone == null || obj == null)
            return true;

        return safeZone.Contains(obj.position, distanceFromEdge);
    }

    public void StopForFinalBoss()
    {
        IsActiveSafeZone = false;

        if (safeZone == null)
            return;

        safeZone.StopShrinkingSafeZone();

        ObjectPooling.Instance?.ReturnToPool(activeSafeZonePool, safeZone.gameObject);

        safeZone = null;
    }

#if UNITY_EDITOR
    #region Gizmos
    private void OnDrawGizmosSelected()
    {
        if (safeZoneData == null || safeZoneData.safeZoneStats == null)
            return;

        DrawStartZoneGizmo();

        GameObject[] targetPoints = GameObject.FindGameObjectsWithTag("TargetCenterPoint");

        foreach (GameObject targetPoint in targetPoints)
        {
            DrawPhaseGizmos(targetPoint.transform.position);
        }
    }

    private void DrawStartZoneGizmo()
    {
        GameObject startPoint = GameObject.FindGameObjectWithTag("StartCenterPoint");

        if (startPoint == null)
            return;

        Handles.color = Color.white;
        Handles.DrawWireDisc(startPoint.transform.position, Vector3.up, safeZoneData.startRadius);

        Handles.Label(startPoint.transform.position + Vector3.forward * safeZoneData.startRadius, $"Start | Radius: {safeZoneData.startRadius:0.#}");
    }

    private void DrawPhaseGizmos(Vector3 center)
    {
        int phaseCount = safeZoneData.safeZoneStats.Count;

        for (int i = 0; i < phaseCount; i++)
        {
            SafeZoneStat stat = safeZoneData.safeZoneStats[i];

            float colorPosition = phaseCount <= 1 ? 0f : i / (phaseCount - 1f);

            Handles.color = Color.Lerp(Color.yellow, Color.red, colorPosition);

            Handles.DrawWireDisc(center, Vector3.up, stat.radius);

            Handles.Label(center + Vector3.forward * stat.radius, $"Shrink Turn {i + 1} | Radius: {stat.radius:0.#}");
        }
    }
    #endregion
#endif
}
