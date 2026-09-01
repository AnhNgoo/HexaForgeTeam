#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

// ============================================================
//  AssetCaptureUtility.cs  –  HexaForge 3D to 2D Asset Capture
//  Static helpers: bounds, auto-framing, file paths, IMGUI.
// ============================================================

namespace HexaForge.AssetCapture
{
    public static class AssetCaptureUtility
    {
        // ── Bounds ───────────────────────────────────────────────────

        /// <summary>
        /// Calculates the combined world-space Bounds of all Renderer
        /// components (MeshRenderer + SkinnedMeshRenderer) in the hierarchy.
        /// Falls back to a unit-cube centred on the root if no renderers found.
        /// </summary>
        public static Bounds CalculateBounds(GameObject root)
        {
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
                return new Bounds(root.transform.position, Vector3.one);

            Bounds b = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                b.Encapsulate(renderers[i].bounds);
            return b;
        }

        // ── Auto-framing ─────────────────────────────────────────────

        /// <summary>
        /// Returns the orthographic half-height needed so the object fits
        /// inside the frame with the given padding fraction (0 = tight fit).
        /// </summary>
        public static float FitOrthographicSize(Bounds b, float aspectRatio, float padding)
        {
            float halfW = b.extents.x / Mathf.Max(aspectRatio, 0.001f);
            float halfH = b.extents.y;
            float halfD = b.extents.z;
            // Camera looks in -Z by default; take the max of X,Y extents.
            float size  = Mathf.Max(halfW, halfH, halfD);
            return size * (1f + padding * 2f);
        }

        /// <summary>
        /// Returns the camera distance so the object's largest extent
        /// fills the frame with the given FOV and padding.
        /// </summary>
        public static float FitPerspectiveDistance(Bounds b, float fovDegrees, float aspectRatio, float padding)
        {
            float maxExtent = Mathf.Max(b.extents.x, b.extents.y, b.extents.z);
            float halfFov   = fovDegrees * Mathf.Deg2Rad * 0.5f;
            if (halfFov < 0.001f) halfFov = 0.001f;
            float dist = maxExtent / Mathf.Tan(halfFov);
            return dist * (1f + padding);
        }

        // ── File Path Helpers ─────────────────────────────────────────

        /// <summary>
        /// Returns an absolute system path from a Unity project-relative path
        /// (e.g. "Assets/2DAssetItem" → "D:/Project/Assets/2DAssetItem").
        /// </summary>
        public static string ProjectRelativeToAbsolute(string relativePath)
        {
            return Path.Combine(Application.dataPath, "..", relativePath)
                       .Replace('\\', '/');
        }

        /// <summary>
        /// Ensures the directory exists on disk (creates it if missing).
        /// Also creates required .meta files via AssetDatabase.
        /// </summary>
        public static void EnsureDirectoryExists(string absolutePath)
        {
            if (!Directory.Exists(absolutePath))
                Directory.CreateDirectory(absolutePath);
        }

        /// <summary>
        /// If a file already exists and overwrite is false, appends a number
        /// suffix to produce a unique path (e.g. Sword_1.png, Sword_2.png …).
        /// </summary>
        public static string ResolveFilePath(string directory, string baseName, string ext, bool overwrite)
        {
            string fullPath = Path.Combine(directory, baseName + ext);
            if (overwrite || !File.Exists(fullPath))
                return fullPath;

            int n = 1;
            while (File.Exists(Path.Combine(directory, $"{baseName}_{n}{ext}")))
                n++;
            return Path.Combine(directory, $"{baseName}_{n}{ext}");
        }

        /// <summary>
        /// Converts an absolute system path back to a Unity project-relative
        /// path (i.e. starting with "Assets/…").
        /// Returns null if the path is outside the project.
        /// </summary>
        public static string AbsoluteToProjectRelative(string absolutePath)
        {
            string dataPath = Application.dataPath.Replace('\\', '/');
            absolutePath = absolutePath.Replace('\\', '/');

            string projectRoot = dataPath.Substring(0, dataPath.LastIndexOf("/Assets"));
            if (absolutePath.StartsWith(projectRoot + "/"))
                return absolutePath.Substring(projectRoot.Length + 1);
            return null;
        }

