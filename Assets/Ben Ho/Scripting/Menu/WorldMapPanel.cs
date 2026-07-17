using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum MapLocationType
{
    SiteOfGrace,
    Castle,
    Dungeon,
    Boss,
    Merchant,
    NPC,
    Chest,
    Portal,
    Quest
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

public class WorldMapPanel : MonoBehaviour
{
    [Header("Map")]
    [SerializeField] private RectTransform mapContent;
    [SerializeField] private RectTransform markerRoot;
    [SerializeField] private RectTransform playerMarker;

    [Header("World Bounds")]
    [SerializeField] private Vector2 worldMinXZ;
    [SerializeField] private Vector2 worldMaxXZ;

    [Header("Player")]
    [SerializeField] private Transform player;

    [Header("Markers")]
    [SerializeField] private MapLocationMarkerUI markerPrefab;
    [SerializeField] private List<MapLocationData> locations = new List<MapLocationData>();

    [Header("Info")]
    [SerializeField] private GameObject infoRoot;
    [SerializeField] private TMP_Text txtLocationName;
    [SerializeField] private TMP_Text txtLocationType;
    [SerializeField] private TMP_Text txtDescription;
    [SerializeField] private TMP_Text txtCurrentArea;

    private readonly List<MapLocationMarkerUI> spawnedMarkers = new List<MapLocationMarkerUI>();

    public void Open()
    {
        gameObject.SetActive(true);
        BuildMarkers();
        UpdatePlayerMarker();
        UpdateCurrentArea();
        HideInfo();
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

        UpdatePlayerMarker();
        UpdateCurrentArea();
    }

    private void BuildMarkers()
    {
        ClearMarkers();

        if (markerPrefab == null || markerRoot == null)
            return;

        foreach (MapLocationData location in locations)
        {
            if (location == null || !location.discovered)
                continue;

            MapLocationMarkerUI marker = Instantiate(markerPrefab, markerRoot);
            marker.Setup(location, this);
            marker.GetComponent<RectTransform>().anchoredPosition =
                WorldToMapPosition(location.worldPosition);

            spawnedMarkers.Add(marker);
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
    }

    private void UpdatePlayerMarker()
    {
        if (player == null || playerMarker == null)
            return;

        playerMarker.anchoredPosition = WorldToMapPosition(player.position);
    }

    private void UpdateCurrentArea()
    {
        if (txtCurrentArea == null || player == null)
            return;

        MapLocationData area = GetCurrentArea(player.position);
        txtCurrentArea.text = area != null ? area.locationName : "Unknown Area";
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

            if (distance <= location.identifyRadius && distance < nearestDistance)
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

        float x = Mathf.InverseLerp(worldMinXZ.x, worldMaxXZ.x, worldPosition.x);
        float y = Mathf.InverseLerp(worldMinXZ.y, worldMaxXZ.y, worldPosition.z);

        Rect rect = mapContent.rect;

        return new Vector2(
            (x - mapContent.pivot.x) * rect.width,
            (y - mapContent.pivot.y) * rect.height
        );
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
}