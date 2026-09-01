#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

// ============================================================
//  AssetCaptureWindow.cs  –  HexaForge 3D to 2D Asset Capture
//
//  Main EditorWindow.  Menu: Tools > 3D to 2D Asset Capture
//
//  Layout (top-to-bottom, scrollable left panel):
//    ┌──────────────────────────────────────┐
//    │  3D TO 2D ASSET CAPTURE  (title bar) │
//    ├────────────────┬─────────────────────┤
//    │  Left panel    │  Preview (right)    │
//    │  – Prefab      │                     │
//    │  – Camera      │   RenderTexture     │
//    │  – Object      │   (click+drag)      │
//    │  – Background  │                     │
//    │  – Lighting    │  Preview controls   │
//    │  – Image Adj.  │                     │
//    │  – Auto Frame  │                     │
//    │  – Export      │                     │
//    │  – Preset      │                     │
//    │  – Batch       │                     │
//    └────────────────┴─────────────────────┘
// ============================================================

namespace HexaForge.AssetCapture
{
    public class AssetCaptureWindow : EditorWindow
    {
        // ── Menu ──────────────────────────────────────────────────────

        [MenuItem("Tools/3D to 2D Asset Capture", priority = 200)]
        public static void Open()
        {
            var win = GetWindow<AssetCaptureWindow>(false, "3D → 2D Capture", true);
            win.minSize = new Vector2(820, 600);
            win.Show();
        }

        // ── Sub-systems ───────────────────────────────────────────────

        private AssetCaptureRenderer            _renderer;
        private AssetCaptureCameraController    _camCtrl;
        private AssetCaptureBatchExporter       _batch;

        // ── Current Working Settings ──────────────────────────────────
        // Stored flat here so IMGUI can bind directly to them.

        private GameObject          _loadedPrefab;
        private ACCameraSettings    _cam    = new ACCameraSettings();
        private ACObjectSettings    _obj    = new ACObjectSettings();
        private ACBackgroundSettings _bg    = new ACBackgroundSettings();
        private ACLightingSettings   _light = new ACLightingSettings();
        private ACImageAdjustSettings _adj  = new ACImageAdjustSettings();
        private ACExportSettings    _exp    = new ACExportSettings();
        private ACAutoFramingSettings _frame = new ACAutoFramingSettings();

        // ── UI State ──────────────────────────────────────────────────

        private Vector2 _leftScroll;
        private bool _foldCamera  = true;
        private bool _foldObj     = true;
        private bool _foldBg      = true;
        private bool _foldLight   = true;
        private bool _foldAdj     = true;
        private bool _foldFrame   = true;
        private bool _foldExport  = true;
        private bool _foldPreset  = false;
        private bool _foldBatch   = false;

        // lighting sub-folds
        private bool _foldMainL = true, _foldFillL = false, _foldRimL = false;

        // Preview size
        private const float PREVIEW_MIN = 240f;
        private float _previewWidth  = 360f;
        private float _previewHeight = 360f;

        // Preset management
        private AssetCapturePreset _activePreset;
        private string             _presetSavePath = "Assets/Editors/AssetCapture/Presets";

        // ── IMGUI Styles (lazy) ───────────────────────────────────────

        private GUIStyle _titleStyle;
        private GUIStyle _previewBgStyle;

        // ── Unity Callbacks ───────────────────────────────────────────

        private void OnEnable()
        {
            _renderer = new AssetCaptureRenderer();
            _renderer.Initialize();
            _camCtrl  = new AssetCaptureCameraController(_cam);
            _batch    = new AssetCaptureBatchExporter();
            EditorApplication.update += OnEditorUpdate;
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
            _renderer?.Cleanup();
            _renderer = null;
        }

        private void OnEditorUpdate()
        {
            // Repaint every frame if the renderer is dirty
            if (_renderer != null && _renderer.IsDirty)
                Repaint();
        }

