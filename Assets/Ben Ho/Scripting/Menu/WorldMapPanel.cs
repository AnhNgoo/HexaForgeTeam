using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public enum MapLocationType
{
    Church,
    Castle,
    Dungeon,
    Garden,
    Boss,
    Merchant,
    NPC,
    Portal

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
    IPointerClickHandler,
    IScrollHandler,
    IPointerDownHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    [Header("Map")]
    [SerializeField] private RectTransform mapContent;
    [SerializeField] private RectTransform markerRoot;
    [SerializeField] private RectTransform playerMarker;

    [Header("World Bounds")]
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

    [Header("Zoom")]
    [SerializeField] private float minZoom = 1f;
    [SerializeField] private float maxZoom = 3f;
    [SerializeField] private float zoomSpeed = 0.15f;

    [Header("Ping")]
    [SerializeField] private RectTransform pingMarker;
    [SerializeField] private bool leftClickToPing = true;
    [SerializeField] private bool rightClickToClearPing = true;

    [Header("Pan")]
    [SerializeField] private bool allowLeftMousePan = true;
    [SerializeField] private bool allowMiddleMousePan = true;
    [SerializeField] private bool clampMapInsideView = true;
    [SerializeField] private float dragThreshold = 5f;

    private float currentZoom = 1f;
    private bool draggingMap;
    private bool hasDragged;
    private Vector2 pointerDownPosition;

    private readonly List<MapLocationMarkerUI> spawnedMarkers =
        new List<MapLocationMarkerUI>();

    private readonly Dictionary<RuntimeMapStructure, MapLocationMarkerUI>
    runtimeMarkers = new Dictionary<RuntimeMapStructure, MapLocationMarkerUI>();

    private void Awake()
    {
        FindPlayerIfMissing();

        if (markerRoot == null)
            markerRoot = mapContent;
    }

    public void Open()
    {
        gameObject.SetActive(true);

        FindPlayerIfMissing();

        currentZoom = 1f;
        draggingMap = false;
        hasDragged = false;

        if (mapContent != null)
        {
            mapContent.gameObject.SetActive(true);
            mapContent.localScale = Vector3.one;
            mapContent.anchoredPosition = Vector2.zero;
        }

        BuildMarkers();
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

    private void Update()
    {
        if (!gameObject.activeSelf)
            return;

        FindPlayerIfMissing();
        UpdatePlayerMarker();
        UpdateCurrentArea();
        RefreshPingMarker();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        pointerDownPosition = eventData.position;
        hasDragged = false;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        bool leftDrag =
            allowLeftMousePan &&
            eventData.button == PointerEventData.InputButton.Left;

        bool middleDrag =
            allowMiddleMousePan &&
            eventData.button == PointerEventData.InputButton.Middle;

        draggingMap = leftDrag || middleDrag;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!draggingMap || mapContent == null)
            return;

        if (Vector2.Distance(pointerDownPosition, eventData.position) >= dragThreshold)
            hasDragged = true;

        mapContent.anchoredPosition += eventData.delta;
        ClampMapPosition();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        draggingMap = false;
    }

    public void OnScroll(PointerEventData eventData)
    {
        if (mapContent == null)
            return;

        float delta = eventData.scrollDelta.y;

        if (Mathf.Approximately(delta, 0f))
            return;

        currentZoom = Mathf.Clamp(
            currentZoom + delta * zoomSpeed,
            minZoom,
            maxZoom
        );

        mapContent.localScale = Vector3.one * currentZoom;
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

    private void BuildMarkers()
    {
        ClearMarkers();

        if (markerPrefab == null)
            return;

        if (markerRoot == null)
            markerRoot = mapContent;

        if (markerRoot == null)
            return;

        foreach (MapLocationData location in locations)
        {
            if (location == null || !location.discovered)
                continue;

            MapLocationMarkerUI marker =
                Instantiate(markerPrefab, markerRoot);

            marker.Setup(location, this);

            RectTransform markerRect =
                marker.GetComponent<RectTransform>();

            if (markerRect != null)
            {
                markerRect.anchoredPosition =
                    WorldToMapPosition(location.worldPosition);
            }

            spawnedMarkers.Add(marker);
        }
        foreach (RuntimeMapStructure structure
                in RuntimeMapStructure.ActiveStructures)
        {
            SpawnRuntimeMarker(structure);
        }
    }

    private void ClearMarkers()
    {
        foreach (MapLocationMarkerUI marker in spawnedMarkers)
        {
            if (marker != null)
                Destroy(marker.gameObject);
        }

        spawnedMarkers.Clear();
        runtimeMarkers.Clear();
    }

    private void UpdatePlayerMarker()
    {
        if (player == null || playerMarker == null)
            return;

        playerMarker.gameObject.SetActive(true);
        playerMarker.anchoredPosition = WorldToMapPosition(player.position);

        playerMarker.localEulerAngles =
            new Vector3(0f, 0f, -player.eulerAngles.y);
    }

    private void UpdateCurrentArea()
    {
        if (txtCurrentArea == null || player == null)
            return;

        MapLocationData area = GetCurrentArea(player.position);
        txtCurrentArea.text =
            area != null ? area.locationName : "Unknown Area";
    }

    private MapLocationData GetCurrentArea(Vector3 position)
    {
        MapLocationData nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (MapLocationData location in locations)
        {
            if (location == null || !location.discovered)
                continue;

            float distance = Vector2.Distance(
                new Vector2(position.x, position.z),
                new Vector2(location.worldPosition.x, location.worldPosition.z)
            );

            if (distance <= location.identifyRadius &&
                distance < nearestDistance)
            {
                nearest = location;
                nearestDistance = distance;
            }
        }

        return nearest;
    }

    public Vector2 WorldToMapPosition(Vector3 worldPosition)
    {
        if (mapContent == null)
            return Vector2.zero;

        float normalizedX =
            Mathf.InverseLerp(
                worldMinXZ.x,
                worldMaxXZ.x,
                worldPosition.x
            );

        float normalizedY =
            Mathf.InverseLerp(
                worldMinXZ.y,
                worldMaxXZ.y,
                worldPosition.z
            );

        Rect rect = mapContent.rect;

        return new Vector2(
            (normalizedX - mapContent.pivot.x) * rect.width,
            (normalizedY - mapContent.pivot.y) * rect.height
        );
    }

    private Vector3 MapToWorldPosition(Vector2 mapPosition)
    {
        Rect rect = mapContent.rect;

        float normalizedX =
            mapPosition.x / rect.width + mapContent.pivot.x;

        float normalizedY =
            mapPosition.y / rect.height + mapContent.pivot.y;

        float worldX =
            Mathf.Lerp(worldMinXZ.x, worldMaxXZ.x, normalizedX);

        float worldZ =
            Mathf.Lerp(worldMinXZ.y, worldMaxXZ.y, normalizedY);

        float worldY =
            player != null ? player.position.y : 0f;

        return new Vector3(worldX, worldY, worldZ);
    }

    private void SetPingFromScreen(PointerEventData eventData)
    {
        if (mapContent == null)
            return;

        Vector2 localPoint;

        bool valid =
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                mapContent,
                eventData.position,
                eventData.pressEventCamera,
                out localPoint
            );

        if (!valid)
            return;

        if (!mapContent.rect.Contains(localPoint))
            return;

        Vector3 worldPosition = MapToWorldPosition(localPoint);

        MapPingService.SetPing(worldPosition);
        RefreshPingMarker();
    }

    private void RefreshPingMarker()
    {
        if (pingMarker == null)
            return;

        bool hasPing = MapPingService.HasPing;

        pingMarker.gameObject.SetActive(hasPing);

        if (!hasPing)
            return;

        if (mapContent != null && !mapContent.gameObject.activeSelf)
            mapContent.gameObject.SetActive(true);

        pingMarker.anchoredPosition =
            WorldToMapPosition(MapPingService.PingWorldPosition);

        pingMarker.SetAsLastSibling();
    }

    private void ClearPing()
    {
        if (pingMarker != null)
            pingMarker.gameObject.SetActive(false);

        MapPingService.ClearPing();
    }

    private void ClampMapPosition()
    {
        if (!clampMapInsideView || mapContent == null)
            return;

        RectTransform viewport =
            mapContent.parent as RectTransform;

        if (viewport == null)
            return;

        Vector2 contentSize =
            mapContent.rect.size * currentZoom;

        Vector2 viewportSize =
            viewport.rect.size;

        Vector2 maxOffset = new Vector2(
            Mathf.Max(0f, (contentSize.x - viewportSize.x) * 0.5f),
            Mathf.Max(0f, (contentSize.y - viewportSize.y) * 0.5f)
        );

        Vector2 position = mapContent.anchoredPosition;

        position.x =
            Mathf.Clamp(position.x, -maxOffset.x, maxOffset.x);

        position.y =
            Mathf.Clamp(position.y, -maxOffset.y, maxOffset.y);

        mapContent.anchoredPosition = position;
    }

    private bool ClickedLocationMarker(PointerEventData eventData)
    {
        if (eventData.pointerPressRaycast.gameObject == null)
            return false;

        return eventData.pointerPressRaycast.gameObject
            .GetComponentInParent<MapLocationMarkerUI>() != null;
    }

    public void ShowInfo(MapLocationData location)
    {
        if (location == null)
            return;

        if (infoRoot != null)
            infoRoot.SetActive(true);

        if (txtLocationName != null)
            txtLocationName.text = location.locationName;

        if (txtLocationType != null)
            txtLocationType.text = location.locationType.ToString();

        if (txtDescription != null)
            txtDescription.text = location.description;
    }

    public void HideInfo()
    {
        if (infoRoot != null)
            infoRoot.SetActive(false);
    }

    private void FindPlayerIfMissing()
    {
        if (player != null)
            return;

        GameObject target =
            GameObject.FindGameObjectWithTag(playerTag);

        if (target != null)
            player = target.transform;
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

    private void SpawnRuntimeMarker(RuntimeMapStructure structure)
    {
        if (structure == null ||
            markerPrefab == null ||
            markerRoot == null ||
            runtimeMarkers.ContainsKey(structure))
        {
            return;
        }

        MapLocationData location = structure.LocationData;

        if (!location.discovered)
            return;

        MapLocationMarkerUI marker =
            Instantiate(markerPrefab, markerRoot);

        marker.Setup(location, this);

        RectTransform markerRect =
            marker.GetComponent<RectTransform>();

        if (markerRect != null)
        {
            markerRect.anchoredPosition =
                WorldToMapPosition(location.worldPosition);
        }

        runtimeMarkers.Add(structure, marker);
        spawnedMarkers.Add(marker);
    }

    private void HandleStructureRegistered(RuntimeMapStructure structure)
    {
        if (gameObject.activeInHierarchy)
            SpawnRuntimeMarker(structure);
    }

    private void HandleStructureUnregistered(RuntimeMapStructure structure)
    {
        if (structure == null ||
            !runtimeMarkers.TryGetValue(structure, out MapLocationMarkerUI marker))
        {
            return;
        }

        runtimeMarkers.Remove(structure);
        spawnedMarkers.Remove(marker);

        if (marker != null)
            Destroy(marker.gameObject);
    }
}