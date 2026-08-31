#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class MapCaptureTools
{
    [MenuItem("Tools/Map Capture 📷/Chụp TOÀN CẢNH map (chọn RenderTexture trước)")]
    private static void CaptureWholeMap()
    {
        RenderTexture rt = Selection.activeObject as RenderTexture;
        if (rt == null)
        {
            Debug.LogError("⚠️ Hãy chọn RenderTexture (MainMap2_RT) trong Project trước!");
            return;
        }

        // 1. Tính bounds gộp của TẤT CẢ terrain tiles
        bool has = false;
        Bounds b = new Bounds();

        foreach (Terrain t in Object.FindObjectsOfType<Terrain>())
        {
            Vector3 pos = t.GetPosition();
            Vector3 size = t.terrainData.size;
            Bounds tb = new Bounds(pos + size * 0.5f, size);
            if (!has) { b = tb; has = true; } else b.Encapsulate(tb);
        }

        if (!has)
        {
            Debug.LogError("⚠️ Không tìm thấy Terrain nào trong scene!");
            return;
        }

        // 2. Tự động đặt camera đúng tâm + ortho phủ kín map
        GameObject camObj = GameObject.Find("MapCaptureCamera");
        if (camObj == null)
        {
            camObj = new GameObject("MapCaptureCamera");
            camObj.AddComponent<Camera>();
        }

        Camera cam = camObj.GetComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = Mathf.Max(b.size.x, b.size.z) * 0.5f * 1.02f;
        cam.transform.position = new Vector3(b.center.x, b.max.y + 500f, b.center.z);
        cam.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 5000f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        cam.targetTexture = rt;

        // 3. Render ngay
        cam.Render();

        Debug.Log($"✅ ĐÃ CHỤP MAP! center={b.center}, size={b.size}, ortho={cam.orthographicSize:F0}");
        Debug.Log($"📋 Bounds cho WorldMapPanel: Min=({b.min.x:F0},{b.min.z:F0}) Max=({b.max.x:F0},{b.max.z:F0})");
    }
}
#endif