        // ── Main GUI ──────────────────────────────────────────────────

        private void OnGUI()
        {
            InitStyles();

            // ── Title bar ────────────────────────────────────────────
            DrawTitleBar();

            // ── Main layout: left panel | preview ───────────────────
            EditorGUILayout.BeginHorizontal();
            {
                // Left – scrollable control panel
                _leftScroll = EditorGUILayout.BeginScrollView(_leftScroll,
                    GUILayout.Width(position.width - _previewWidth - 6),
                    GUILayout.ExpandHeight(true));
                {
                    DrawLeftPanel();
                }
                EditorGUILayout.EndScrollView();

                // Resize handle (a thin column)
                GUILayout.Box("", GUILayout.Width(4), GUILayout.ExpandHeight(true));
                var handleRect = GUILayoutUtility.GetLastRect();
                EditorGUIUtility.AddCursorRect(handleRect, MouseCursor.ResizeHorizontal);
                if (Event.current.type == EventType.MouseDrag &&
                    handleRect.Contains(Event.current.mousePosition))
                {
                    _previewWidth -= Event.current.delta.x;
                    _previewWidth  = Mathf.Clamp(_previewWidth, PREVIEW_MIN, position.width - 320);
                    Repaint();
                    Event.current.Use();
                }

                // Right – preview
                DrawPreviewPanel();
            }
            EditorGUILayout.EndHorizontal();

            // With PreviewRenderUtility, rendering happens inside DrawPreviewPanel()
            // during EventType.Repaint. We just need to ensure a repaint is scheduled.
            if (_renderer != null && _renderer.IsDirty)
                Repaint();
        }

        // ── Title Bar ─────────────────────────────────────────────────

        private void DrawTitleBar()
        {
            Rect titleRect = EditorGUILayout.GetControlRect(false, 36);
            EditorGUI.DrawRect(titleRect, new Color(0.15f, 0.15f, 0.18f, 1f));
            GUI.Label(titleRect, "  3D TO 2D ASSET CAPTURE  –  HexaForge", _titleStyle);
        }

        // ── Left Panel ────────────────────────────────────────────────

        private void DrawLeftPanel()
        {
            EditorGUILayout.Space(4);

            DrawPrefabSection();
            AssetCaptureUtility.DrawSeparator();

            DrawCameraSection();
            AssetCaptureUtility.DrawSeparator();

            DrawObjectSection();
            AssetCaptureUtility.DrawSeparator();

            DrawAutoFramingSection();
            AssetCaptureUtility.DrawSeparator();

            DrawBackgroundSection();
            AssetCaptureUtility.DrawSeparator();

            DrawLightingSection();
            AssetCaptureUtility.DrawSeparator();

            DrawImageAdjustSection();
            AssetCaptureUtility.DrawSeparator();

            DrawExportSection();
            AssetCaptureUtility.DrawSeparator();

            DrawPresetSection();
            AssetCaptureUtility.DrawSeparator();

            _batch.DrawUI(_renderer, BuildPreset());

            EditorGUILayout.Space(8);
        }

        // ── Section: Prefab ───────────────────────────────────────────

        private void DrawPrefabSection()
        {
            EditorGUILayout.LabelField("PREFAB / MODEL", EditorStyles.boldLabel);
            AssetCaptureUtility.BeginBox();

            EditorGUI.BeginChangeCheck();
            var newPrefab = (GameObject)EditorGUILayout.ObjectField(
                "Prefab", _loadedPrefab, typeof(GameObject), false);
            if (EditorGUI.EndChangeCheck() && newPrefab != _loadedPrefab)
            {
                _loadedPrefab = newPrefab;
                LoadPrefab();
            }

            // Drop zone
            Rect dropRect = EditorGUILayout.GetControlRect(false, 36);
            EditorGUI.DrawRect(dropRect, new Color(0.25f, 0.25f, 0.28f, 0.7f));
            GUI.Label(dropRect, "  ↓ Drag & Drop Prefab Here",
                EditorStyles.centeredGreyMiniLabel);
            HandlePrefabDrop(dropRect);

            if (_loadedPrefab != null)
            {
                EditorGUILayout.HelpBox($"Loaded: {_loadedPrefab.name}", MessageType.None);
                if (GUILayout.Button("Unload Prefab", GUILayout.Height(20)))
                {
                    _loadedPrefab = null;
                    _renderer.UnloadPrefab();
                    _exp.fileName = "";
                }
            }

            AssetCaptureUtility.EndBox();
        }

