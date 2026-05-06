using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[InitializeOnLoad]
public static class ProjectSizeAnalyzer
{
    private const float ListRowMaxHeight = 20f;
    private const float SizeLabelWidth = 90f;
    private const long SmallSizeBytes = 1L * 1024 * 1024;
    private const long MediumSizeBytes = 50L * 1024 * 1024;

    private static readonly Dictionary<string, long> SizeCache = new Dictionary<string, long>();
    private static GUIStyle sizeLabelStyle;
    private static GUIStyle modernToggleStyle;
    private static GUIStyle roundedButtonStyle;
    private static VisualElement toolbarUI;
    private static bool showSizes = true; // Default to show sizes

    private static float positionOffset = -200f; // Position near undo history on the right
    private static float buttonHeight = 22f; // Slightly larger for better touch area

    static ProjectSizeAnalyzer()
    {
        EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;
        EditorApplication.projectChanged += ClearCache;
        EditorApplication.delayCall += AddToolbarUI;
    }

    private static void ClearCache()
    {
        SizeCache.Clear();
    }

    private static void AddToolbarUI()
    {
        var toolbarType = typeof(Editor).Assembly.GetType("UnityEditor.Toolbar");
        if (toolbarType == null) return;

        var toolbars = Resources.FindObjectsOfTypeAll(toolbarType);
        if (toolbars.Length == 0) return;

        var toolbar = toolbars[0];
        var rootField = toolbarType.GetField("m_Root", BindingFlags.NonPublic | BindingFlags.Instance);
        if (rootField == null) return;

        var root = rootField.GetValue(toolbar) as VisualElement;
        if (root == null) return;

        var rightContainer = root.Q("ToolbarZoneRightAlign"); // Use right align container
        if (rightContainer == null)
        {
            // Fallback to left align if right align doesn't exist
            rightContainer = root.Q("ToolbarZoneLeftAlign");
            if (rightContainer == null) return;
        }

        // Remove old UI if it exists to prevent duplication
        if (toolbarUI != null)
        {
            rightContainer.Remove(toolbarUI);
        }

        toolbarUI = new IMGUIContainer(OnGUI);
        toolbarUI.style.marginRight = -positionOffset; // Use marginRight for right alignment

        rightContainer.Add(toolbarUI);
    }

    private static GUIStyle CreateRoundedButtonStyle()
    {
        if (roundedButtonStyle == null)
        {
            roundedButtonStyle = new GUIStyle(GUIStyle.none)
            {
                fixedHeight = buttonHeight,
                fontSize = 10,
                padding = new RectOffset(12, 12, 3, 3),
                alignment = TextAnchor.MiddleCenter,
                clipping = TextClipping.Clip,
                border = new RectOffset(8, 8, 8, 8) // Larger border for rounded effect
            };

            // Create custom textures for different states
            var normalTexture = CreateRoundedTexture(new Color(0.3f, 0.3f, 0.3f, 0.7f), new Color(0.25f, 0.25f, 0.25f, 0.8f));
            roundedButtonStyle.normal.background = normalTexture;
            roundedButtonStyle.normal.textColor = Color.white;

            var activeTexture = CreateRoundedTexture(new Color(0.2f, 0.5f, 0.8f, 0.9f), new Color(0.15f, 0.45f, 0.75f, 0.95f));
            roundedButtonStyle.onNormal.background = activeTexture;
            roundedButtonStyle.onNormal.textColor = Color.white;

            var hoverTexture = CreateRoundedTexture(new Color(0.4f, 0.4f, 0.4f, 0.8f), new Color(0.35f, 0.35f, 0.35f, 0.85f));
            roundedButtonStyle.hover.background = hoverTexture;
            roundedButtonStyle.hover.textColor = Color.white;

            var onHoverTexture = CreateRoundedTexture(new Color(0.25f, 0.6f, 0.9f, 0.95f), new Color(0.2f, 0.55f, 0.85f, 1f));
            roundedButtonStyle.onHover.background = onHoverTexture;
            roundedButtonStyle.onHover.textColor = Color.white;
        }

        return roundedButtonStyle;
    }

    private static Texture2D CreateRoundedTexture(Color backgroundColor, Color borderColor)
    {
        int width = 32;
        int height = 32;
        var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);

