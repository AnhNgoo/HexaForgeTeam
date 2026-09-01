#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

// ============================================================
//  AssetCaptureExporter.cs  –  HexaForge 3D to 2D Asset Capture
//
//  Handles PNG/JPG export and automatic TextureImporter setup.
// ============================================================

namespace HexaForge.AssetCapture
{
    public static class AssetCaptureExporter
    {
        // ── Export Entry Points ───────────────────────────────────────

        /// <summary>
        /// Renders the model and exports it to disk.
        /// Returns the Unity project-relative path of the saved file,
        /// or null on failure.
        /// </summary>
        public static string Export(
            AssetCaptureRenderer renderer,
            AssetCapturePreset   preset,
            string               overrideFileName = null)
        {
            if (renderer == null || !renderer.HasPrefab)
            {
                EditorUtility.DisplayDialog("Asset Capture",
                    "Please load a prefab before exporting.", "OK");
                return null;
            }

            // Resolve output info
            ACExportSettings exp = preset.export;
            string baseName = string.IsNullOrEmpty(overrideFileName)
                ? (string.IsNullOrEmpty(exp.fileName) ? "Capture" : exp.fileName)
                : overrideFileName;

            string extension = exp.format == ACExportFormat.PNG ? ".png" : ".jpg";

            // Absolute path of output directory
            string absDir = AssetCaptureUtility.ProjectRelativeToAbsolute(exp.exportPath);
            AssetCaptureUtility.EnsureDirectoryExists(absDir);

            // Resolve final file path (handle overwrite / numbering)
            string absFilePath = AssetCaptureUtility.ResolveFilePath(
                absDir, baseName, extension, exp.overwriteExistingFile);

            // Confirm overwrite if disabled
            if (!exp.overwriteExistingFile && File.Exists(absFilePath))
            {
                bool ok = EditorUtility.DisplayDialog("Asset Capture",
                    $"File already exists:\n{absFilePath}\n\nOverwrite?", "Yes", "Cancel");
                if (!ok) return null;
            }

            // Render at export resolution
            bool transparent = preset.background.transparentBackground && exp.format == ACExportFormat.PNG;
            Texture2D tex = renderer.RenderToTexture2D(
                preset.camera, preset.background, preset.lighting,
                preset.imageAdjust,
                exp.resolutionWidth, exp.resolutionHeight,
                transparent);

            if (tex == null)
            {
                Debug.LogError("[AssetCapture] Render failed – texture is null.");
                return null;
            }

            // Encode
            byte[] bytes;
            if (exp.format == ACExportFormat.PNG)
                bytes = tex.EncodeToPNG();
            else
                bytes = tex.EncodeToJPG(95);

            UnityEngine.Object.DestroyImmediate(tex);

            if (bytes == null || bytes.Length == 0)
            {
                Debug.LogError("[AssetCapture] Encoding failed.");
                return null;
            }

            // Write file
            File.WriteAllBytes(absFilePath, bytes);

            // Refresh AssetDatabase
            AssetDatabase.Refresh();

            // Convert to project-relative path for TextureImporter
            string relPath = AssetCaptureUtility.AbsoluteToProjectRelative(absFilePath);
            if (relPath != null)
            {
                ApplyTextureImportSettings(relPath, exp, transparent);
                AssetDatabase.ImportAsset(relPath, ImportAssetOptions.ForceSynchronousImport);
                Debug.Log($"[AssetCapture] Exported → {relPath}");
            }
            else
            {
                Debug.LogWarning("[AssetCapture] Could not determine project-relative path. " +
                                 "Texture import settings were not applied.");
            }

            return relPath ?? absFilePath;
        }

        // ── Texture Importer Setup ────────────────────────────────────

        /// <summary>
        /// Applies Sprite (2D and UI) import settings to the exported texture.
        /// </summary>
        public static void ApplyTextureImportSettings(
            string projectRelativePath,
            ACExportSettings exp,
            bool transparent)
        {
            var importer = AssetImporter.GetAtPath(projectRelativePath) as TextureImporter;
            if (importer == null) return;

            // Sprite type
            importer.textureType      = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;

            // spriteMeshType is NOT a direct property on TextureImporter in Unity 2022.3.
            // It must be set via TextureImporterSettings.
            var texSettings = new TextureImporterSettings();
            importer.ReadTextureSettings(texSettings);
            texSettings.spriteMeshType = exp.spriteMode == ACSpriteMode.Tight
                ? SpriteMeshType.Tight
                : SpriteMeshType.FullRect;
            importer.SetTextureSettings(texSettings);

            // Alpha
            importer.alphaIsTransparency = transparent;
            importer.alphaSource = transparent
                ? TextureImporterAlphaSource.FromInput
                : TextureImporterAlphaSource.None;

            // Mip maps
            importer.mipmapEnabled = exp.generateMipMaps;

            // Filter
            importer.filterMode = FilterMode.Bilinear;

            // Compression
            if      (exp.compression == ACCompression.LowQuality)    importer.textureCompression = TextureImporterCompression.CompressedLQ;
            else if (exp.compression == ACCompression.NormalQuality)  importer.textureCompression = TextureImporterCompression.Compressed;
            else if (exp.compression == ACCompression.HighQuality)    importer.textureCompression = TextureImporterCompression.CompressedHQ;
            else                                                       importer.textureCompression = TextureImporterCompression.Uncompressed;

            // Max size – use nearest power-of-two that contains the export resolution
            int maxDim = Mathf.Max(exp.resolutionWidth, exp.resolutionHeight);
            importer.maxTextureSize = Mathf.NextPowerOfTwo(maxDim);

            importer.isReadable = false;
            importer.SaveAndReimport();
        }

        // ── Path Browse Helper ────────────────────────────────────────

        /// <summary>
        /// Opens a folder-picker dialog and returns the selected path as a
        /// project-relative string (e.g. "Assets/Icons"), or null if cancelled.
        /// </summary>
        public static string BrowsePath(string currentPath)
        {
            string absDefault = AssetCaptureUtility.ProjectRelativeToAbsolute(currentPath);
            string selected   = EditorUtility.OpenFolderPanel(
                "Select Export Folder", absDefault, "");

            if (string.IsNullOrEmpty(selected)) return null;
            return AssetCaptureUtility.AbsoluteToProjectRelative(selected) ?? currentPath;
        }
    }
}
#endif