        // ── Section: Camera ───────────────────────────────────────────

        private void DrawCameraSection()
        {
            _foldCamera = AssetCaptureUtility.DrawFoldout("  CAMERA", _foldCamera);
            if (!_foldCamera) return;

            AssetCaptureUtility.BeginBox();
            EditorGUI.BeginChangeCheck();

            // Projection
            _cam.projection = (ACProjection)EditorGUILayout.EnumPopup("Projection", _cam.projection);
            AssetCaptureUtility.DrawSeparator(1, 1);

            // Position
            _cam.position = EditorGUILayout.Vector3Field("Position", _cam.position);

            // Rotation
            _cam.eulerRotation = EditorGUILayout.Vector3Field("Rotation", _cam.eulerRotation);

            // Distance
            _cam.distance = EditorGUILayout.FloatField("Distance", _cam.distance);

            if (_cam.projection == ACProjection.Perspective)
                _cam.fieldOfView = EditorGUILayout.Slider("Field of View", _cam.fieldOfView, 1f, 179f);
            else
                _cam.orthographicSize = EditorGUILayout.FloatField("Ortho Size", _cam.orthographicSize);

            if (EditorGUI.EndChangeCheck())
            {
                _camCtrl.UpdateSettings(_cam);
                MarkDirty();
            }

            EditorGUILayout.Space(2);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Reset Camera", GUILayout.Height(22))) { _camCtrl.ResetCamera(); MarkDirty(); }
            if (GUILayout.Button("Auto Center",  GUILayout.Height(22))) AutoCenter();
            if (GUILayout.Button("Fit To Frame", GUILayout.Height(22))) FitToFrame();
            EditorGUILayout.EndHorizontal();

            AssetCaptureUtility.EndBox();
        }

        // ── Section: Object Transform ─────────────────────────────────

        private void DrawObjectSection()
        {
            _foldObj = AssetCaptureUtility.DrawFoldout("  OBJECT TRANSFORM", _foldObj);
            if (!_foldObj) return;

            AssetCaptureUtility.BeginBox();
            EditorGUI.BeginChangeCheck();

            _obj.position      = EditorGUILayout.Vector3Field("Position", _obj.position);
            _obj.eulerRotation = EditorGUILayout.Vector3Field("Rotation", _obj.eulerRotation);
            _obj.scale         = EditorGUILayout.Vector3Field("Scale",    _obj.scale);

            if (EditorGUI.EndChangeCheck())
            {
                _renderer.ApplyObjectTransform(_obj);
                MarkDirty();
            }

            EditorGUILayout.Space(2);
            if (GUILayout.Button("Reset Transform", GUILayout.Height(22)))
            {
                _obj.position = Vector3.zero;
                _obj.eulerRotation = Vector3.zero;
                _obj.scale    = Vector3.one;
                _renderer.ApplyObjectTransform(_obj);
                MarkDirty();
            }
            AssetCaptureUtility.EndBox();
        }

        // ── Section: Auto Framing ─────────────────────────────────────

