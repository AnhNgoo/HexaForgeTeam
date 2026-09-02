#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// ============================================================
//  AssetCaptureRenderer.cs  -  HexaForge 3D to 2D Asset Capture
//
//  v3 - Active Scene approach (fixes URP pink materials).
//
//  ROOT CAUSE of pink materials:
//    PreviewRenderUtility creates an isolated "preview scene".
//    URP 14.x (Unity 2022.3) does NOT render cameras in preview
//    scenes properly via camera.Render() - materials appear pink.
//
//  FIX:
//    Create the capture camera and all objects in the ACTIVE scene
//    (not a preview scene) with HideFlags.HideAndDontSave.
//    URP always processes cameras that live in the active scene,
//    so URP shaders and Shader Graph materials render correctly.
//
//  ISOLATION:
//    - All capture objects are moved to Layer 31 (rarely used).
//    - Camera cullingMask = 1 << 31  (only sees capture objects).
//    - During RenderIsolated(): scene lights and cameras are
//      temporarily patched to EXCLUDE layer 31, then restored.
//    - SceneVisibilityManager hides the prefab from Scene view.
// ============================================================

namespace HexaForge.AssetCapture
{
    public class AssetCaptureRenderer : IDisposable
    {
        // Layer used for capture objects. Layer 31 is almost always
        // unused in game projects. If yours is occupied we fall back
        // to scanning for any free layer in Initialize().
        private const int DEFAULT_CAPTURE_LAYER = 31;
        private int _captureLayer = DEFAULT_CAPTURE_LAYER;

        // ── Scene objects (active scene, HideAndDontSave) ─────────────

        private GameObject _cameraGO;
        private Camera     _camera;

        private GameObject _mainLightGO, _fillLightGO, _rimLightGO;
        private Light      _mainLight,   _fillLight,   _rimLight;

        private GameObject _prefabInstance;

        // ── Render Textures ───────────────────────────────────────────

        private RenderTexture _previewRT;
        private RenderTexture _adjustedRT;

        // ── State ─────────────────────────────────────────────────────

        private bool _isInitialized;
        private bool _needsRender = true;

        public bool       IsDirty        => _needsRender;
        public bool       HasPrefab      => _prefabInstance != null;
        public GameObject PrefabInstance => _prefabInstance;

        // ── Init / Cleanup ────────────────────────────────────────────

        public void Initialize()
        {
            if (_isInitialized) return;

            // Find a free layer for capture isolation
            _captureLayer = FindFreeLayer();

            // Camera lives in the ACTIVE scene so URP recognises it
            _cameraGO = new GameObject("__AC_Camera") { hideFlags = HideFlags.HideAndDontSave };
            _camera = _cameraGO.AddComponent<Camera>();
            _camera.enabled         = false;
            _camera.cullingMask     = 1 << _captureLayer;
            _camera.clearFlags      = CameraClearFlags.SolidColor;
            _camera.backgroundColor = Color.clear;
            _camera.nearClipPlane   = 0.01f;
            _camera.farClipPlane    = 2000f;

            // Add UniversalAdditionalCameraData so URP uses the correct renderer
            EnsureUrpCameraData(_camera);

            // 3 directional lights, all on _captureLayer only
            _mainLightGO = CreateDirectionalLight("__AC_MainLight", _captureLayer, out _mainLight);
            _fillLightGO = CreateDirectionalLight("__AC_FillLight", _captureLayer, out _fillLight);
            _rimLightGO  = CreateDirectionalLight("__AC_RimLight",  _captureLayer, out _rimLight);

            _isInitialized = true;
            _needsRender   = true;
        }

        public void Cleanup()
        {
            UnloadPrefab();

            DestroyGO(ref _rimLightGO);
            DestroyGO(ref _fillLightGO);
            DestroyGO(ref _mainLightGO);
            _rimLight = _fillLight = _mainLight = null;

            DestroyGO(ref _cameraGO);
            _camera = null;

            ReleaseRT(ref _previewRT);
            ReleaseRT(ref _adjustedRT);

            AssetCaptureImageAdjust.Cleanup();
            _isInitialized = false;
        }

        public void Dispose() => Cleanup();

        // ── Prefab ────────────────────────────────────────────────────

        public void LoadPrefab(GameObject prefab,
                               ACObjectSettings      objSettings,
                               ACAutoFramingSettings framing,
                               AssetCaptureCameraController camCtrl)
        {
            if (!_isInitialized) Initialize();
            UnloadPrefab();
            if (prefab == null) return;

            _prefabInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            _prefabInstance.hideFlags = HideFlags.HideAndDontSave;

            // Move to capture layer so ONLY our camera sees it
            SetLayerRecursively(_prefabInstance, _captureLayer);

            // Hide from Scene view viewport to avoid visual clutter
            SceneVisibilityManager.instance.Hide(_prefabInstance, true);

            ApplyObjectTransform(objSettings);

            if (framing != null && (framing.autoCenter || framing.autoFit))
            {
                Bounds bounds = AssetCaptureUtility.CalculateBounds(_prefabInstance);
                if (framing.autoCenter) camCtrl.AutoCenter(bounds);
                if (framing.autoFit)    camCtrl.FitToFrame(bounds, framing.padding, 1f);
            }

            MarkDirty();
        }

