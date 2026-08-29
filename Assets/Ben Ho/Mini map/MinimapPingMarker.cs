using UnityEngine;

/// <summary>
/// Hiển thị marker ping trên Minimap (bản đồ nhỏ trong gameplay).
/// Tự ẩn khi không có ping hoặc khi player đến gần ping.
/// </summary>
public class MinimapPingMarker : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private RectTransform pingIcon;   // Icon ping trên minimap
    [SerializeField] private RectTransform minimapRect; // Rect của MapContent

    [Header("Khung map (phải KHỚP với khung camera chụp map)")]
    [SerializeField] private bool useTerrainBounds = true;
    [SerializeField] private Vector3 mapCenter;
    [SerializeField] private Vector2 mapSize = new Vector2(1000, 1000);

    [Header("Rotation (tuỳ chọn)")]
    [SerializeField] private bool rotateWithPingDirection = false;
    [SerializeField] private Transform player;

    private void Start()
    {
        if (useTerrainBounds)
            FitToTerrain();
    }

    [ContextMenu("Fit to Terrain")]
    public void FitToTerrain()
    {
        Terrain t = Terrain.activeTerrain;
        if (t == null)
        {
            Debug.LogWarning("⚠️ Không tìm thấy Terrain!");
            return;
        }
        Vector3 pos = t.GetPosition();
        Vector3 size = t.terrainData.size;
        mapCenter = pos + size * 0.5f;
        mapSize = new Vector2(size.x, size.z);
        Debug.Log($"✅ MinimapPingMarker fit: center={mapCenter}, size={mapSize}");
    }

    private void LateUpdate()
    {
        if (pingIcon == null) return;

        bool hasPing = MapPingService.HasPing;

        if (!hasPing || minimapRect == null)
        {
            pingIcon.gameObject.SetActive(false);
            return;
        }

        // Tính vị trí normalized 0..1 của ping trên map
        Vector3 ping = MapPingService.PingWorldPosition;
        float nx = (ping.x - (mapCenter.x - mapSize.x * 0.5f)) / mapSize.x;
        float ny = (ping.z - (mapCenter.z - mapSize.y * 0.5f)) / mapSize.y;

        // Ping nằm ngoài map → ẩn luôn
        if (nx < 0f || nx > 1f || ny < 0f || ny > 1f)
        {
            pingIcon.gameObject.SetActive(false);
            return;
        }

        pingIcon.gameObject.SetActive(true);

        // Đặt vị trí
        Vector2 rectSize = minimapRect.rect.size;
        pingIcon.anchoredPosition = new Vector2(
            (nx - 0.5f) * rectSize.x,
            (ny - 0.5f) * rectSize.y
        );

        // Tuỳ chọn: quay icon chỉ về hướng ping từ player
        if (rotateWithPingDirection && player != null)
        {
            Vector2 toPing = new Vector2(
                ping.x - player.position.x,
                ping.z - player.position.z
            );
            float angle = Mathf.Atan2(toPing.x, toPing.y) * Mathf.Rad2Deg;
            pingIcon.localEulerAngles = new Vector3(0, 0, -angle + 180f);
        }
    }
}