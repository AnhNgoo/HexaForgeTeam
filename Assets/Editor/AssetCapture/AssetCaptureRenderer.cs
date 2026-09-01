#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

// ============================================================
//  AssetCaptureRenderer.cs  –  HexaForge 3D to 2D Asset Capture
//
//  v2 – uses PreviewRenderUtility for URP-compatible rendering.
//
//  Root cause of blank preview: EditorSceneManager.NewPreviewScene()
//  cameras are NOT rendered by URP when calling camera.Render()
//  directly.  PreviewRenderUtility has internal Unity plumbing that
//  properly triggers the SRP render pipeline.
//
//  Architecture:
//    • PRU manages its own isolated preview scene automatically
//    • PRU.lights[0/1] = Main / Fill  (built-in)
//    • rimLightGO added manually to PRU's scene = Rim
//    • DrawPreview() must be called inside OnGUI (Repaint event)
//    • RenderToTexture2D() uses BeginStaticPreview for export
// ============================================================

namespace HexaForge.AssetCapture
{
    public class AssetCaptureRenderer : IDisposable
    {
        // ── PreviewRenderUtility ──────────────────────────────────────

        private PreviewRenderUtility _pru;

        // ── Preview scene objects ─────────────────────────────────────

        private GameObject _prefabInstance;
        private GameObject _rimLightGO;
        private Light      _rimLight;

        // ── State ─────────────────────────────────────────────────────

        private bool _isInitialized;
        private bool _needsRender = true;

        public bool      IsDirty        => _needsRender;
        public bool      HasPrefab      => _prefabInstance != null;
        public GameObject PrefabInstance => _prefabInstance;

        // ── Setup / Teardown ──────────────────────────────────────────

        /// <summary>Initialise the PreviewRenderUtility and rim light.</summary>
        public void Initialize()
        {
            if (_isInitialized) return;

            // true = render full scene in the preview scene
            // true = allow HDR (good for URP)
            _pru = new PreviewRenderUtility(true, true);
            _pru.camera.nearClipPlane = 0.01f;
            _pru.camera.farClipPlane  = 2000f;
            _pru.camera.clearFlags    = CameraClearFlags.SolidColor;
            _pru.camera.backgroundColor = new Color(0, 0, 0, 0);

            // PRU provides _pru.lights[0] and [1] (main & fill).
            // Add a 3rd rim light manually in PRU's own scene.
            Scene pruScene = _pru.camera.gameObject.scene;
            _rimLightGO = new GameObject("AC_RimLight") { hideFlags = HideFlags.HideAndDontSave };
            SceneManager.MoveGameObjectToScene(_rimLightGO, pruScene);
            _rimLight         = _rimLightGO.AddComponent<Light>();
            _rimLight.type    = LightType.Directional;
            _rimLight.enabled = false;

            _isInitialized = true;
            _needsRender   = true;
        }

        /// <summary>Destroy all preview resources. Safe to call multiple times.</summary>
        public void Cleanup()
        {
            UnloadPrefab();

            if (_rimLightGO != null)
            {
                UnityEngine.Object.DestroyImmediate(_rimLightGO);
                _rimLightGO = null;
                _rimLight   = null;
            }

            _pru?.Cleanup();   // closes PRU's internal preview scene
            _pru = null;

            AssetCaptureImageAdjust.Cleanup();
            _isInitialized = false;
        }

        public void Dispose() => Cleanup();

        // ── Prefab Management ─────────────────────────────────────────

        /// <summary>
        /// Instantiates <paramref name="prefab"/> into the PRU preview scene.
        /// The original prefab is never modified.
        /// </summary>
        public void LoadPrefab(GameObject prefab,
                               ACObjectSettings     objSettings,
                               ACAutoFramingSettings framing,
                               AssetCaptureCameraController camCtrl)
        {
            if (!_isInitialized) Initialize();

            UnloadPrefab();
            if (prefab == null) return;

            _prefabInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            _prefabInstance.hideFlags = HideFlags.HideAndDontSave;
            SceneManager.MoveGameObjectToScene(_prefabInstance, _pru.camera.gameObject.scene);

            ApplyObjectTransform(objSettings);

            // Auto-framing: calculate bounds AFTER transform applied
            if (framing != null && (framing.autoCenter || framing.autoFit))
            {
                Bounds bounds = AssetCaptureUtility.CalculateBounds(_prefabInstance);
                if (framing.autoCenter) camCtrl.AutoCenter(bounds);
                if (framing.autoFit)    camCtrl.FitToFrame(bounds, framing.padding, 1f);
            }

            MarkDirty();
        }

