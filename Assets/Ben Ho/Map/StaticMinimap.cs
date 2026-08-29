using UnityEngine;

public class StaticMinimap : MonoBehaviour
{
    public enum BoundsSource { Terrain, Camera, Renderers, Manual }

    [Header("UI")]
    [SerializeField] private RectTransform mapRect;   // MapContent
    [SerializeField] private RectTransform mapView;   // MapViewport (khung nhìn)
    [SerializeField] private RectTransform playerMarker;
    [SerializeField] private RectTransform pingMarker;

    [Header("Player")]
    [SerializeField] private Transform player;
    [SerializeField] private string playerTag = "Player";

    [Header("Khung map (world)")]
    [SerializeField] private BoundsSource boundsSource = BoundsSource.Renderers;
    [SerializeField] private Camera boundsCamera;
    [SerializeField] private Transform[] mapRoots;
    [SerializeField, Range(0.8f, 1.2f)] private float scaleFactor = 1f;
    [SerializeField] private Vector3 mapCenter;
    [SerializeField] private Vector2 mapSize = new Vector2(100, 100);

    [Header("Zoom (CHỈ scale khung map)")]
    [SerializeField] private bool enableZoom = true;
    [SerializeField] private float minZoom = 1f;
    [SerializeField] private float maxZoom = 4f;
    [SerializeField] private float zoomSpeed = 0.6f;
    private float currentZoom = 1f;

    [Header("Click để ping")]
    [SerializeField] private bool clickToPing = true;

    [Header("Debug")]
    [SerializeField] private bool debugLog = true;

    private void Start() => ApplyBounds();

    // ================= BOUNDS =================
    [ContextMenu("Apply Bounds")]
    public void ApplyBounds()
    {
        if (boundsSource == BoundsSource.Terrain) FitToTerrain();
        else if (boundsSource == BoundsSource.Camera) FitToCamera();
        else if (boundsSource == BoundsSource.Renderers) FitToRoot();

        mapSize *= scaleFactor;
        if (debugLog) Debug.Log($"[StaticMinimap] Bounds: center={mapCenter}, size={mapSize}");
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
    }

    [ContextMenu("Fit to Camera")]
    public void FitToCamera()
    {
        if (boundsCamera == null) return;
        float halfH = boundsCamera.orthographicSize;
        float aspect = boundsCamera.targetTexture != null
            ? (float)boundsCamera.targetTexture.width / boundsCamera.targetTexture.height
            : boundsCamera.aspect;
        Vector3 p = boundsCamera.transform.position;
        mapCenter = new Vector3(p.x, 0f, p.z);
        mapSize = new Vector2(halfH * aspect * 2f, halfH * 2f);
    }

    [ContextMenu("Fit to Root")]
    public void FitToRoot()
    {
        if (mapRoots == null || mapRoots.Length == 0) return;

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
            mapCenter = b.center;
            mapSize = new Vector2(b.size.x, b.size.z);
        }
    }

    // ================= CHUYỂN ĐỔI TOẠ ĐỘ =================
    public Vector2 WorldToMapNormalized(Vector3 worldPos)
    {
        float nx = (worldPos.x - (mapCenter.x - mapSize.x * 0.5f)) / mapSize.x;
        float ny = (worldPos.z - (mapCenter.z - mapSize.y * 0.5f)) / mapSize.y;
        return new Vector2(nx, ny);
    }

    public Vector3 MapToWorld(Vector2 n)
    {
        return new Vector3(
            mapCenter.x + (n.x - 0.5f) * mapSize.x,
            0f,
            mapCenter.z + (n.y - 0.5f) * mapSize.y);
    }

    // ================= ZOOM + CLICK PING =================
    private void Update()
    {
        if (!gameObject.activeInHierarchy) return;

        // Zoom bằng lăn chuột — CHỈ khi con trỏ nằm trong khung map
        if (enableZoom && mapRect != null && IsPointerOverMap())
        {
            float scroll = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                currentZoom = Mathf.Clamp(currentZoom + scroll * zoomSpeed, minZoom, maxZoom);
                mapRect.localScale = Vector3.one * currentZoom; // CHỈ scale MapContent
            }
        }

        // Click trái trên map → ping đúng vị trí world
        if (clickToPing && Input.GetMouseButtonDown(0) && IsPointerOverMap())
        {
            TryPingAtCursor();
        }
    }

    private bool IsPointerOverMap()
    {
        if (mapView == null) return false;
        return RectTransformUtility.RectangleContainsScreenPoint(
            mapView, Input.mousePosition, null);
    }

    private void TryPingAtCursor()
    {
        Vector2 local;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                mapRect, Input.mousePosition, null, out local))
            return;

        Vector2 size = mapRect.rect.size;
        Vector2 n = new Vector2(local.x / size.x + 0.5f, local.y / size.y + 0.5f);

        if (n.x < 0f || n.x > 1f || n.y < 0f || n.y > 1f) return;

        Vector3 world = MapToWorld(n);
        MapPingService.SetPing(world);

        if (debugLog) Debug.Log($"📍 Click ping → world: {world}");
    }

    // ================= ĐẶT MARKER =================
    private void LateUpdate()
    {
        if (player == null) FindPlayer();

        float inv = 1f / currentZoom; // giữ icon KHÔNG phình khi zoom

        if (playerMarker != null)
        {
            if (player != null) Place(playerMarker, player.position);
            else playerMarker.gameObject.SetActive(false);
            playerMarker.localScale = Vector3.one * inv;
        }

        if (pingMarker != null)
        {
            if (MapPingService.HasPing) Place(pingMarker, MapPingService.PingWorldPosition);
            else pingMarker.gameObject.SetActive(false);
            pingMarker.localScale = Vector3.one * inv;
        }
    }

    private void Place(RectTransform marker, Vector3 worldPos)
    {
        if (mapRect == null) return;

        Vector2 n = WorldToMapNormalized(worldPos);
        n.x = Mathf.Clamp01(n.x);
        n.y = Mathf.Clamp01(n.y);

        marker.gameObject.SetActive(true);
        Vector2 size = mapRect.rect.size;
        marker.anchoredPosition = new Vector2((n.x - 0.5f) * size.x, (n.y - 0.5f) * size.y);
    }

    private void FindPlayer()
    {
        if (player != null) return;

        GameObject p = GameObject.FindGameObjectWithTag(playerTag);
        if (p == null) p = GameObject.Find("Kael(Clone)");
        if (p == null) p = GameObject.Find("Kael");
        if (p == null)
        {
            CharacterBase cb = FindObjectOfType<CharacterBase>();
            if (cb != null) p = cb.gameObject;
        }

        if (p != null) player = p.transform;
    }
}