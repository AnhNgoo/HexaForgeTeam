using UnityEngine;

public class StaticMinimap : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private RectTransform mapRect;      // MapContent
    [SerializeField] private RectTransform playerMarker;
    [SerializeField] private RectTransform pingMarker;

    [Header("Player")]
    [SerializeField] private Transform player;
    [SerializeField] private string playerTag = "Player";

    [Header("Khung map (world)")]
    [SerializeField] private bool useTerrainBounds = true;
    [SerializeField] private Vector3 mapCenter;
    [SerializeField] private Vector2 mapSize = new Vector2(100, 100);

    private void Start()
    {
        if (useTerrainBounds)
            FitToTerrain();
    }

    [ContextMenu("Fit to Terrain")]
    public void FitToTerrain()
    {
        Terrain t = Terrain.activeTerrain;
        if (t == null) return;
        Vector3 pos = t.GetPosition();
        Vector3 size = t.terrainData.size;
        mapCenter = pos + size * 0.5f;
        mapSize = new Vector2(size.x, size.z);
        Debug.Log($"✅ StaticMinimap fit: center={mapCenter}, size={mapSize}");
    }

    private void LateUpdate()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag(playerTag);
            if (p != null) player = p.transform;
        }

        // Player marker
        if (playerMarker != null && player != null)
            Place(playerMarker, player.position);

        // Ping marker (tự ẩn khi không có ping / khi tới nơi)
        if (pingMarker != null)
        {
            if (MapPingService.HasPing)
                Place(pingMarker, MapPingService.PingWorldPosition);
            else
                pingMarker.gameObject.SetActive(false);
        }
    }

    private void Place(RectTransform marker, Vector3 worldPos)
    {
        float nx = (worldPos.x - (mapCenter.x - mapSize.x * 0.5f)) / mapSize.x;
        float ny = (worldPos.z - (mapCenter.z - mapSize.y * 0.5f)) / mapSize.y;

        // Ngoài map → ẩn
        if (nx < 0f || nx > 1f || ny < 0f || ny > 1f)
        {
            marker.gameObject.SetActive(false);
            return;
        }

        marker.gameObject.SetActive(true);
        Vector2 size = mapRect.rect.size;
        marker.anchoredPosition = new Vector2((nx - 0.5f) * size.x, (ny - 0.5f) * size.y);
    }
}