using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

    [Header("Map UI Components")]
    [SerializeField] private RectTransform mapContent;
    [SerializeField] private Image mapImage;
    [SerializeField] private RawImage mapRawImage;
    [SerializeField] private RectTransform markerRoot;
    [SerializeField] private RectTransform playerMarker;
    [SerializeField] private RectTransform safeZoneRing;
    [SerializeField] private Image safeZoneOutsideOverlay;

    [Header("Fallback Bounds (Nếu Scene không có SceneMapConfig)")]
    [SerializeField] private BoundsSource boundsSource = BoundsSource.Renderers;
    [SerializeField] private Transform[] mapRoots;
    [SerializeField, Range(0.8f, 1.2f)] private float boundsScale = 1f;
    [SerializeField] private Vector2 worldMinXZ = new Vector2(-1500f, -1500f);
    [SerializeField] private Vector2 worldMaxXZ = new Vector2(1500f, 1500f);

    [Header("Player Tracking")]
    [SerializeField] private Transform player;
    [SerializeField] private string playerTag = "Player";

    [Header("Markers")]
    [SerializeField] private MapLocationMarkerUI markerPrefab;
    [SerializeField] private List<MapLocationData> defaultLocations = new List<MapLocationData>();

    [Header("Info Panel")]
    [SerializeField] private GameObject infoRoot;
    [SerializeField] private TMP_Text txtLocationName;
    [SerializeField] private TMP_Text txtLocationType;
    [SerializeField] private TMP_Text txtDescription;
    [SerializeField] private TMP_Text txtCurrentArea;

    [Header("Zoom")]
    [SerializeField] private float minZoom = 1f;
    [SerializeField] private float maxZoom = 4f;
    [SerializeField] private float zoomSpeed = 0.25f;
    [SerializeField] private bool zoomAtCursor = true;

    [Header("Ping")]
    [SerializeField] private RectTransform pingMarker;
    [SerializeField] private bool leftClickToPing = true;
    [SerializeField] private bool rightClickToClearPing = true;

    [Header("Ping Arrival")]
    [SerializeField] private bool enableArrivalDetection = true;
    [SerializeField] private float arrivalDistance = 5f;
    [SerializeField] private bool checkArrivalWhenMapClosed = true;

    [Header("Pan / Drag")]
    [SerializeField] private bool allowLeftMousePan = true;
    [SerializeField] private bool allowMiddleMousePan = true;
    [SerializeField] private bool clampMapInsideView = true;
    [SerializeField] private float dragThreshold = 5f;

    [Header("Safe Zone Visual")]
    [SerializeField, Range(1f, 1.2f)]
    private float safeZoneRingVisualScale = 1.08f;

    private float currentZoom = 1f;
    private bool draggingMap;
    private bool hasDragged;
    private Vector2 pointerDownPosition;
    private SceneMapConfig currentSceneConfig;

    private readonly List<MapLocationMarkerUI> spawnedMarkers = new List<MapLocationMarkerUI>();
    private readonly Dictionary<RuntimeMapStructure, MapLocationMarkerUI> runtimeMarkers
        = new Dictionary<RuntimeMapStructure, MapLocationMarkerUI>();
    private readonly Dictionary<RectTransform, Vector3> markerBaseScales
        = new Dictionary<RectTransform, Vector3>();

    private void Awake()
    {
        if (mapImage == null && mapContent != null) mapImage = mapContent.GetComponent<Image>();
        if (mapRawImage == null && mapContent != null) mapRawImage = mapContent.GetComponent<RawImage>();
        if (markerRoot == null) markerRoot = mapContent;

        FindPlayerIfMissing();

        CacheMarkerBaseScale(playerMarker);
        CacheMarkerBaseScale(pingMarker);

        EnsureRectMaskOnParent();
        HideSafeZoneVisuals();
    }

    private void OnEnable()
    {
        RuntimeMapStructure.Registered += HandleStructureRegistered;
        RuntimeMapStructure.Unregistered += HandleStructureUnregistered;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        RuntimeMapStructure.Registered -= HandleStructureRegistered;
        RuntimeMapStructure.Unregistered -= HandleStructureUnregistered;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        player = null;
        FindPlayerIfMissing();
        ApplySceneConfig(scene);

        if (gameObject.activeInHierarchy)
        {
            BuildMarkers();
            UpdateSafeZoneRing();
            UpdatePlayerMarker();
        }
    }

    private void Update()
    {
        FindPlayerIfMissing();

        if (enableArrivalDetection && (gameObject.activeSelf || checkArrivalWhenMapClosed))
        {
            CheckPingArrival();
        }

        if (!gameObject.activeSelf) return;

        UpdatePlayerMarker();
        UpdateSafeZoneRing();
        UpdateCurrentArea();
        RefreshPingMarker();
    }

    private void CheckPingArrival()
    {
        if (!MapPingService.HasPing || player == null) return;

        Vector3 ping = MapPingService.PingWorldPosition;
        Vector3 p = player.position;

        float dx = p.x - ping.x;
        float dz = p.z - ping.z;
        float distance = Mathf.Sqrt(dx * dx + dz * dz);

        if (distance <= arrivalDistance)
        {
            MapPingService.ClearPing();
            if (pingMarker != null) pingMarker.gameObject.SetActive(false);
        }
    }

    public void Open()
    {
        gameObject.SetActive(true);
        FindPlayerIfMissing();

        currentZoom = 1f;
        draggingMap = false;
        hasDragged = false;

        // Nạp cấu hình ảnh Map & Bounds của Scene hiện tại
        ApplySceneConfig();

        if (mapContent != null)
        {
            mapContent.gameObject.SetActive(true);
            mapContent.localScale = Vector3.one;
            mapContent.anchoredPosition = Vector2.zero;
        }

        BuildMarkers();
        UpdateSafeZoneRing();
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

    /// <summary>
    /// Tự động tìm SceneMapConfig trong Scene để nạp Ảnh Map & Tọa độ Bounds tương ứng
    /// </summary>
    public void ApplySceneConfig()
    {
        ApplySceneConfig(SceneManager.GetActiveScene());
    }

    private void ApplySceneConfig(Scene targetScene)
    {
        currentSceneConfig = FindSceneMapConfig(targetScene);

        if (currentSceneConfig != null)
        {
            if (mapImage != null &&
                currentSceneConfig.mapSprite != null)
            {
                mapImage.sprite = currentSceneConfig.mapSprite;
                mapImage.enabled = true;

                if (mapRawImage != null)
                    mapRawImage.enabled = false;
            }
            else if (mapRawImage != null &&
                     currentSceneConfig.mapTexture != null)
            {
                mapRawImage.texture =
                    currentSceneConfig.mapTexture;

                mapRawImage.enabled = true;

                if (mapImage != null)
                    mapImage.enabled = false;
            }

            currentSceneConfig.GetBounds(
                out worldMinXZ,
                out worldMaxXZ
            );

            Debug.Log(
                $"[WorldMapPanel] Scene={targetScene.name}, " +
                $"Config={currentSceneConfig.gameObject.scene.name}, " +
                $"Sprite={currentSceneConfig.mapSprite?.name}, " +
                $"Texture={currentSceneConfig.mapTexture?.name}"
            );

            return;
        }

        Debug.LogWarning(
            $"[WorldMapPanel] Scene {targetScene.name} " +
            "không có SceneMapConfig."
        );

        if (boundsSource != BoundsSource.Manual)
            AutoFitWorldBounds();
    }

    private SceneMapConfig FindSceneMapConfig(Scene targetScene)
    {
        SceneMapConfig[] configs =
            FindObjectsOfType<SceneMapConfig>(true);

        foreach (SceneMapConfig config in configs)
        {
            if (config != null &&
                config.gameObject.scene == targetScene)
            {
                return config;
            }
        }

        return null;
    }

    public void AutoFitWorldBounds()
    {
        bool has = false;
        Bounds b = new Bounds();

        var terrains = FindObjectsOfType<Terrain>(true);
        foreach (Terrain t in terrains)
        {
            Vector3 pos = t.GetPosition();
            Vector3 size = t.terrainData.size;
            Bounds tb = new Bounds(pos + size * 0.5f, size);
            if (!has) { b = tb; has = true; } else b.Encapsulate(tb);
        }

        if (has)
        {
            float hw = b.size.x * 0.5f * boundsScale;
            float hd = b.size.z * 0.5f * boundsScale;
            worldMinXZ = new Vector2(b.center.x - hw, b.center.z - hd);
            worldMaxXZ = new Vector2(b.center.x + hw, b.center.z + hd);
        }
    }

    private void EnsureRectMaskOnParent()
    {
        if (mapContent == null) return;
        RectTransform parent = mapContent.parent as RectTransform;
        if (parent != null && parent.GetComponent<RectMask2D>() == null)
        {
            parent.gameObject.AddComponent<RectMask2D>();
        }
    }

    // ==================== POINTER & DRAG ====================
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
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    viewport, eventData.position, null, out Vector2 vpLocal);

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
        if (hasDragged)
            return;

        if (ClickedLocationMarker(eventData))
            return;

        if (leftClickToPing &&
            eventData.button == PointerEventData.InputButton.Left)
        {
            SetPingFromScreen(eventData);
            return;
        }

        if (rightClickToClearPing &&
            eventData.button == PointerEventData.InputButton.Right)
        {
            ClearPing();
        }
    }

    // ==================== MARKERS & PLAYER ====================
    private void BuildMarkers()
    {
        ClearMarkers();

        if (markerPrefab == null) return;
        if (markerRoot == null) markerRoot = mapContent;
        if (markerRoot == null) return;

        // 1. Nạp danh sách công trình của SceneMapConfig (nếu có) hoặc defaultLocations
        List<MapLocationData> activeLocs = (currentSceneConfig != null && currentSceneConfig.sceneLocations.Count > 0)
            ? currentSceneConfig.sceneLocations
            : defaultLocations;

        foreach (MapLocationData location in activeLocs)
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

        // 2. Nạp toàn bộ các công trình có gắn component RuntimeMapStructure trong Scene
        foreach (RuntimeMapStructure structure in RuntimeMapStructure.ActiveStructures)
        {
            SpawnRuntimeMarker(structure);
        }

        // Luôn giữ Player Marker ở lớp trên cùng
        if (playerMarker != null) playerMarker.SetAsLastSibling();
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
        if (playerMarker == null) return;

        if (player == null)
        {
            playerMarker.gameObject.SetActive(false);
            return;
        }

        playerMarker.gameObject.SetActive(true);
        playerMarker.anchoredPosition = WorldToMapPosition(player.position);
        Transform heading = Camera.main != null ? Camera.main.transform : player;
        playerMarker.localEulerAngles = new Vector3(0f, 0f, -heading.eulerAngles.y);
        playerMarker.SetAsLastSibling();
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

    private void UpdateSafeZoneRing()
    {
        if (mapContent == null)
        {
            HideSafeZoneVisuals();
            return;
        }

        SafeZoneManager manager = SafeZoneManager.Instance;
        SafeZone safeZone = manager != null ? manager.SafeZone : null;

        if (safeZone == null ||
            safeZone.CurrentRadii.x <= 0f ||
            safeZone.CurrentRadii.y <= 0f)
        {
            HideSafeZoneVisuals();
            return;
        }


        // Vòng bo ban đầu bao trùm toàn bản đồ, chưa cần hiển thị trên UI.
        if (!manager.IsTutorialMode &&
            manager.CurrentPhaseIndex == 0 &&
            !safeZone.IsShrinking)
        {
            HideSafeZoneVisuals();
            return;
        }

        float worldWidth =
            Mathf.Max(worldMaxXZ.x - worldMinXZ.x, 1f);

        float worldHeight =
            Mathf.Max(worldMaxXZ.y - worldMinXZ.y, 1f);

        Rect mapRect = mapContent.rect;
        Vector2 radii = safeZone.CurrentRadii;

        Vector2 centerOnMap =
            WorldToMapPosition(safeZone.CurrentCenterPoint);

        if (safeZoneOutsideOverlay != null)
        {
            Vector2 centerUV = new Vector2(
                centerOnMap.x / mapRect.width + mapContent.pivot.x,
                centerOnMap.y / mapRect.height + mapContent.pivot.y
            );

            Vector2 radiiUV = new Vector2(
                radii.x / worldWidth,
                radii.y / worldHeight
            );

            safeZoneOutsideOverlay.gameObject.SetActive(true);

            Material material = safeZoneOutsideOverlay.material;

            if (material != null)
            {
                material.SetVector("_ZoneCenter", centerUV);
                material.SetVector("_ZoneRadii", radiiUV);
            }

            safeZoneOutsideOverlay.transform.SetAsFirstSibling();
        }

        if (safeZoneRing != null)
        {
            safeZoneRing.gameObject.SetActive(true);
            safeZoneRing.anchoredPosition = centerOnMap;

            Vector2 exactZoneSize = new Vector2(
                radii.x * 2f / worldWidth * mapRect.width,
                radii.y * 2f / worldHeight * mapRect.height
            );

            safeZoneRing.sizeDelta = exactZoneSize * safeZoneRingVisualScale;

            safeZoneRing.SetSiblingIndex(1);
        }
    }

    private void HideSafeZoneVisuals()
    {
        if (safeZoneRing != null)
            safeZoneRing.gameObject.SetActive(false);

        if (safeZoneOutsideOverlay != null)
            safeZoneOutsideOverlay.gameObject.SetActive(false);
    }

    public Vector2 WorldToMapPosition(Vector3 worldPosition)
    {
        if (mapContent == null) return Vector2.zero;

        float widthRange = Mathf.Max(worldMaxXZ.x - worldMinXZ.x, 1f);
        float heightRange = Mathf.Max(worldMaxXZ.y - worldMinXZ.y, 1f);

        float normalizedX = Mathf.Clamp01((worldPosition.x - worldMinXZ.x) / widthRange);
        float normalizedY = Mathf.Clamp01((worldPosition.z - worldMinXZ.y) / heightRange);

        Rect rect = mapContent.rect;
        return new Vector2(
            (normalizedX - mapContent.pivot.x) * rect.width,
            (normalizedY - mapContent.pivot.y) * rect.height);
    }

    private Vector3 MapToWorldPosition(Vector2 mapPosition)
    {
        Rect rect = mapContent.rect;

        float normalizedX = (mapPosition.x / rect.width) + mapContent.pivot.x;
        float normalizedY = (mapPosition.y / rect.height) + mapContent.pivot.y;

        float worldX = Mathf.Lerp(worldMinXZ.x, worldMaxXZ.x, normalizedX);
        float worldZ = Mathf.Lerp(worldMinXZ.y, worldMaxXZ.y, normalizedY);
        float worldY = player != null ? player.position.y : 0f;

        return new Vector3(worldX, worldY, worldZ);
    }

    private void SetPingFromScreen(PointerEventData eventData)
    {
        if (mapContent == null) return;

        bool valid = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            mapContent, eventData.position, eventData.pressEventCamera, out Vector2 localPoint);

        if (!valid || !mapContent.rect.Contains(localPoint)) return;

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

        pingMarker.anchoredPosition = WorldToMapPosition(MapPingService.PingWorldPosition);
        pingMarker.SetAsLastSibling();
    }

    private void ClearPing()
    {
        if (pingMarker != null) pingMarker.gameObject.SetActive(false);
        MapPingService.ClearPing();
    }

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

        List<MapLocationData> activeLocs = (currentSceneConfig != null && currentSceneConfig.sceneLocations.Count > 0)
            ? currentSceneConfig.sceneLocations
            : defaultLocations;

        foreach (MapLocationData location in activeLocs)
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

    private void FindPlayerIfMissing()
    {
        if (player != null && player.gameObject.activeInHierarchy) return;

        // 1. Tìm bằng Tag "Player"
        GameObject target = GameObject.FindGameObjectWithTag(playerTag);
        if (target != null)
        {
            player = target.transform;
            return;
        }

        // 2. Fallback tìm bằng PlayerManager
        if (PlayerManager.Instance != null && PlayerManager.Instance.CurrentCharacterBase != null)
        {
            player = PlayerManager.Instance.CurrentCharacterBase.transform;
            return;
        }

        // 3. Fallback tìm theo CharacterController
        var cc = FindObjectOfType<CharacterController>();
        if (cc != null)
        {
            player = cc.transform;
        }
    }
}