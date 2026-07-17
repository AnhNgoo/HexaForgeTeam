using System.IO;
using UnityEngine;

public class WorldMapCapture : MonoBehaviour
{
    [SerializeField] private Camera mapCamera;
    [SerializeField] private RenderTexture renderTexture;
    [SerializeField] private string fileName = "WorldMap.png";

    [ContextMenu("Capture World Map")]
    public void Capture()
    {
        if (mapCamera == null || renderTexture == null)
            return;

        RenderTexture currentRT = RenderTexture.active;

        mapCamera.targetTexture = renderTexture;
        RenderTexture.active = renderTexture;

        mapCamera.Render();

        Texture2D texture = new Texture2D(
            renderTexture.width,
            renderTexture.height,
            TextureFormat.RGBA32,
            false
        );

        texture.ReadPixels(
            new Rect(0, 0, renderTexture.width, renderTexture.height),
            0,
            0
        );

        texture.Apply();

        byte[] bytes = texture.EncodeToPNG();

        string path = Path.Combine(Application.dataPath, fileName);
        File.WriteAllBytes(path, bytes);

        RenderTexture.active = currentRT;

        Debug.Log("World map saved to: " + path);
    }
}