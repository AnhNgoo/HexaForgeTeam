using UnityEngine;

public class LiveMinimap : MonoBehaviour
{
    [Header("Camera theo dõi (camera minimap)")]
    [SerializeField] private Camera mapCamera;

    [Header("UI")]
    [SerializeField] private RectTransform mapRect;      // MapContent
    [SerializeField] private RectTransform playerMarker;
    [SerializeField] private RectTransform pingMarker;

    [Header("Tuỳ chọn")]
    [SerializeField] private bool rotatePlayerMarker = true;
    [SerializeField] private Transform player;
    [SerializeField] private string playerTag = "Player";

    private void LateUpdate()
    {
        if (mapCamera == null) return;

        // ===== 1. Icon nhân vật LUÔN ở giữa (camera đã bám nhân vật) =====
        if (playerMarker != null)
        {
            playerMarker.gameObject.SetActive(true);
            playerMarker.anchoredPosition = Vector2.zero;

            if (rotatePlayerMarker)
            {
                if (player == null) FindPlayer();
                if (player != null)
                {
                    float yaw = Mathf.Atan2(player.forward.x, player.forward.z) * Mathf.Rad2Deg;
                    playerMarker.localEulerAngles = new Vector3(0, 0, -yaw);
                }
            }
        }

        // ===== 2. Ping marker: vị trí tương đối so với nhân vật =====
        if (pingMarker != null)
        {
            if (!MapPingService.HasPing)
            {
                pingMarker.gameObject.SetActive(false);
                return;
            }

            if (player == null) FindPlayer();
            Vector3 origin = player != null ? player.position : mapCamera.transform.position;

            Vector3 offset = MapPingService.PingWorldPosition - origin;
            offset.y = 0f;

            // Đổi sang hệ toạ độ của camera (xoay hay không xoay vẫn đúng)
            Vector3 local = mapCamera.transform.InverseTransformDirection(offset);

            float halfH = mapCamera.orthographicSize;
            float halfW = halfH * mapCamera.aspect;

            float nx = local.x / (halfW * 2f);
            float ny = local.y / (halfH * 2f);

            // Ngoài tầm nhìn → kẹp vào mép map (vẫn thấy hướng ping)
            nx = Mathf.Clamp(nx, -0.5f, 0.5f);
            ny = Mathf.Clamp(ny, -0.5f, 0.5f);

            Vector2 size = mapRect.rect.size;
            pingMarker.gameObject.SetActive(true);
            pingMarker.anchoredPosition = new Vector2(nx * size.x, ny * size.y);
        }
    }

    private void FindPlayer()
    {
        GameObject p = GameObject.FindGameObjectWithTag(playerTag);
        if (p == null) p = GameObject.Find("Kael(Clone)");
        if (p == null)
        {
            CharacterBase cb = FindObjectOfType<CharacterBase>();
            if (cb != null) p = cb.gameObject;
        }
        if (p != null) player = p.transform;
    }
}