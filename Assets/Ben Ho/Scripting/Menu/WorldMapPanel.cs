using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum MapLocationType
{
    Church, Castle, Dungeon, Garden, Boss, Merchant, NPC, Portal
}

[Serializable]
public class MapLocationData
{
    public string locationName;
    public MapLocationType locationType;
    [TextArea] public string description;
    public Vector3 worldPosition;
    public Sprite icon;
    public bool discovered = true;
    public float identifyRadius = 50f;
}

public class WorldMapPanel : MonoBehaviour,
    IPointerClickHandler, IScrollHandler, IPointerDownHandler,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public enum BoundsSource { Manual, Renderers, Terrain }

    [Header("Map")]
    [SerializeField] private RectTransform mapContent;
    [SerializeField] private RectTransform markerRoot;
    [SerializeField] private RectTransform playerMarker;

    [Header("World Bounds (tự động lấy từ Map Roots)")]
    [SerializeField] private BoundsSource boundsSource = BoundsSource.Renderers;
    [SerializeField] private Transform[] mapRoots;
    [SerializeField, Range(0.8f, 1.2f)] private float boundsScale = 1f;
    [SerializeField] private Vector2 worldMinXZ = new Vector2(-1500f, -1500f);
    [SerializeField] private Vector2 worldMaxXZ = new Vector2(1500f, 1500f);

    [Header("Player")]
    [SerializeField] private Transform player;
    [SerializeField] private string playerTag = "Player";

    [Header("Markers")]
    [SerializeField] private MapLocationMarkerUI markerPrefab;
    [SerializeField] private List<MapLocationData> locations = new List<MapLocationData>();

    [Header("Info")]
    [SerializeField] private GameObject infoRoot;
    [SerializeField] private TMP_Text txtLocationName;
    [SerializeField] private TMP_Text txtLocationType;
    [SerializeField] private TMP_Text txtDescription;
    [SerializeField] private TMP_Text txtCurrentArea;

    [Header("Zoom (CHỈ trong khung map - tự gắn RectMask2D nếu thiếu)")]
    [SerializeField] private float minZoom = 1f;
    [SerializeField] private float maxZoom = 4f;
    [SerializeField] private float zoomSpeed = 0.25f;
    [SerializeField] private bool zoomAtCursor = true;

    [Header("Ping")]
    [SerializeField] private RectTransform pingMarker;
    [SerializeField] private bool leftClickToPing = true;
    [SerializeField] private bool rightClickToClearPing = true;

    [Header("Ping Arrival (tự xóa ping khi player tới nơi)")]
    [SerializeField] private bool enableArrivalDetection = true;
    [SerializeField] private float arrivalDistance = 5f;
    [SerializeField] private bool checkArrivalWhenMapClosed = true;

    [Header("Pan")]
    [SerializeField] private bool allowLeftMousePan = true;
    [SerializeField] private bool allowMiddleMousePan = true;
    [SerializeField] private bool clampMapInsideView = true;
    [SerializeField] private float dragThreshold = 5f;

    private float currentZoom = 1f;
    private bool draggingMap;
    private bool hasDragged;
    private Vector2 pointerDownPosition;

    private readonly List<MapLocationMarkerUI> spawnedMarkers = new List<MapLocationMarkerUI>();
    private readonly Dictionary<RuntimeMapStructure, MapLocationMarkerUI> runtimeMarkers
        = new Dictionary<RuntimeMapStructure, MapLocationMarkerUI>();
    private readonly Dictionary<RectTransform, Vector3> markerBaseScales
        = new Dictionary<RectTransform, Vector3>();

    // ======================================================================
    //                               LIFECYCLE
    // ======================================================================

    private void Awake()
    {
        FindPlayerIfMissing();
        if (markerRoot == null) markerRoot = mapContent;

        CacheMarkerBaseScale(playerMarker);
        CacheMarkerBaseScale(pingMarker);

        EnsureRectMaskOnParent();
    }

    private void OnEnable()
    {
        RuntimeMapStructure.Registered += HandleStructureRegistered;
        RuntimeMapStructure.Unregistered += HandleStructureUnregistered;
    }

    private void OnDisable()
    {
        RuntimeMapStructure.Registered -= HandleStructureRegistered;
        RuntimeMapStructure.Unregistered -= HandleStructureUnregistered;
    }

    private void Update()
    {
        FindPlayerIfMissing();

        // Check arrival: chạy cả khi map đóng (nếu tick option)
        if (enableArrivalDetection && (gameObject.activeSelf || checkArrivalWhenMapClosed))
        {
            CheckPingArrival();
        }

        // Logic cũ chỉ chạy khi map mở
        if (!gameObject.activeSelf) return;

        UpdatePlayerMarker();
        UpdateCurrentArea();
        RefreshPingMarker();
    }

    /// <summary>
    /// Nếu player cách ping ≤ arrivalDistance → tự ClearPing.
    /// Chỉ so khoảng cách trên mặt phẳng ngang (bỏ qua độ cao).
    /// </summary>
    private void CheckPingArrival()
    {
        if (!MapPingService.HasPing || player == null) return;

        Vector3 ping = MapPingService.PingWorldPosition;
        Vector3 p = player.position;

        // So sánh XZ (bỏ qua Y)
        float dx = p.x - ping.x;
        float dz = p.z - ping.z;
        float distance = Mathf.Sqrt(dx * dx + dz * dz);

        if (distance <= arrivalDistance)
        {
            Debug.Log($"✅ [WorldMapPanel] Đã tới ping (cách {distance:F1}m) → xóa ping");
            MapPingService.ClearPing();

            if (pingMarker != null)
                pingMarker.gameObject.SetActive(false);
        }
    }

    public void Open()
    {
        gameObject.SetActive(true);
        FindPlayerIfMissing();

        currentZoom = 1f;
        draggingMap = false;
        hasDragged = false;

        if (boundsSource != BoundsSource.Manual)
            AutoFitWorldBounds();

        if (mapContent != null)
        {
            mapContent.gameObject.SetActive(true);
            mapContent.localScale = Vector3.one;
            mapContent.anchoredPosition = Vector2.zero;
        }

        BuildMarkers();
        UpdateMarkerScales();
        UpdatePlayerMarker();
        UpdateCurrentArea();
        RefreshPingMarker();
        HideInfo();
        ClampMapPosition();
    }

    public void Close()
    {
        ClearMarkers();
        HideInfo();
        gameObject.SetActive(false);
    }

    // ======================================================================
    //                           AUTO BOUNDS TỪ MAP ROOTS
    // ======================================================================

    [ContextMenu("Auto-fit bounds từ Map Roots")]
    public void AutoFitWorldBounds()
    {
        if (mapRoots == null || mapRoots.Length == 0)
        {
            Debug.LogWarning("⚠️ [WorldMapPanel] Chưa gán Map Roots!");
            return;
        }

        bool has = false;
        Bounds b = new Bounds();

        foreach (Transform root in mapRoots)
        {
            if (root == null) continue;

            foreach (Terrain t in root.GetComponentsInChildren<Terrain>(true))
            {
                Vector3 pos = t.GetPosition();
                Vector3 size = t.terrainData.size;
                Bounds tb = new Bounds(pos + size * 0.5f, size);
                if (!has) { b = tb; has = true; } else b.Encapsulate(tb);
            }

            foreach (Renderer r in root.GetComponentsInChildren<Renderer>(true))
            {
                if (r is ParticleSystemRenderer) continue;
                if (!has) { b = r.bounds; has = true; } else b.Encapsulate(r.bounds);
            }
        }

        if (has)
        {
            float hw = b.size.x * 0.5f * boundsScale;
            float hd = b.size.z * 0.5f * boundsScale;
            worldMinXZ = new Vector2(b.center.x - hw, b.center.z - hd);
            worldMaxXZ = new Vector2(b.center.x + hw, b.center.z + hd);
            Debug.Log($"✅ [WorldMapPanel] Auto-fit bounds: Min={worldMinXZ}, Max={worldMaxXZ}");
        }
        else
        {
            Debug.LogWarning("⚠️ [WorldMapPanel] Map Roots không có Renderer/Terrain nào!");
        }
    }

    private void EnsureRectMaskOnParent()
    {
        if (mapContent == null) return;
        RectTransform parent = mapContent.parent as RectTransform;
        if (parent == null) return;

        if (parent.GetComponent<RectMask2D>() == null)
        {
            parent.gameObject.AddComponent<RectMask2D>();
            Debug.Log($"✅ [WorldMapPanel] Auto-added RectMask2D trên {parent.name}");
        }
    }

    // ======================================================================
    //                            POINTER EVENTS
    // ======================================================================

    public void OnPointerDown(PointerEventData eventData)
    {
        pointerDownPosition = eventData.position;
        hasDragged = false;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        bool leftDrag = allowLeftMousePan && eventData.button == PointerEventData.InputButton.Left;
        bool middleDrag = allowMiddleMousePan && eventData.button == PointerEventData.InputButton.Middle;
        draggingMap = leftDrag || middleDrag;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!draggingMap || mapContent == null) return;

        if (Vector2.Distance(pointerDownPosition, eventData.position) >= dragThreshold)
            hasDragged = true;

        mapContent.anchoredPosition += eventData.delta;
        ClampMapPosition();
    }

    public void OnEndDrag(PointerEventData eventData) => draggingMap = false;

    public void OnScroll(PointerEventData eventData)
    {
        if (mapContent == null) return;

        float delta = eventData.scrollDelta.y;
        if (Mathf.Approximately(delta, 0f)) return;

        float oldZoom = currentZoom;
        currentZoom = Mathf.Clamp(currentZoom + delta * zoomSpeed, minZoom, maxZoom);
        if (Mathf.Approximately(currentZoom, oldZoom)) return;

        if (zoomAtCursor)
        {
            RectTransform viewport = mapContent.parent as RectTransform;
            if (viewport != null)
            {
                Vector2 vpLocal;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    viewport, eventData.position, null, out vpLocal);

                Vector2 localOnMap = vpLocal - mapContent.anchoredPosition;
                float ratio = currentZoom / oldZoom;
                mapContent.anchoredPosition = vpLocal - localOnMap * ratio;
            }
        }

        mapContent.localScale = Vector3.one * currentZoom;
        UpdateMarkerScales();
        ClampMapPosition();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (hasDragged) return;

        if (ClickedLocationMarker(eventData)) return;

        if (leftClickToPing && eventData.button == PointerEventData.InputButton.Left)
        {
            SetPingFromScreen(eventData);
            return;
        }

        if (rightClickToClearPing && eventData.button == PointerEventData.InputButton.Right)
            ClearPing();
    }

    // ======================================================================
    //                            MARKERS + PLAYER
    // ======================================================================

    private void BuildMarkers()
    {
        ClearMarkers();

        if (markerPrefab == null) return;
        if (markerRoot == null) markerRoot = mapContent;
        if (markerRoot == null) return;

        foreach (MapLocationData location in locations)
        {
            if (location == null || !location.discovered) continue;

            MapLocationMarkerUI marker = Instantiate(markerPrefab, markerRoot);
            marker.Setup(location, this);

            RectTransform markerRect = marker.GetComponent<RectTransform>();
            if (markerRect != null)
            {
                markerRect.anchoredPosition = WorldToMapPosition(location.worldPosition);
                CacheMarkerBaseScale(markerRect);
            }

            spawnedMarkers.Add(marker);
        }

        foreach (RuntimeMapStructure structure in RuntimeMapStructure.ActiveStructures)
            SpawnRuntimeMarker(structure);
    }

    private void ClearMarkers()
    {
        foreach (MapLocationMarkerUI marker in spawnedMarkers)
        {
            if (marker == null) continue;
            RectTransform markerRect = marker.transform as RectTransform;
            if (markerRect != null) markerBaseScales.Remove(markerRect);
            Destroy(marker.gameObject);
        }

        spawnedMarkers.Clear();
        runtimeMarkers.Clear();
    }

    private void UpdatePlayerMarker()
    {
        if (player == null || playerMarker == null) return;

        playerMarker.gameObject.SetActive(true);
        playerMarker.anchoredPosition = WorldToMapPosition(player.position);
        playerMarker.localEulerAngles = new Vector3(0f, 0f, -player.eulerAngles.y);
    }

    private void UpdateMarkerScales()
    {
        ApplyInverseZoomScale(playerMarker);
        ApplyInverseZoomScale(pingMarker);

        foreach (MapLocationMarkerUI marker in spawnedMarkers)
        {
            if (marker != null)
                ApplyInverseZoomScale(marker.transform as RectTransform);
        }
    }

    private void CacheMarkerBaseScale(RectTransform marker)
    {
        if (marker != null && !markerBaseScales.ContainsKey(marker))
            markerBaseScales.Add(marker, marker.localScale);
    }

    private void ApplyInverseZoomScale(RectTransform marker)
    {
        if (marker == null) return;

        CacheMarkerBaseScale(marker);

        if (!markerBaseScales.TryGetValue(marker, out Vector3 baseScale)) return;

        bool inheritsMapScale = mapContent != null && marker.IsChildOf(mapContent);
        float safeZoom = Mathf.Max(currentZoom, Mathf.Epsilon);
        float scaleDivisor = inheritsMapScale ? safeZoom : 1f;
        marker.localScale = baseScale / scaleDivisor;
    }

    // ======================================================================
    //                           WORLD ↔ MAP CONVERSION
    // ======================================================================

    public Vector2 WorldToMapPosition(Vector3 worldPosition)
    {
        if (mapContent == null) return Vector2.zero;

        float normalizedX = Mathf.InverseLerp(worldMinXZ.x, worldMaxXZ.x, worldPosition.x);
        float normalizedY = Mathf.InverseLerp(worldMinXZ.y, worldMaxXZ.y, worldPosition.z);

        Rect rect = mapContent.rect;
        return new Vector2(
            (normalizedX - mapContent.pivot.x) * rect.width,
            (normalizedY - mapContent.pivot.y) * rect.height);
    }

    private Vector3 MapToWorldPosition(Vector2 mapPosition)
    {
        Rect rect = mapContent.rect;

        float normalizedX = mapPosition.x / rect.width + mapContent.pivot.x;
        float normalizedY = mapPosition.y / rect.height + mapContent.pivot.y;

        float worldX = Mathf.Lerp(worldMinXZ.x, worldMaxXZ.x, normalizedX);
        float worldZ = Mathf.Lerp(worldMinXZ.y, worldMaxXZ.y, normalizedY);
        float worldY = player != null ? player.position.y : 0f;

        return new Vector3(worldX, worldY, worldZ);
    }

    // ======================================================================
    //                                PING
    // ======================================================================

    private void SetPingFromScreen(PointerEventData eventData)
    {
        if (mapContent == null) return;

        Vector2 localPoint;
        bool valid = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            mapContent, eventData.position, eventData.pressEventCamera, out localPoint);

        if (!valid) return;
        if (!mapContent.rect.Contains(localPoint)) return;

        Vector3 worldPosition = MapToWorldPosition(localPoint);
        MapPingService.SetPing(worldPosition);
        RefreshPingMarker();
    }

    private void RefreshPingMarker()
    {
        if (pingMarker == null) return;

        bool hasPing = MapPingService.HasPing;
        pingMarker.gameObject.SetActive(hasPing);

        if (!hasPing) return;

        if (mapContent != null && !mapContent.gameObject.activeSelf)
            mapContent.gameObject.SetActive(true);

        pingMarker.anchoredPosition = WorldToMapPosition(MapPingService.PingWorldPosition);
        pingMarker.SetAsLastSibling();
    }

    private void ClearPing()
    {
        if (pingMarker != null) pingMarker.gameObject.SetActive(false);
        MapPingService.ClearPing();
    }

    // ======================================================================
    //                                PAN / CLAMP
    // ======================================================================

    private void ClampMapPosition()
    {
        if (!clampMapInsideView || mapContent == null) return;

        RectTransform viewport = mapContent.parent as RectTransform;
        if (viewport == null) return;

        Vector2 viewportSize = viewport.rect.size;
        Vector2 mapSize = Vector2.Scale(mapContent.rect.size, Vector2.one * currentZoom);

        Vector2 halfExcess = new Vector2(
            Mathf.Max(0f, (mapSize.x - viewportSize.x) * 0.5f),
            Mathf.Max(0f, (mapSize.y - viewportSize.y) * 0.5f));

        Vector2 position = mapContent.anchoredPosition;
        position.x = Mathf.Clamp(position.x, -halfExcess.x, halfExcess.x);
        position.y = Mathf.Clamp(position.y, -halfExcess.y, halfExcess.y);

        if (mapSize.x <= viewportSize.x) position.x = 0f;
        if (mapSize.y <= viewportSize.y) position.y = 0f;

        mapContent.anchoredPosition = position;
    }

    // ======================================================================
    //                               INFO / AREA
    // ======================================================================

    private void UpdateCurrentArea()
    {
        if (txtCurrentArea == null || player == null) return;

        MapLocationData area = GetCurrentArea(player.position);
        txtCurrentArea.text = area != null ? area.locationName : "Unknown Area";
    }

    private MapLocationData GetCurrentArea(Vector3 position)
    {
        MapLocationData nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (MapLocationData location in locations)
        {
            if (location == null || !location.discovered) continue;

            float distance = Vector2.Distance(
                new Vector2(position.x, position.z),
                new Vector2(location.worldPosition.x, location.worldPosition.z));

            if (distance <= location.identifyRadius && distance < nearestDistance)
            {
                nearest = location;
                nearestDistance = distance;
            }
        }

        return nearest;
    }

    public void ShowInfo(MapLocationData location)
    {
        if (location == null) return;
        if (infoRoot != null) infoRoot.SetActive(true);
        if (txtLocationName != null) txtLocationName.text = location.locationName;
        if (txtLocationType != null) txtLocationType.text = location.locationType.ToString();
        if (txtDescription != null) txtDescription.text = location.description;
    }

    public void HideInfo()
    {
        if (infoRoot != null) infoRoot.SetActive(false);
    }

    private bool ClickedLocationMarker(PointerEventData eventData)
    {
        if (eventData.pointerPressRaycast.gameObject == null) return false;
        return eventData.pointerPressRaycast.gameObject.GetComponentInParent<MapLocationMarkerUI>() != null;
    }

    // ======================================================================
    //                              RUNTIME MARKERS
    // ======================================================================

    private void SpawnRuntimeMarker(RuntimeMapStructure structure)
    {
        if (structure == null || markerPrefab == null || markerRoot == null
            || runtimeMarkers.ContainsKey(structure))
            return;

        MapLocationData location = structure.LocationData;
        if (!location.discovered) return;

        MapLocationMarkerUI marker = Instantiate(markerPrefab, markerRoot);
        marker.Setup(location, this);

        RectTransform markerRect = marker.GetComponent<RectTransform>();
        if (markerRect != null)
        {
            markerRect.anchoredPosition = WorldToMapPosition(location.worldPosition);
            CacheMarkerBaseScale(markerRect);
            ApplyInverseZoomScale(markerRect);
        }

        runtimeMarkers.Add(structure, marker);
        spawnedMarkers.Add(marker);
    }

    private void HandleStructureRegistered(RuntimeMapStructure structure)
    {
        if (gameObject.activeInHierarchy) SpawnRuntimeMarker(structure);
    }

    private void HandleStructureUnregistered(RuntimeMapStructure structure)
    {
        if (structure == null || !runtimeMarkers.TryGetValue(structure, out MapLocationMarkerUI marker))
            return;

        runtimeMarkers.Remove(structure);
        spawnedMarkers.Remove(marker);

        if (marker != null)
        {
            RectTransform markerRect = marker.transform as RectTransform;
            if (markerRect != null) markerBaseScales.Remove(markerRect);
            Destroy(marker.gameObject);
        }
    }

    // ======================================================================
    //                                  HELPERS
    // ======================================================================

    private void FindPlayerIfMissing()
    {
        if (player != null) return;
        GameObject target = GameObject.FindGameObjectWithTag(playerTag);
        if (target != null) player = target.transform;
    }
}