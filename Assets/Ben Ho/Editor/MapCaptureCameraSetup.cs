#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class MapCaptureCameraSetup
{
    private static readonly string[] excludeLayers = { "UI", "Player", "Enemy" };

    [MenuItem("Tools/Map Capture 📷/1. Tạo camera chụp map (chọn RenderTexture trước)")]
    private static void SetupCamera()
    {
        RenderTexture rt = Selection.activeObject as RenderTexture;
        if (rt == null)
        {
            Debug.LogError("⚠️ Hãy chọn RenderTexture (MainMap_RT) trong Project trước!");
            return;
        }

        // ✅ Tự động tính bounds của TOÀN BỘ map
        Bounds bounds = CalculateMapBounds();
        Debug.Log($"🗺️ Map bounds: center={bounds.center}, size={bounds.size}");

        GameObject camObj = GameObject.Find("MapCaptureCamera");
        if (camObj == null)
        {
            camObj = new GameObject("MapCaptureCamera");
            camObj.AddComponent<Camera>();
        }

        Camera cam = camObj.GetComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = Mathf.Max(bounds.size.x, bounds.size.z) * 0.525f;
        cam.transform.position = new Vector3(bounds.center.x, bounds.max.y + 500f, bounds.center.z);
        cam.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 5000f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        cam.cullingMask = CalculateCullingMask();
        cam.targetTexture = rt;

        cam.Render(); // Render NGAY

        Debug.Log("✅ Camera đã render! Select MainMap_RT và xem preview trong Inspector để kiểm tra.");
    }

    private static int CalculateCullingMask()
    {
        int mask = ~0;
        foreach (string layer in excludeLayers)
        {
            int idx = LayerMask.NameToLayer(layer);
            if (idx >= 0)
                mask &= ~(1 << idx);
        }
        return mask;
    }

    private static Bounds CalculateMapBounds()
    {
        bool hasBounds = false;
        Bounds result = new Bounds();

        // ✅ Quét tất cả Terrain
        foreach (Terrain t in Object.FindObjectsOfType<Terrain>())
        {
            Vector3 pos = t.GetPosition();
            Vector3 size = t.terrainData.size;
            Bounds b = new Bounds(pos + size * 0.5f, size);

            if (!hasBounds) { result = b; hasBounds = true; }
            else result.Encapsulate(b);
        }

        // ✅ Quét tất cả renderer (nhà cửa, cây cối, núi...)
        foreach (Renderer r in Object.FindObjectsOfType<Renderer>())
        {
            if (r is ParticleSystemRenderer) continue;
            if (r.GetComponentInParent<Camera>() != null) continue;

            string layerName = LayerMask.LayerToName(r.gameObject.layer);
            bool excluded = false;
            foreach (string ex in excludeLayers)
                if (layerName == ex) { excluded = true; break; }
            if (excluded) continue;

            if (!hasBounds) { result = r.bounds; hasBounds = true; }
            else result.Encapsulate(r.bounds);
        }

        if (!hasBounds)
        {
            Debug.LogWarning("⚠️ Không tìm thấy renderer nào! Map có thể chỉ sinh ra khi PLAY.");
            result = new Bounds(Vector3.zero, Vector3.one * 1000f);
        }

        return result;
    }
}
#endif