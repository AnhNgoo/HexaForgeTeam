#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

// ============================================================
//  AssetCaptureImageAdjust.cs  –  HexaForge 3D to 2D Asset Capture
//
//  Manages the image-adjustment Material (loaded from shader)
//  and applies it via Graphics.Blit onto a destination RT.
// ============================================================

namespace HexaForge.AssetCapture
{
    public static class AssetCaptureImageAdjust
    {
        private const string SHADER_NAME = "Hidden/HexaForge/ImageAdjust";

        private static Material _mat;

        // Shader property IDs (cached for performance)
        private static readonly int ID_Brightness = Shader.PropertyToID("_Brightness");
        private static readonly int ID_Contrast   = Shader.PropertyToID("_Contrast");
        private static readonly int ID_Saturation = Shader.PropertyToID("_Saturation");
        private static readonly int ID_Exposure   = Shader.PropertyToID("_Exposure");
        private static readonly int ID_Gamma      = Shader.PropertyToID("_Gamma");

        // ── Public API ────────────────────────────────────────────────

        /// <summary>
        /// Applies brightness/contrast/saturation/exposure/gamma adjustments
        /// by blitting from <paramref name="src"/> to <paramref name="dst"/>.
        /// If the shader is unavailable, falls back to a plain copy.
        /// </summary>
        public static void Apply(RenderTexture src, RenderTexture dst,
                                 ACImageAdjustSettings settings)
        {
            if (settings == null || settings.IsIdentity)
            {
                // No adjustments needed – plain copy
                Graphics.Blit(src, dst);
                return;
            }

            Material mat = GetOrCreateMaterial();
            if (mat == null)
            {
                Debug.LogWarning("[AssetCapture] ImageAdjust shader not found. " +
                                 "Using plain copy. Shader: " + SHADER_NAME);
                Graphics.Blit(src, dst);
                return;
            }

            mat.SetFloat(ID_Brightness, settings.brightness);
            mat.SetFloat(ID_Contrast,   settings.contrast);
            mat.SetFloat(ID_Saturation, settings.saturation);
            mat.SetFloat(ID_Exposure,   settings.exposure);
            mat.SetFloat(ID_Gamma,      settings.gamma);

            Graphics.Blit(src, dst, mat);
        }

        /// <summary>
        /// Applies adjustments in-place (src is used as both input and output
        /// via an intermediate RenderTexture).
        /// </summary>
        public static void ApplyInPlace(ref RenderTexture rt, ACImageAdjustSettings settings)
        {
            if (settings == null || settings.IsIdentity) return;

            var tmp = RenderTexture.GetTemporary(rt.width, rt.height, 0,
                                                  RenderTextureFormat.ARGB32);
            Apply(rt, tmp, settings);

            // Copy result back into rt
            Graphics.Blit(tmp, rt);
            RenderTexture.ReleaseTemporary(tmp);
        }

        /// <summary>
        /// Applies adjustments to a Texture2D by going through RenderTextures.
        /// Returns a new Texture2D with adjustments applied.
        /// The caller is responsible for destroying the returned texture.
        /// </summary>
        public static Texture2D ApplyToTexture2D(Texture2D source, ACImageAdjustSettings settings)
        {
            if (source == null) return null;

            // Upload source to GPU
            var srcRT = RenderTexture.GetTemporary(source.width, source.height, 0,
                                                    RenderTextureFormat.ARGB32);
            var dstRT = RenderTexture.GetTemporary(source.width, source.height, 0,
                                                    RenderTextureFormat.ARGB32);
            Graphics.Blit(source, srcRT);
            Apply(srcRT, dstRT, settings);

            // Read back to Texture2D
            var prev = RenderTexture.active;
            RenderTexture.active = dstRT;
            var result = new Texture2D(source.width, source.height, TextureFormat.ARGB32, false);
            result.ReadPixels(new Rect(0, 0, dstRT.width, dstRT.height), 0, 0);
            result.Apply();
            RenderTexture.active = prev;

            RenderTexture.ReleaseTemporary(srcRT);
            RenderTexture.ReleaseTemporary(dstRT);
            return result;
        }

        /// <summary>Release the cached material (call on tool cleanup).</summary>
        public static void Cleanup()
        {
            if (_mat != null)
            {
                Object.DestroyImmediate(_mat);
                _mat = null;
            }
        }

        // ── Internal ─────────────────────────────────────────────────

        private static Material GetOrCreateMaterial()
        {
            if (_mat != null) return _mat;

            Shader s = Shader.Find(SHADER_NAME);
            if (s == null) return null;

            _mat = new Material(s) { hideFlags = HideFlags.HideAndDontSave };
            return _mat;
        }
    }
}
#endif
