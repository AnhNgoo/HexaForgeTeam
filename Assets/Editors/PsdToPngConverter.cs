using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class PsdToPngConverter : EditorWindow
{
    UnityEngine.Object folderObject;
    string folderPath;
    List<string> psdPaths = new List<string>();
    List<bool> selected;
    Vector2 scroll;

    [MenuItem("Window/PSD to PNG Converter")]
    static void OpenWindow()
    {
        var w = GetWindow<PsdToPngConverter>("PSD → PNG");
        w.minSize = new Vector2(420, 300);
    }

    void OnEnable()
    {
        UpdateFromSelection();
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("PSD → PNG Converter", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        folderObject = EditorGUILayout.ObjectField("Folder (Project)", folderObject, typeof(DefaultAsset), false);
        if (GUILayout.Button("Scan PSDs", GUILayout.Width(100)))
        {
            UpdateFromFolderField();
        }
        EditorGUILayout.EndHorizontal();

        if (!string.IsNullOrEmpty(folderPath))
        {
            EditorGUILayout.LabelField("Folder:", folderPath);
        }

        EditorGUILayout.Space();

        if (psdPaths.Count == 0)
        {
            EditorGUILayout.HelpBox("No .psd files found in the selected folder.", MessageType.Info);
        }
        else
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Select All")) { SetAll(true); }
            if (GUILayout.Button("Deselect All")) { SetAll(false); }
            if (GUILayout.Button("Refresh")) { UpdateFromFolderField(); }
            EditorGUILayout.EndHorizontal();

            scroll = EditorGUILayout.BeginScrollView(scroll);
            for (int i = 0; i < psdPaths.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                selected[i] = EditorGUILayout.Toggle(selected[i], GUILayout.Width(18));
                EditorGUILayout.LabelField(psdPaths[i]);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();
            if (GUILayout.Button("Convert Selected to PNG", GUILayout.Height(30)))
            {
                ConvertSelected();
            }
        }
    }

    void UpdateFromSelection()
    {
        var sel = Selection.activeObject;
        if (sel != null)
        {
            var path = AssetDatabase.GetAssetPath(sel);
            if (AssetDatabase.IsValidFolder(path))
            {
                folderObject = sel;
                folderPath = path;
                ScanPsdFiles();
                return;
            }
        }
        // fallback: no selection
        folderPath = string.Empty;
        psdPaths.Clear();
        selected = new List<bool>();
    }

    void UpdateFromFolderField()
    {
        if (folderObject != null)
        {
            var path = AssetDatabase.GetAssetPath(folderObject);
            if (AssetDatabase.IsValidFolder(path))
            {
                folderPath = path;
                ScanPsdFiles();
                return;
            }
        }
        EditorUtility.DisplayDialog("PSD→PNG", "Please choose a valid folder inside the Project window.", "OK");
    }

    void ScanPsdFiles()
    {
        psdPaths.Clear();
        selected = new List<bool>();
        if (string.IsNullOrEmpty(folderPath)) return;

        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folderPath });
        foreach (var g in guids)
        {
            var p = AssetDatabase.GUIDToAssetPath(g);
            if (p.EndsWith(".psd", StringComparison.OrdinalIgnoreCase))
            {
                psdPaths.Add(p);
                selected.Add(true);
            }
        }
    }

    void SetAll(bool v)
    {
        for (int i = 0; i < selected.Count; i++) selected[i] = v;
    }

    void ConvertSelected()
    {
        if (psdPaths.Count == 0) return;

        int total = 0;
        for (int i = 0; i < selected.Count; i++) if (selected[i]) total++;
        if (total == 0) return;

        try
        {
            int done = 0;
            for (int i = 0; i < psdPaths.Count; i++)
            {
                if (!selected[i]) continue;
                string assetPath = psdPaths[i];
                EditorUtility.DisplayProgressBar("Converting PSD→PNG", assetPath, (float)done / total);

                var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (importer == null)
                {
                    Debug.LogWarning("Can't get importer for " + assetPath);
                    done++; continue;
                }

                bool origReadable = importer.isReadable;
                TextureImporterCompression origCompression = importer.textureCompression;
                TextureImporterType origType = importer.textureType;

                // Make readable to extract pixels
                if (!origReadable || importer.textureCompression != TextureImporterCompression.Uncompressed || importer.textureType != TextureImporterType.Default)
                {
                    importer.isReadable = true;
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    importer.textureType = TextureImporterType.Default;
                    importer.SaveAndReimport();
                }

                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
                if (tex == null)
                {
                    Debug.LogWarning("Unable to load texture: " + assetPath);
                }
                else
                {
                    Texture2D copy = new Texture2D(tex.width, tex.height, TextureFormat.RGBA32, false);
                    try
                    {
                        // Try to get pixels
                        var pixels = tex.GetPixels();
                        copy.SetPixels(pixels);
                        copy.Apply();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning("Failed to read pixels from " + assetPath + " : " + ex.Message);
                        copy = null;
                    }

                    if (copy != null)
                    {
                        byte[] png = copy.EncodeToPNG();
                        string projectRoot = Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length);
                        string absSource = Path.Combine(projectRoot, assetPath);
                        string absDest = Path.ChangeExtension(absSource, ".png");
                        try
                        {
                            File.WriteAllBytes(absDest, png);
                            string relDest = Path.ChangeExtension(assetPath, ".png");
                            AssetDatabase.ImportAsset(relDest);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError("Failed to write PNG for " + assetPath + " : " + ex.Message);
                        }
                    }
                }

                // restore importer
                if (importer != null)
                {
                    importer.isReadable = origReadable;
                    importer.textureCompression = origCompression;
                    importer.textureType = origType;
                    importer.SaveAndReimport();
                }

                done++;
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();
        }

        EditorUtility.DisplayDialog("PSD→PNG", "Conversion finished.", "OK");
        UpdateFromFolderField();
    }
}