        public void UnloadPrefab()
        {
            if (_prefabInstance == null) return;
            try { SceneVisibilityManager.instance.Show(_prefabInstance, true); } catch { }
            UnityEngine.Object.DestroyImmediate(_prefabInstance);
            _prefabInstance = null;
        }

        public void ApplyObjectTransform(ACObjectSettings s)
        {
            if (_prefabInstance == null || s == null) return;
            _prefabInstance.transform.localPosition    = s.position;
            _prefabInstance.transform.localEulerAngles = s.eulerRotation;
            _prefabInstance.transform.localScale       = s.scale;
            MarkDirty();
        }

        public void MarkDirty() => _needsRender = true;

        // ── Preview (called from OnGUI / Repaint) ─────────────────────

        public void DrawPreview(Rect rect,
                                ACCameraSettings      cam,
                                ACBackgroundSettings  bg,
                                ACLightingSettings    lighting,
                                ACImageAdjustSettings adjust)
        {
            if (!_isInitialized || _camera == null) return;
            if (Event.current.type != EventType.Repaint) return;

            int w = Mathf.Max(1, (int)rect.width);
            int h = Mathf.Max(1, (int)rect.height);

            _previewRT = EnsureRT(_previewRT, w, h);
            ConfigureCamera(_camera, cam, bg);
            ConfigureLights(lighting);

            // Render into _previewRT with full scene isolation
            RenderIsolated(_previewRT);

            // Apply image adjustment
            RenderTexture displayRT = _previewRT;
            if (adjust != null && !adjust.IsIdentity)
            {
                _adjustedRT = EnsureRT(_adjustedRT, w, h);
                AssetCaptureImageAdjust.Apply(_previewRT, _adjustedRT, adjust);
                displayRT = _adjustedRT;
            }

            GUI.DrawTexture(rect, displayRT, ScaleMode.StretchToFill, true);
            _needsRender = false;
        }

        // ── Export ────────────────────────────────────────────────────

        public Texture2D RenderToTexture2D(ACCameraSettings      cam,
                                            ACBackgroundSettings  bg,
                                            ACLightingSettings    lighting,
                                            ACImageAdjustSettings adjust,
                                            int width, int height,
                                            bool transparent)
        {
            if (!_isInitialized || _camera == null) return null;

            var exportRT = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            exportRT.Create();

            ACBackgroundSettings exportBg = transparent
                ? new ACBackgroundSettings { transparentBackground = true }
                : bg;

            ConfigureCamera(_camera, cam, exportBg);
            ConfigureLights(lighting);
            RenderIsolated(exportRT);

            // Apply adjustment
            RenderTexture finalRT = exportRT;
            RenderTexture tempRT  = null;
            if (adjust != null && !adjust.IsIdentity)
            {
                tempRT  = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
                AssetCaptureImageAdjust.Apply(exportRT, tempRT, adjust);
                finalRT = tempRT;
            }

            // Read pixels (preserves alpha)
            var prevActive = RenderTexture.active;
            RenderTexture.active = finalRT;
            TextureFormat fmt = transparent ? TextureFormat.ARGB32 : TextureFormat.RGB24;
            var result = new Texture2D(width, height, fmt, false);
            result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            result.Apply();
            RenderTexture.active = prevActive;

            if (tempRT != null) RenderTexture.ReleaseTemporary(tempRT);
            exportRT.Release();
            UnityEngine.Object.DestroyImmediate(exportRT);

            return result;
        }

        // ── Isolated Render ───────────────────────────────────────────

        /// <summary>
        /// Renders _camera to <paramref name="target"/> while:
        ///   1. Scene lights temporarily exclude _captureLayer (no bleed-in).
        ///   2. Scene cameras temporarily exclude _captureLayer (no bleed-out).
        /// All masks are restored after rendering.
        /// </summary>
        private void RenderIsolated(RenderTexture target)
        {
            int captureBit = 1 << _captureLayer;

            // ── Patch scene lights ────────────────────────────────────
            var allLights       = UnityEngine.Object.FindObjectsOfType<Light>();
            var savedLightMasks = new Dictionary<Light, int>(allLights.Length);
            foreach (var l in allLights)
            {
                if (l == _mainLight || l == _fillLight || l == _rimLight) continue;
                savedLightMasks[l] = l.cullingMask;
                l.cullingMask = l.cullingMask & ~captureBit;
            }

            // ── Patch scene cameras ───────────────────────────────────
            var allCameras    = UnityEngine.Object.FindObjectsOfType<Camera>();
            var savedCamMasks = new Dictionary<Camera, int>(allCameras.Length);
            foreach (var c in allCameras)
            {
                if (c == _camera) continue;
                savedCamMasks[c] = c.cullingMask;
                c.cullingMask = c.cullingMask & ~captureBit;
            }

            // ── Render ────────────────────────────────────────────────
            _camera.targetTexture = target;
            _camera.Render();   // URP processes this correctly (camera in active scene)
            _camera.targetTexture = null;

            // ── Restore ───────────────────────────────────────────────
            foreach (var kvp in savedLightMasks)
                if (kvp.Key != null) kvp.Key.cullingMask = kvp.Value;

            foreach (var kvp in savedCamMasks)
                if (kvp.Key != null) kvp.Key.cullingMask = kvp.Value;
        }