        // ── IMGUI Helpers ─────────────────────────────────────────────

        private static GUIStyle _sectionHeaderStyle;
        private static GUIStyle _boldFoldoutStyle;

        /// <summary>Lazy-initialise reusable IMGUI styles.</summary>
        public static void InitStyles()
        {
            if (_sectionHeaderStyle == null)
            {
                _sectionHeaderStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize  = 11,
                    alignment = TextAnchor.MiddleLeft
                };
            }

            if (_boldFoldoutStyle == null)
            {
                _boldFoldoutStyle = new GUIStyle(EditorStyles.foldout)
                {
                    fontStyle = FontStyle.Bold
                };
            }
        }

        /// <summary>Draws a collapsible section header. Returns new fold state.</summary>
        public static bool DrawFoldout(string title, bool expanded)
        {
            InitStyles();
            EditorGUILayout.Space(2);
            bool result = EditorGUILayout.Foldout(expanded, title, true, _boldFoldoutStyle ?? EditorStyles.foldout);
            return result;
        }

        /// <summary>Draws a thin horizontal separator line.</summary>
        public static void DrawSeparator(float topMargin = 2f, float bottomMargin = 2f)
        {
            GUILayout.Space(topMargin);
            Rect r = EditorGUILayout.GetControlRect(false, 1f);
            EditorGUI.DrawRect(r, new Color(0.5f, 0.5f, 0.5f, 0.4f));
            GUILayout.Space(bottomMargin);
        }

        /// <summary>Begins a visually-boxed section.</summary>
        public static void BeginBox()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Space(2);
        }

        /// <summary>Ends a visually-boxed section.</summary>
        public static void EndBox()
        {
            GUILayout.Space(2);
            EditorGUILayout.EndVertical();
        }

        /// <summary>Draws a Vector3 field with compact XYZ layout.</summary>
        public static Vector3 DrawVector3(string label, Vector3 value)
        {
            return EditorGUILayout.Vector3Field(label, value);
        }

        /// <summary>Draws a full-width button with a consistent height.</summary>
        public static bool DrawButton(string label, float height = 24f, Color? tint = null)
        {
            Color prev = GUI.backgroundColor;
            if (tint.HasValue) GUI.backgroundColor = tint.Value;
            bool pressed = GUILayout.Button(label, GUILayout.Height(height));
            GUI.backgroundColor = prev;
            return pressed;
        }

        /// <summary>Draws two side-by-side buttons.</summary>
        public static bool DrawButtonPair(string labelA, string labelB, out bool pressedB, float height = 24f)
        {
            bool pressedA = false;
            pressedB = false;
            EditorGUILayout.BeginHorizontal();
            pressedA = GUILayout.Button(labelA, GUILayout.Height(height));
            pressedB = GUILayout.Button(labelB, GUILayout.Height(height));
            EditorGUILayout.EndHorizontal();
            return pressedA;
        }

        // ── Render Texture ────────────────────────────────────────────

        /// <summary>
        /// Creates (or recreates) an ARGB32 RenderTexture at the given resolution.
        /// Destroys the old one if dimensions changed.
        /// </summary>
        public static RenderTexture EnsureRenderTexture(ref RenderTexture rt, int w, int h)
        {
            if (rt != null && rt.width == w && rt.height == h)
                return rt;

            if (rt != null)
            {
                rt.Release();
                Object.DestroyImmediate(rt);
            }

            rt = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32)
            {
                antiAliasing    = 1,
                filterMode      = FilterMode.Bilinear,
                useMipMap       = false,
                autoGenerateMips = false,
                name            = "AC_PreviewRT"
            };
            rt.Create();
            return rt;
        }
    }
}
#endif
