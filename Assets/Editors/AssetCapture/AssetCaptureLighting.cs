#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.SceneManagement;

// ============================================================
//  AssetCaptureLighting.cs  –  HexaForge 3D to 2D Asset Capture
//
//  Creates and manages up to 3 directional lights
//  (Main / Fill / Rim) inside the preview scene.
//  Does NOT modify any scene object or global RenderSettings.
// ============================================================

namespace HexaForge.AssetCapture
{
    public class AssetCaptureLighting
    {
        // ── Private state ─────────────────────────────────────────────

        private GameObject _mainGO, _fillGO, _rimGO;
        private Light      _mainL,  _fillL,  _rimL;

        private bool _isInitialized;

        // ── Public ────────────────────────────────────────────────────

        public bool IsInitialized => _isInitialized;

        /// <summary>
        /// Creates three directional lights and moves them into
        /// the given <paramref name="targetScene"/> (a preview scene).
        /// Call once per session; call Cleanup() to destroy.
        /// </summary>
        public void Initialize(Scene targetScene)
        {
            Cleanup(); // ensure clean state

            _mainGO = CreateLight("AC_Main_Light", targetScene, out _mainL);
            _fillGO = CreateLight("AC_Fill_Light", targetScene, out _fillL);
            _rimGO  = CreateLight("AC_Rim_Light",  targetScene, out _rimL);

            _isInitialized = true;
        }

        /// <summary>
        /// Applies the given lighting settings to the three lights.
        /// Also sets the scene's ambient colour.
        /// </summary>
        public void ApplySettings(ACLightingSettings s)
        {
            if (!_isInitialized || s == null) return;

            ApplyToLight(_mainGO, _mainL, s.mainLight);
            ApplyToLight(_fillGO, _fillL, s.fillLight);
            ApplyToLight(_rimGO,  _rimL,  s.rimLight);
        }

        /// <summary>Destroys all light GameObjects. Safe to call multiple times.</summary>
        public void Cleanup()
        {
            SafeDestroy(ref _mainGO);
            SafeDestroy(ref _fillGO);
            SafeDestroy(ref _rimGO);
            _mainL = _fillL = _rimL = null;
            _isInitialized = false;
        }

        // ── Private helpers ───────────────────────────────────────────

        private static GameObject CreateLight(string name, Scene scene, out Light light)
        {
            var go = new GameObject(name) { hideFlags = HideFlags.HideAndDontSave };

            // Move into preview scene so it doesn't pollute the main scene
            if (scene.IsValid())
                SceneManager.MoveGameObjectToScene(go, scene);

            light           = go.AddComponent<Light>();
            light.type      = LightType.Directional;
            light.shadows   = LightShadows.None; // shadows handled separately
            return go;
        }

        private static void ApplyToLight(GameObject go, Light light, ACLightSettings s)
        {
            if (go == null || light == null || s == null) return;
            go.SetActive(s.enabled);
            if (!s.enabled) return;

            light.color        = s.color;
            light.intensity    = s.intensity;
            go.transform.eulerAngles = s.rotation;
        }

        private static void SafeDestroy(ref GameObject go)
        {
            if (go != null)
            {
                Object.DestroyImmediate(go);
                go = null;
            }
        }
    }
}
#endif
