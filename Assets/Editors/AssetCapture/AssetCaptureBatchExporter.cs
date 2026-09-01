#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// ============================================================
//  AssetCaptureBatchExporter.cs  –  HexaForge 3D to 2D Asset Capture
//
//  Batch export: renders multiple prefabs with the same preset.
// ============================================================

namespace HexaForge.AssetCapture
{
    public class AssetCaptureBatchExporter
    {
        // ── State ─────────────────────────────────────────────────────

        private List<GameObject> _prefabs     = new List<GameObject>();
        private bool             _foldout     = true;
        private Vector2          _scrollPos;

        // ── Public API ────────────────────────────────────────────────

        public bool HasPrefabs => _prefabs.Count > 0;

        /// <summary>
        /// Draws the Batch Export section UI.
        /// Call inside a GUILayout/EditorGUILayout context.
        /// Returns true if settings changed and preview should refresh.
        /// </summary>
        public void DrawUI(AssetCaptureRenderer renderer, AssetCapturePreset preset)
        {
            _foldout = AssetCaptureUtility.DrawFoldout("  BATCH EXPORT", _foldout);
            if (!_foldout) return;

            AssetCaptureUtility.BeginBox();

            EditorGUILayout.LabelField("Drag prefabs into the list below:", EditorStyles.miniLabel);
            EditorGUILayout.Space(2);

            // ── List ──────────────────────────────────────────────────
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos,
                GUILayout.MaxHeight(150));

            for (int i = 0; i < _prefabs.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();

                // Slot
                var newPrefab = (GameObject)EditorGUILayout.ObjectField(
                    _prefabs[i], typeof(GameObject), false);
                if (newPrefab != _prefabs[i])
                    _prefabs[i] = newPrefab;

                // Remove
                if (GUILayout.Button("✕", GUILayout.Width(22), GUILayout.Height(18)))
                {
                    _prefabs.RemoveAt(i);
                    EditorGUILayout.EndHorizontal();
                    break;
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();

            // ── Add Slot / Clear ─────────────────────────────────────
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+ Add Slot", GUILayout.Height(20)))
                _prefabs.Add(null);
            if (GUILayout.Button("Clear All", GUILayout.Height(20)))
                _prefabs.Clear();
            EditorGUILayout.EndHorizontal();

            // ── Drop-zone for drag & drop ────────────────────────────
            Rect dropRect = EditorGUILayout.GetControlRect(false, 30);
            EditorGUI.DrawRect(dropRect, new Color(0.3f, 0.3f, 0.3f, 0.3f));
            GUI.Label(dropRect, "  ↓ Drop Prefabs Here", EditorStyles.centeredGreyMiniLabel);
            HandleDrop(dropRect);

            EditorGUILayout.Space(4);

            // ── Export All button ─────────────────────────────────────
            Color prev = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
            bool exportAll = GUILayout.Button("⬇  EXPORT ALL (" + _prefabs.Count + " prefabs)",
                GUILayout.Height(32));
            GUI.backgroundColor = prev;

            if (exportAll)
                RunBatchExport(renderer, preset);

            AssetCaptureUtility.EndBox();
        }

        // ── Batch Logic ───────────────────────────────────────────────

        private void RunBatchExport(AssetCaptureRenderer renderer, AssetCapturePreset preset)
        {
            if (renderer == null || preset == null) return;

            int exported = 0, failed = 0;

            for (int i = 0; i < _prefabs.Count; i++)
            {
                GameObject prefab = _prefabs[i];
                if (prefab == null) continue;

                // Progress bar
                bool cancelled = EditorUtility.DisplayCancelableProgressBar(
                    "Batch Export",
                    $"Exporting {prefab.name} … ({i + 1}/{_prefabs.Count})",
                    (float)i / _prefabs.Count);

                if (cancelled) break;

                try
                {
                    // Load this prefab into the renderer
                    var camCtrl = new AssetCaptureCameraController(preset.camera.Clone());
                    renderer.LoadPrefab(prefab, preset.objectTransform,
                                        preset.autoFraming, camCtrl);

                    // Override file name to prefab name
                    string result = AssetCaptureExporter.Export(renderer, preset,
                        overrideFileName: prefab.name);

                    if (result != null) exported++;
                    else                failed++;
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[AssetCapture] Batch export failed for '{prefab.name}': {ex.Message}");
                    failed++;
                }
            }

            EditorUtility.ClearProgressBar();

            string summary = $"Batch export complete.\n" +
                             $"Exported: {exported}  |  Failed/Skipped: {failed}";
            EditorUtility.DisplayDialog("Asset Capture – Batch Export", summary, "OK");
        }

        // ── Drag & Drop ───────────────────────────────────────────────

        private void HandleDrop(Rect dropRect)
        {
            Event evt = Event.current;

            if ((evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)
                && dropRect.Contains(evt.mousePosition))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    foreach (var obj in DragAndDrop.objectReferences)
                    {
                        if (obj is GameObject go)
                            _prefabs.Add(go);
                    }
                    evt.Use();
                }
            }
        }
    }
}
#endif