        // Fill with background color
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = backgroundColor;
        }

        // Draw rounded corners
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Calculate distance from center to determine if it's within the rounded corner area
                float centerX = width / 2f;
                float centerY = height / 2f;
                float dx = x - centerX;
                float dy = y - centerY;

                // For corners, we want to draw border color where appropriate
                if (IsCornerPixel(x, y, width, height))
                {
                    float distFromEdgeX = Math.Min(x, width - x - 1);
                    float distFromEdgeY = Math.Min(y, height - y - 1);
                    float minDist = Math.Min(distFromEdgeX, distFromEdgeY);

                    if (minDist <= 2) // Border thickness
                    {
                        pixels[y * width + x] = borderColor;
                    }
                }
                else
                {
                    // Draw inner rounded rectangle
                    float cornerRadius = width / 3f; // Make it more rounded
                    float outerDistance = Math.Min(Math.Min(x, width - x), Math.Min(y, height - y));

                    // Calculate distance from each corner
                    bool inRoundedArea = false;
                    float[] cornerX = { cornerRadius, width - cornerRadius, cornerRadius, width - cornerRadius };
                    float[] cornerY = { cornerRadius, cornerRadius, height - cornerRadius, height - cornerRadius };

                    for (int c = 0; c < 4; c++)
                    {
                        float dist = Mathf.Sqrt((x - cornerX[c]) * (x - cornerX[c]) + (y - cornerY[c]) * (y - cornerY[c]));
                        if (dist <= cornerRadius)
                        {
                            inRoundedArea = true;
                            break;
                        }
                    }

                    // Also check if it's within the main body (not in sharp corners)
                    if ((x > cornerRadius && x < width - cornerRadius) ||
                        (y > cornerRadius && y < height - cornerRadius) ||
                        inRoundedArea)
                    {
                        // Apply border effect
                        float distFromBorder = Math.Min(Math.Min(x, width - x), Math.Min(y, height - y));
                        if (distFromBorder <= 1) // Inner border effect
                        {
                            pixels[y * width + x] = borderColor;
                        }
                    }
                }
            }
        }

        texture.SetPixels(pixels);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;
        texture.Apply();
        return texture;
    }

    private static bool IsCornerPixel(int x, int y, int width, int height)
    {
        int cornerSize = width / 4; // Adjust corner detection size
        return (x < cornerSize && y < cornerSize) ||           // Top-left
               (x >= width - cornerSize && y < cornerSize) ||  // Top-right
               (x < cornerSize && y >= height - cornerSize) || // Bottom-left
               (x >= width - cornerSize && y >= height - cornerSize); // Bottom-right
    }

    private static void OnGUI()
    {
        GUILayout.BeginHorizontal();

        GUIStyle style = CreateRoundedButtonStyle();

        bool newShowSizes = GUILayout.Toggle(showSizes, showSizes ? "SIZE ON" : "SIZE OFF", style, GUILayout.Width(90), GUILayout.Height(buttonHeight));

        if (newShowSizes != showSizes)
        {
            showSizes = newShowSizes;
            // Repaint all project windows to update the display
            var projectBrowserType = typeof(Editor).Assembly.GetType("UnityEditor.ProjectBrowser");
            if (projectBrowserType != null)
            {
                var browsers = Resources.FindObjectsOfTypeAll(projectBrowserType);
                foreach (var browserObj in browsers)
                {
                    var browser = browserObj as EditorWindow;
                    if (browser != null)
                    {
                        browser.Repaint();
                    }
                }
            }
        }

        GUILayout.EndHorizontal();
    }

    private static void OnProjectWindowItemGUI(string guid, Rect selectionRect)
    {
        if (!showSizes) return; // Don't show sizes if toggled off

        if (selectionRect.height > ListRowMaxHeight)
            return; // Chi hien thi trong che do list

        var assetPath = AssetDatabase.GUIDToAssetPath(guid);
        if (string.IsNullOrEmpty(assetPath))
            return;

        var sizeBytes = GetSizeBytes(assetPath);
        var sizeText = FormatBytes(sizeBytes);
        var labelRect = selectionRect;
        labelRect.xMin = Mathf.Max(selectionRect.xMin, selectionRect.xMax - SizeLabelWidth);
        var oldColor = GUI.color;
        GUI.color = GetSizeColor(sizeBytes);
        GUI.Label(labelRect, sizeText, GetSizeLabelStyle());
        GUI.color = oldColor;
    }

    private static Color GetSizeColor(long sizeBytes)
    {
        if (sizeBytes >= MediumSizeBytes)
            return new Color(1f, 0.45f, 0.35f);

        if (sizeBytes >= SmallSizeBytes)
            return new Color(1f, 0.75f, 0.35f);

        return new Color(0.5f, 0.9f, 0.6f);
    }

    private static GUIStyle GetSizeLabelStyle()
    {
        if (sizeLabelStyle == null)
        {
            sizeLabelStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleRight
            };
        }

        return sizeLabelStyle;
    }

    private static long GetSizeBytes(string assetPath)
    {
        if (SizeCache.TryGetValue(assetPath, out var cached))
            return cached;

        long total = 0;
        var absPath = AssetPathToAbsolute(assetPath);

        if (AssetDatabase.IsValidFolder(assetPath))
        {
            if (Directory.Exists(absPath))
            {
                foreach (var file in Directory.EnumerateFiles(absPath, "*", SearchOption.AllDirectories))
                {
                    if (file.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
                        continue;

                    try
                    {
                        total += new FileInfo(file).Length;
                    }
                    catch
                    {
                        // Ignore files that cannot be accessed
                    }
                }
            }
        }
        else
        {
            if (File.Exists(absPath))
            {
                try
                {
                    total = new FileInfo(absPath).Length;
                }
                catch
                {
                    total = 0;
                }
            }
        }

        SizeCache[assetPath] = total;
        return total;
    }

    private static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return string.Format("{0:0.##} {1}", len, sizes[order]);
    }

    private static string AssetPathToAbsolute(string assetPath)
    {
        if (string.IsNullOrEmpty(assetPath))
            return string.Empty;

        if (!assetPath.StartsWith("Assets", StringComparison.OrdinalIgnoreCase))
            return string.Empty;

        var relative = assetPath.Substring("Assets".Length).TrimStart('/', '\\');
        return Path.Combine(Application.dataPath, relative);
    }
}