        /// <summary>Destroys the current prefab instance. Original asset is untouched.</summary>
        public void UnloadPrefab()
        {
            if (_prefabInstance != null)
            {
                UnityEngine.Object.DestroyImmediate(_prefabInstance);
                _prefabInstance = null;
            }
        }

        /// <summary>Applies position/rotation/scale to the loaded prefab instance.</summary>
        public void ApplyObjectTransform(ACObjectSettings s)
        {
            if (_prefabInstance == null || s == null) return;
            _prefabInstance.transform.localPosition    = s.position;
            _prefabInstance.transform.localEulerAngles = s.eulerRotation;
            _prefabInstance.transform.localScale       = s.scale;
            MarkDirty();
        }

        public void MarkDirty() => _needsRender = true;

        // ── Preview Rendering ─────────────────────────────────────────

        /// <summary>
        /// Renders the preview scene and draws it into <paramref name="rect"/>.
        ///
        /// MUST be called inside an OnGUI context during EventType.Repaint.
        /// PreviewRenderUtility.BeginPreview / EndPreview handle URP rendering
        /// correctly; do not call camera.Render() directly for URP projects.
        /// </summary>
        public void DrawPreview(Rect rect,
                                ACCameraSettings      cam,
                                ACBackgroundSettings  bg,
                                ACLightingSettings    lighting,
                                ACImageAdjustSettings adjust)
        {
            if (!_isInitialized || _pru == null) return;
            if (Event.current.type != EventType.Repaint) return;

            int w = Mathf.Max(1, Mathf.RoundToInt(rect.width));
            int h = Mathf.Max(1, Mathf.RoundToInt(rect.height));

            // 1. Open preview context
            _pru.BeginPreview(rect, GUIStyle.none);

            // 2. Configure camera, lights, ambient
            ConfigureCamera(_pru.camera, cam, bg);
            ConfigureLights(lighting);
            _pru.ambientColor = lighting != null
                ? lighting.ambientColor * lighting.ambientIntensity
                : Color.black;

            // 3. Render (PRU internally triggers URP SRP pipeline)
            _pru.Render();

            // 4. Draw result
            bool needsAdjust = adjust != null && !adjust.IsIdentity;
            if (needsAdjust)
            {
                // Intercept the rendered RT, apply adjustment, draw manually
                RenderTexture srcRT = _pru.camera.targetTexture;
                if (srcRT != null)
                {
                    var adjRT = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.ARGB32);
                    AssetCaptureImageAdjust.Apply(srcRT, adjRT, adjust);
                    _pru.EndPreview();
                    GUI.DrawTexture(rect, adjRT, ScaleMode.StretchToFill, true);
                    RenderTexture.ReleaseTemporary(adjRT);
                }
                else
                {
                    _pru.EndAndDrawPreview(rect);
                }
            }
            else
            {
                // Standard path: EndAndDrawPreview handles alpha-blending
                _pru.EndAndDrawPreview(rect);
            }

            _needsRender = false;
        }

        // ── Export Rendering ──────────────────────────────────────────

