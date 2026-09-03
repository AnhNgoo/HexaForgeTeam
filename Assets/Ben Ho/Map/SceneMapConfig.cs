using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class SceneMapConfig : MonoBehaviour
{
    [Header("Ảnh Map riêng cho Scene này")]
    [Tooltip("Kéo Sprite hoặc Texture ảnh map của scene này vào đây")]
    public Sprite mapSprite;
    public Texture mapTexture;

    [Header("Tọa độ World Bounds của Scene")]
    [Tooltip("Tích chọn để tự động đo kích thước theo Terrain/Renderers của Scene")]
    public bool autoFitBoundsFromScene = true;
    public Transform[] mapRoots;
    [Range(0.8f, 1.5f)] public float boundsScale = 1f;

    [Tooltip("Nếu không Auto-fit, điền tọa độ góc Min/Max XZ thủ công")]
    public Vector2 manualMinXZ = new Vector2(-500f, -500f);
    public Vector2 manualMaxXZ = new Vector2(500f, 500f);

    [Header("Danh sách Công trình / Địa điểm trong Scene này")]
    public List<MapLocationData> sceneLocations = new List<MapLocationData>();

    public void GetBounds(out Vector2 minXZ, out Vector2 maxXZ)
    {
        if (!autoFitBoundsFromScene)
        {
            minXZ = manualMinXZ;
            maxXZ = manualMaxXZ;
            return;
        }

        Bounds b = new Bounds();
        bool has = false;

        // Ưu tiên đo theo mapRoots nếu có gán
        if (mapRoots != null && mapRoots.Length > 0)
        {
            foreach (var root in mapRoots)
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
        }
        else
        {
            // Tự động tìm tất cả Terrain trong Scene hiện tại
            var terrains = FindObjectsOfType<Terrain>(true);
            foreach (Terrain t in terrains)
            {
                Vector3 pos = t.GetPosition();
                Vector3 size = t.terrainData.size;
                Bounds tb = new Bounds(pos + size * 0.5f, size);
                if (!has) { b = tb; has = true; } else b.Encapsulate(tb);
            }
        }

        if (has)
        {
            float hw = b.size.x * 0.5f * boundsScale;
            float hd = b.size.z * 0.5f * boundsScale;
            minXZ = new Vector2(b.center.x - hw, b.center.z - hd);
            maxXZ = new Vector2(b.center.x + hw, b.center.z + hd);
        }
        else
        {
            minXZ = manualMinXZ;
            maxXZ = manualMaxXZ;
        }
    }
}