        private void DrawAutoFramingSection()
        {
            _foldFrame = AssetCaptureUtility.DrawFoldout("  AUTO FRAMING", _foldFrame);
            if (!_foldFrame) return;

            AssetCaptureUtility.BeginBox();
            EditorGUI.BeginChangeCheck();

            _frame.autoCenter = EditorGUILayout.Toggle("Auto Center", _frame.autoCenter);
            _frame.autoFit    = EditorGUILayout.Toggle("Auto Fit",    _frame.autoFit);
            _frame.padding    = EditorGUILayout.Slider("Padding", _frame.padding, 0f, 0.5f);

            if (EditorGUI.EndChangeCheck()) MarkDirty();

            EditorGUILayout.Space(2);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Apply Auto Center", GUILayout.Height(22))) AutoCenter();
            if (GUILayout.Button("Apply Fit",         GUILayout.Height(22))) FitToFrame();
            EditorGUILayout.EndHorizontal();

            AssetCaptureUtility.EndBox();
        }

        // ── Section: Background ───────────────────────────────────────

        private void DrawBackgroundSection()
        {
            _foldBg = AssetCaptureUtility.DrawFoldout("  BACKGROUND", _foldBg);
            if (!_foldBg) return;

            AssetCaptureUtility.BeginBox();
            EditorGUI.BeginChangeCheck();

            _bg.transparentBackground = EditorGUILayout.Toggle(
                "Transparent Background", _bg.transparentBackground);

            using (new EditorGUI.DisabledGroupScope(_bg.transparentBackground))
                _bg.backgroundColor = EditorGUILayout.ColorField("Background Color", _bg.backgroundColor);

            if (EditorGUI.EndChangeCheck()) MarkDirty();
            AssetCaptureUtility.EndBox();
        }

        // ── Section: Lighting ─────────────────────────────────────────

        private void DrawLightingSection()
        {
            _foldLight = AssetCaptureUtility.DrawFoldout("  LIGHTING", _foldLight);
            if (!_foldLight) return;

            AssetCaptureUtility.BeginBox();
            EditorGUI.BeginChangeCheck();

            DrawLightSettings("Main Light",  ref _light.mainLight,  ref _foldMainL);
            DrawLightSettings("Fill Light",  ref _light.fillLight,  ref _foldFillL);
            DrawLightSettings("Rim Light",   ref _light.rimLight,   ref _foldRimL);

            AssetCaptureUtility.DrawSeparator(1, 1);
            _light.ambientColor     = EditorGUILayout.ColorField("Ambient Color",     _light.ambientColor);
            _light.ambientIntensity = EditorGUILayout.Slider("Ambient Intensity", _light.ambientIntensity, 0f, 3f);

            if (EditorGUI.EndChangeCheck()) MarkDirty();
            AssetCaptureUtility.EndBox();
        }

        private void DrawLightSettings(string label, ref ACLightSettings s, ref bool fold)
        {
            fold = EditorGUILayout.Foldout(fold, label, true);
            if (!fold) return;

            EditorGUI.indentLevel++;
            s.enabled   = EditorGUILayout.Toggle("Enabled",   s.enabled);
            using (new EditorGUI.DisabledGroupScope(!s.enabled))
            {
                s.rotation  = EditorGUILayout.Vector3Field("Rotation",  s.rotation);
                s.intensity = EditorGUILayout.Slider("Intensity", s.intensity, 0f, 8f);
                s.color     = EditorGUILayout.ColorField("Color",     s.color);
            }
            EditorGUI.indentLevel--;
        }

        // ── Section: Image Adjustment ─────────────────────────────────