        /// <summary>
        /// Renders at the export resolution and returns a new Texture2D.
        /// Can be called outside of OnGUI.
        /// Caller must DestroyImmediate the returned texture when done.
        /// </summary>
        public Texture2D RenderToTexture2D(ACCameraSettings      cam,
                                            ACBackgroundSettings  bg,
                                            ACLightingSettings    lighting,
                                            ACImageAdjustSettings adjust,
                                            int width, int height,
                                            bool transparent)
        {
            if (!_isInitialized || _pru == null) return null;

            // ── IMPORTANT ──────────────────────────────────────────────
            // Do NOT use BeginStaticPreview / EndStaticPreview.
            // EndStaticPreview() composites the result onto a solid grey
            // background, which destroys the alpha channel.
            //
            // Instead: BeginPreview(GUIStyle.none) creates an ARGB32 RT,
            // we read pixels manually before EndPreview() to keep alpha.
            // ──────────────────────────────────────────────────────────

            _pru.BeginPreview(new Rect(0, 0, width, height), GUIStyle.none);

            // Force transparent clear when exporting with transparency
            ACBackgroundSettings exportBg = bg;
            if (transparent)
            {
                exportBg = new ACBackgroundSettings { transparentBackground = true };
            }

            ConfigureCamera(_pru.camera, cam, exportBg);
            ConfigureLights(lighting);
            _pru.ambientColor = lighting != null
                ? lighting.ambientColor * lighting.ambientIntensity
                : Color.black;

            _pru.Render();

            // ── Read pixels from the camera RT (before EndPreview resets it) ──
            RenderTexture srcRT = _pru.camera.targetTexture;
            Texture2D result = null;

            if (srcRT != null)
            {
                // Apply image adjustment to a temp RT if needed
                RenderTexture finalRT  = srcRT;
                RenderTexture tempRT   = null;

                if (adjust != null && !adjust.IsIdentity)
                {
                    tempRT  = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
                    AssetCaptureImageAdjust.Apply(srcRT, tempRT, adjust);
                    finalRT = tempRT;
                }

                // Read pixels (preserves alpha for transparent exports)
                RenderTexture prevActive = RenderTexture.active;
                RenderTexture.active = finalRT;

                TextureFormat fmt = transparent ? TextureFormat.ARGB32 : TextureFormat.RGB24;
                result = new Texture2D(width, height, fmt, false);
                result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                result.Apply();

                RenderTexture.active = prevActive;

                if (tempRT != null)
                    RenderTexture.ReleaseTemporary(tempRT);
            }

            _pru.EndPreview();

            return result;
        }


        // ── Private Helpers ───────────────────────────────────────────

        private static void ConfigureCamera(Camera cam, ACCameraSettings s, ACBackgroundSettings bg)
        {
            if (cam == null || s == null) return;

            cam.orthographic     = s.projection == ACProjection.Orthographic;
            cam.fieldOfView      = s.fieldOfView;
            cam.orthographicSize = s.orthographicSize;
            cam.nearClipPlane    = 0.01f;
            cam.farClipPlane     = 2000f;
            cam.transform.position    = s.position;
            cam.transform.eulerAngles = s.eulerRotation;

            cam.clearFlags = CameraClearFlags.SolidColor;
            if (bg != null)
            {
                cam.backgroundColor = bg.transparentBackground
                    ? new Color(0f, 0f, 0f, 0f)
                    : new Color(bg.backgroundColor.r, bg.backgroundColor.g,
                                bg.backgroundColor.b, 1f);
            }
            else
            {
                cam.backgroundColor = new Color(0f, 0f, 0f, 0f);
            }
        }

        private void ConfigureLights(ACLightingSettings s)
        {
            if (_pru == null || s == null) return;

            // Built-in PRU lights
            if (_pru.lights.Length > 0) ApplyToPRULight(_pru.lights[0], s.mainLight);
            if (_pru.lights.Length > 1) ApplyToPRULight(_pru.lights[1], s.fillLight);

            // Manual rim light
            if (_rimLight != null && _rimLightGO != null)
            {
                _rimLightGO.SetActive(s.rimLight.enabled);
                if (s.rimLight.enabled)
                {
                    _rimLight.color               = s.rimLight.color;
                    _rimLight.intensity           = s.rimLight.intensity;
                    _rimLightGO.transform.eulerAngles = s.rimLight.rotation;
                }
            }
        }

        private static void ApplyToPRULight(Light light, ACLightSettings s)
        {
            if (light == null || s == null) return;
            light.enabled = s.enabled;
            if (!s.enabled) return;
            light.type               = LightType.Directional;
            light.color              = s.color;
            light.intensity          = s.intensity;
            light.transform.eulerAngles = s.rotation;
        }
    }
}
#endif