        // ── Private Helpers ───────────────────────────────────────────

        private static int FindFreeLayer()
        {
            for (int i = 31; i >= 8; i--)
                if (string.IsNullOrEmpty(LayerMask.LayerToName(i)))
                    return i;
            Debug.LogWarning("[AssetCapture] No free layer found. Defaulting to layer 31.");
            return DEFAULT_CAPTURE_LAYER;
        }

        private static GameObject CreateDirectionalLight(string name, int layer, out Light light)
        {
            var go = new GameObject(name) { hideFlags = HideFlags.HideAndDontSave };
            light             = go.AddComponent<Light>();
            light.type        = LightType.Directional;
            light.cullingMask = 1 << layer;
            light.shadows     = LightShadows.None;
            light.enabled     = false;
            return go;
        }

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
            cam.backgroundColor = (bg != null && !bg.transparentBackground)
                ? new Color(bg.backgroundColor.r, bg.backgroundColor.g, bg.backgroundColor.b, 1f)
                : Color.clear;
        }

        private void ConfigureLights(ACLightingSettings s)
        {
            if (s == null) return;
            ApplyLightSettings(_mainLight, _mainLightGO, s.mainLight);
            ApplyLightSettings(_fillLight, _fillLightGO, s.fillLight);
            ApplyLightSettings(_rimLight,  _rimLightGO,  s.rimLight);
        }

        private void ApplyLightSettings(Light light, GameObject go, ACLightSettings s)
        {
            if (light == null || go == null || s == null) return;
            go.SetActive(s.enabled);
            if (!s.enabled) return;
            light.color              = s.color;
            light.intensity          = s.intensity;
            light.shadows            = LightShadows.None;
            light.cullingMask        = 1 << _captureLayer;
            go.transform.eulerAngles = s.rotation;
        }

        private static void EnsureUrpCameraData(Camera cam)
        {
            if (cam == null) return;
            Type urpDataType =
                Type.GetType("UnityEngine.Rendering.Universal.UniversalAdditionalCameraData, Unity.RenderPipelines.Universal.Runtime") ??
                Type.GetType("UnityEngine.Rendering.Universal.UniversalAdditionalCameraData, UnityEngine.Rendering.Universal");
            if (urpDataType == null) return;

            Component cameraData = cam.gameObject.GetComponent(urpDataType);
            if (cameraData == null)
                cameraData = cam.gameObject.AddComponent(urpDataType);
            if (cameraData == null) return;

            try
            {
                urpDataType.GetProperty("renderType")?.SetValue(cameraData, 0);           // Base
                urpDataType.GetProperty("renderPostProcessing")?.SetValue(cameraData, false);
                urpDataType.GetProperty("antialiasing")?.SetValue(cameraData, 0);         // None
                urpDataType.GetProperty("depthPrimingMode")?.SetValue(cameraData, 0);     // Disabled
                urpDataType.GetProperty("stopNaN")?.SetValue(cameraData, false);
                urpDataType.GetProperty("dithering")?.SetValue(cameraData, false);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning("[AssetCapture] URP camera config: " + ex.Message);
            }
        }

        private static void SetLayerRecursively(GameObject go, int layer)
        {
            go.layer = layer;
            foreach (Transform child in go.transform)
                SetLayerRecursively(child.gameObject, layer);
        }

        private static RenderTexture EnsureRT(RenderTexture rt, int w, int h)
        {
            if (rt != null && rt.IsCreated() && rt.width == w && rt.height == h) return rt;
            if (rt != null) { rt.Release(); UnityEngine.Object.DestroyImmediate(rt); }
            var newRT = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32);
            newRT.Create();
            return newRT;
        }

        private static void ReleaseRT(ref RenderTexture rt)
        {
            if (rt == null) return;
            rt.Release();
            UnityEngine.Object.DestroyImmediate(rt);
            rt = null;
        }

        private static void DestroyGO(ref GameObject go)
        {
            if (go == null) return;
            UnityEngine.Object.DestroyImmediate(go);
            go = null;
        }
    }
}
#endif