        private void DrawImageAdjustSection()
        {
            _foldAdj = AssetCaptureUtility.DrawFoldout("  IMAGE ADJUSTMENT", _foldAdj);
            if (!_foldAdj) return;

            AssetCaptureUtility.BeginBox();
            EditorGUI.BeginChangeCheck();

            _adj.brightness = EditorGUILayout.Slider("Brightness", _adj.brightness, -1f,  1f);
            _adj.contrast   = EditorGUILayout.Slider("Contrast",   _adj.contrast,    0f,  3f);
            _adj.saturation = EditorGUILayout.Slider("Saturation", _adj.saturation,  0f,  3f);
            _adj.exposure   = EditorGUILayout.Slider("Exposure",   _adj.exposure,   -3f,  3f);
            _adj.gamma      = EditorGUILayout.Slider("Gamma",      _adj.gamma,      0.1f, 3f);

            if (EditorGUI.EndChangeCheck()) MarkDirty();

            if (GUILayout.Button("Reset Adjustments", GUILayout.Height(20)))
            {
                _adj = new ACImageAdjustSettings();
                MarkDirty();
            }
            AssetCaptureUtility.EndBox();
        }

        // ── Section: Export ───────────────────────────────────────────

        private void DrawExportSection()
        {
            _foldExport = AssetCaptureUtility.DrawFoldout("  EXPORT", _foldExport);
            if (!_foldExport) return;

            AssetCaptureUtility.BeginBox();

            // Resolution presets
            EditorGUILayout.LabelField("Resolution", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            DrawResolutionButton(128); DrawResolutionButton(256);
            DrawResolutionButton(512); DrawResolutionButton(1024); DrawResolutionButton(2048);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("W", GUILayout.Width(12));
            _exp.resolutionWidth = EditorGUILayout.IntField(_exp.resolutionWidth, GUILayout.Width(60));
            EditorGUILayout.LabelField("H", GUILayout.Width(12));
            _exp.resolutionHeight = EditorGUILayout.IntField(_exp.resolutionHeight, GUILayout.Width(60));
            EditorGUILayout.EndHorizontal();

            AssetCaptureUtility.DrawSeparator(2, 2);

            // Path
            EditorGUILayout.LabelField("Output", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            _exp.exportPath = EditorGUILayout.TextField("Path", _exp.exportPath);
            if (GUILayout.Button("...", GUILayout.Width(28)))
            {
                string newPath = AssetCaptureExporter.BrowsePath(_exp.exportPath);
                if (newPath != null) _exp.exportPath = newPath;
            }
            EditorGUILayout.EndHorizontal();

            _exp.fileName = EditorGUILayout.TextField("File Name", _exp.fileName);
            _exp.overwriteExistingFile = EditorGUILayout.Toggle("Overwrite", _exp.overwriteExistingFile);

            AssetCaptureUtility.DrawSeparator(2, 2);

            // Sprite settings
            EditorGUILayout.LabelField("Sprite Import", EditorStyles.boldLabel);
            _exp.spriteMode    = (ACSpriteMode)EditorGUILayout.EnumPopup("Mesh Type",    _exp.spriteMode);
            _exp.compression   = (ACCompression)EditorGUILayout.EnumPopup("Compression", _exp.compression);
            _exp.generateMipMaps = EditorGUILayout.Toggle("Generate Mip Maps", _exp.generateMipMaps);

            EditorGUILayout.Space(4);

            // Export buttons
            Color prev = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.3f, 0.6f, 1f);
            if (GUILayout.Button("⬇  EXPORT PNG", GUILayout.Height(32)))
                DoExport(ACExportFormat.PNG);

            GUI.backgroundColor = new Color(1f, 0.7f, 0.3f);
            if (GUILayout.Button("⬇  EXPORT JPG", GUILayout.Height(32)))
                DoExport(ACExportFormat.JPG);

            GUI.backgroundColor = prev;
            AssetCaptureUtility.EndBox();
        }

        private void DrawResolutionButton(int size)
        {
            bool active = _exp.resolutionWidth == size && _exp.resolutionHeight == size;
            Color prev = GUI.backgroundColor;
            if (active) GUI.backgroundColor = new Color(0.4f, 0.7f, 1f);
            if (GUILayout.Button(size.ToString(), GUILayout.Height(20)))
            {
                _exp.resolutionWidth  = size;
                _exp.resolutionHeight = size;
            }
            GUI.backgroundColor = prev;
        }

        // ── Section: Preset ───────────────────────────────────────────

        private void DrawPresetSection()
        {
            _foldPreset = AssetCaptureUtility.DrawFoldout("  PRESET", _foldPreset);
            if (!_foldPreset) return;

            AssetCaptureUtility.BeginBox();

            _activePreset = (AssetCapturePreset)EditorGUILayout.ObjectField(
                "Preset Asset", _activePreset, typeof(AssetCapturePreset), false);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Save Preset", GUILayout.Height(24)))
                SavePreset();

            if (GUILayout.Button("Load Preset", GUILayout.Height(24)) && _activePreset != null)
                LoadPreset(_activePreset);

            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("New Preset Asset", GUILayout.Height(22)))
                CreateNewPresetAsset();

            AssetCaptureUtility.EndBox();
        }

