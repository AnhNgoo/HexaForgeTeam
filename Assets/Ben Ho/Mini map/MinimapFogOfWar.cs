using UnityEngine;
using UnityEngine.UI;

public class MinimapFogOfWar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Camera minimapCamera;
    [SerializeField] private RawImage fogRawImage;

    [Header("World Bounds X/Z")]
    [SerializeField] private Vector2 worldMin =
        new Vector2(-500f, -500f);

    [SerializeField] private Vector2 worldMax =
        new Vector2(500f, 500f);

    [Header("Fog")]
    [SerializeField, Range(64, 1024)]
    private int resolution = 256;

    [SerializeField] private float revealRadius = 15f;
    [SerializeField] private float revealStep = 0.5f;

    [SerializeField, Range(0f, 1f)]
    private float softEdge = 0.3f;

    [SerializeField, Range(0f, 1f)]
    private float fogOpacity = 0.9f;

    private Texture2D fogTexture;
    private Color32[] fogPixels;
    private Vector3 lastRevealPosition =
        Vector3.positiveInfinity;

    private void Start()
    {
        if (player == null)
        {
            GameObject target =
                GameObject.FindGameObjectWithTag("Player");

            if (target != null)
                player = target.transform;
        }

        CreateFogTexture();
    }

    private void LateUpdate()
    {
        if (player == null ||
            minimapCamera == null ||
            fogTexture == null)
        {
            return;
        }

        UpdateFogUV();

        Vector2 movement = new Vector2(
            player.position.x - lastRevealPosition.x,
            player.position.z - lastRevealPosition.z);

        if (movement.sqrMagnitude >=
            revealStep * revealStep)
        {
            Reveal(player.position);
            lastRevealPosition = player.position;
        }
    }

    private void CreateFogTexture()
    {
        fogTexture = new Texture2D(
            resolution,
            resolution,
            TextureFormat.RGBA32,
            false);

        fogTexture.name = "Runtime Minimap Fog";
        fogTexture.wrapMode = TextureWrapMode.Clamp;
        fogTexture.filterMode = FilterMode.Bilinear;

        fogPixels =
            new Color32[resolution * resolution];

        byte alpha =
            (byte)(fogOpacity * 255f);

        for (int i = 0; i < fogPixels.Length; i++)
        {
            fogPixels[i] =
                new Color32(0, 0, 0, alpha);
        }

        fogTexture.SetPixels32(fogPixels);
        fogTexture.Apply(false);

        fogRawImage.texture = fogTexture;
        fogRawImage.color = Color.white;
        fogRawImage.raycastTarget = false;
    }

    private void Reveal(Vector3 worldPosition)
    {
        float worldWidth =
            worldMax.x - worldMin.x;

        float worldDepth =
            worldMax.y - worldMin.y;

        int centerX = Mathf.RoundToInt(
            (worldPosition.x - worldMin.x) /
            worldWidth * resolution);

        int centerY = Mathf.RoundToInt(
            (worldPosition.z - worldMin.y) /
            worldDepth * resolution);

        int radiusX = Mathf.CeilToInt(
            revealRadius / worldWidth * resolution);

        int radiusY = Mathf.CeilToInt(
            revealRadius / worldDepth * resolution);

        float innerRadius =
            Mathf.Clamp01(1f - softEdge);

        for (int y = -radiusY; y <= radiusY; y++)
        {
            for (int x = -radiusX; x <= radiusX; x++)
            {
                int pixelX = centerX + x;
                int pixelY = centerY + y;

                if (pixelX < 0 || pixelX >= resolution ||
                    pixelY < 0 || pixelY >= resolution)
                {
                    continue;
                }

                float normalizedX =
                    radiusX > 0 ? (float)x / radiusX : 0f;

                float normalizedY =
                    radiusY > 0 ? (float)y / radiusY : 0f;

                float distance = Mathf.Sqrt(
                    normalizedX * normalizedX +
                    normalizedY * normalizedY);

                if (distance > 1f)
                    continue;

                float edgeAlpha = Mathf.InverseLerp(
                    innerRadius,
                    1f,
                    distance);

                byte newAlpha = (byte)(
                    edgeAlpha * fogOpacity * 255f);

                int index =
                    pixelY * resolution + pixelX;

                if (newAlpha < fogPixels[index].a)
                {
                    fogPixels[index].a = newAlpha;
                }
            }
        }

        fogTexture.SetPixels32(fogPixels);
        fogTexture.Apply(false);
    }

    private void UpdateFogUV()
    {
        float worldWidth =
            worldMax.x - worldMin.x;

        float worldDepth =
            worldMax.y - worldMin.y;

        float aspect = minimapCamera.targetTexture != null
            ? (float)minimapCamera.targetTexture.width /
              minimapCamera.targetTexture.height
            : minimapCamera.aspect;

        float halfHeight =
            minimapCamera.orthographicSize;

        float halfWidth =
            halfHeight * aspect;

        Vector3 cameraPosition =
            minimapCamera.transform.position;

        fogRawImage.uvRect = new Rect(
            (cameraPosition.x - halfWidth - worldMin.x) /
            worldWidth,

            (cameraPosition.z - halfHeight - worldMin.y) /
            worldDepth,

            halfWidth * 2f / worldWidth,
            halfHeight * 2f / worldDepth
        );
    }

    private void OnDestroy()
    {
        if (fogTexture != null)
            Destroy(fogTexture);
    }
}