        // ── Preview Panel ─────────────────────────────────────────────

        private void DrawPreviewPanel()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(_previewWidth));

            // Title
            EditorGUILayout.LabelField("  PREVIEW", EditorStyles.boldLabel,
                GUILayout.Height(20));

            // Preview image
            _previewHeight = _previewWidth; // square by default
            Rect previewRect = EditorGUILayout.GetControlRect(false,
                _previewHeight, GUILayout.Width(_previewWidth));

            // Checkerboard background (indicates transparency)
            DrawCheckerboard(previewRect);

            if (_renderer != null && _renderer.HasPrefab)
            {
                // PreviewRenderUtility renders directly into previewRect during Repaint.
                // This correctly triggers URP's SRP pipeline.
                _renderer.DrawPreview(previewRect, _cam, _bg, _light, _adj);
            }
            else
            {
                EditorGUI.LabelField(previewRect, "Load a prefab to see preview",
                    EditorStyles.centeredGreyMiniLabel);
            }


            // Handle mouse interaction (orbit/pan/zoom)
            bool viewChanged = _camCtrl.HandleInput(previewRect, Event.current);
            if (viewChanged)
            {
                MarkDirty();
                Repaint();
            }

            // Preview controls below
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Reset View", GUILayout.Height(20)))
            { _camCtrl.ResetCamera(); MarkDirty(); }
            if (GUILayout.Button("Fit", GUILayout.Height(20)))
                FitToFrame();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Left drag: Orbit  |  Middle drag: Pan  |  Scroll: Zoom",
                EditorStyles.centeredGreyMiniLabel);

            EditorGUILayout.EndVertical();
        }

        // ── Helpers ───────────────────────────────────────────────────

        private void LoadPrefab()
        {
            if (_loadedPrefab == null) { _renderer.UnloadPrefab(); return; }

            _exp.fileName = _loadedPrefab.name;
            _renderer.LoadPrefab(_loadedPrefab, _obj, _frame, _camCtrl);
            MarkDirty();
        }

        private void MarkDirty() => _renderer?.MarkDirty();

        private void AutoCenter()
        {
            if (!_renderer.HasPrefab || _renderer.PrefabInstance == null) return;
            Bounds bounds = AssetCaptureUtility.CalculateBounds(_renderer.PrefabInstance);
            _camCtrl.AutoCenter(bounds);
            MarkDirty();
        }

        private void FitToFrame()
        {
            if (!_renderer.HasPrefab || _renderer.PrefabInstance == null) return;
            float aspect = _previewWidth / Mathf.Max(_previewHeight, 1f);
            Bounds bounds = AssetCaptureUtility.CalculateBounds(_renderer.PrefabInstance);
            _camCtrl.FitToFrame(bounds, _frame.padding, aspect);
            MarkDirty();
        }

        private void DoExport(ACExportFormat fmt)
        {
            _exp.format = fmt;
            if (string.IsNullOrEmpty(_exp.fileName) && _loadedPrefab != null)
                _exp.fileName = _loadedPrefab.name;

            var preset = BuildPreset();
            string path = AssetCaptureExporter.Export(_renderer, preset);
            if (path != null)
                EditorUtility.RevealInFinder(
                    AssetCaptureUtility.ProjectRelativeToAbsolute(path));
        }

        private AssetCapturePreset BuildPreset()
        {
            var p = ScriptableObject.CreateInstance<AssetCapturePreset>();
            p.camera         = _cam.Clone();
            p.objectTransform = _obj.Clone();
            p.background     = _bg.Clone();
            p.lighting       = _light.Clone();
            p.imageAdjust    = _adj.Clone();
            p.export         = _exp.Clone();
            p.autoFraming    = _frame.Clone();
            return p;
        }

        private void SavePreset()
        {
            // If we already have an asset selected, overwrite it
            if (_activePreset != null)
            {
                _activePreset.CopyFrom(BuildPreset());
                EditorUtility.SetDirty(_activePreset);
                AssetDatabase.SaveAssets();
                Debug.Log("[AssetCapture] Preset saved: " + AssetDatabase.GetAssetPath(_activePreset));
                return;
            }
            CreateNewPresetAsset();
        }

        private void CreateNewPresetAsset()
        {
            AssetCaptureUtility.EnsureDirectoryExists(
                AssetCaptureUtility.ProjectRelativeToAbsolute(_presetSavePath));
            AssetDatabase.Refresh();

            string assetPath = AssetDatabase.GenerateUniqueAssetPath(
                _presetSavePath + "/NewCapturePreset.asset");

            var preset = BuildPreset();
            preset.presetName = Path.GetFileNameWithoutExtension(assetPath);
            AssetDatabase.CreateAsset(preset, assetPath);
            AssetDatabase.SaveAssets();
            _activePreset = preset;
            EditorGUIUtility.PingObject(preset);
            Debug.Log("[AssetCapture] Preset created: " + assetPath);
        }

        private void LoadPreset(AssetCapturePreset p)
        {
            _cam    = p.camera.Clone();
            _obj    = p.objectTransform.Clone();
            _bg     = p.background.Clone();
            _light  = p.lighting.Clone();
            _adj    = p.imageAdjust.Clone();
            _exp    = p.export.Clone();
            _frame  = p.autoFraming.Clone();
            _camCtrl.UpdateSettings(_cam);
            if (_loadedPrefab != null) LoadPrefab();
            MarkDirty();
        }

        private void HandlePrefabDrop(Rect rect)
        {
            Event evt = Event.current;
            if ((evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)
                && rect.Contains(evt.mousePosition))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    foreach (var obj in DragAndDrop.objectReferences)
                    {
                        if (obj is GameObject go)
                        {
                            _loadedPrefab = go;
                            LoadPrefab();
                            break;
                        }
                    }
                    evt.Use();
                }
            }
        }

        private static void DrawCheckerboard(Rect rect)
        {
            // Simple alternating grey squares to indicate transparency
            Color a = new Color(0.35f, 0.35f, 0.35f);
            Color b = new Color(0.25f, 0.25f, 0.25f);
            float size = 12f;
            int cols = Mathf.CeilToInt(rect.width  / size);
            int rows = Mathf.CeilToInt(rect.height / size);

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    Color col = ((r + c) % 2 == 0) ? a : b;
                    Rect cell = new Rect(rect.x + c * size, rect.y + r * size, size, size);
                    EditorGUI.DrawRect(
                        new Rect(cell.x, cell.y,
                            Mathf.Min(size, rect.xMax - cell.x),
                            Mathf.Min(size, rect.yMax - cell.y)), col);
                }
            }
        }

        private void InitStyles()
        {
            AssetCaptureUtility.InitStyles();

            if (_titleStyle == null)
            {
                _titleStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize  = 13,
                    alignment = TextAnchor.MiddleLeft
                };
                _titleStyle.normal.textColor = new Color(0.9f, 0.9f, 1f);
            }
        }
    }
}
#endif

