#if UNITY_EDITOR

using System.IO;
using UnityEditor;
using UnityEngine;

public static class RenderTexturePngExporter
{
    private const string MenuPath =
        "Tools/Export Selected RenderTexture To PNG";

    [MenuItem(MenuPath)]
    private static void ExportSelectedRenderTexture()
    {
        RenderTexture source = Selection.activeObject as RenderTexture;

        if (source == null)
        {
            Debug.LogError("Please select a RenderTexture asset first.");
            return;
        }

        string outputPath = EditorUtility.SaveFilePanel(
            "Export RenderTexture",
            Application.dataPath,
            source.name + ".png",
            "png"
        );

        if (string.IsNullOrEmpty(outputPath))
            return;

        RenderTexture previousActive = RenderTexture.active;
        RenderTexture temporary = null;
        Texture2D outputTexture = null;

        try
        {
            if (!source.IsCreated())
                source.Create();

            // MainMap_RT is Linear. Blit to an sRGB temporary texture
            // so the exported PNG does not become unexpectedly dark.
            temporary = RenderTexture.GetTemporary(
                source.width,
                source.height,
                0,
                RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.sRGB
            );

            Graphics.Blit(source, temporary);
            RenderTexture.active = temporary;

            outputTexture = new Texture2D(
                source.width,
                source.height,
                TextureFormat.RGBA32,
                false
            );

            outputTexture.ReadPixels(
                new Rect(0, 0, source.width, source.height),
                0,
                0,
                false
            );

            outputTexture.Apply(false, false);

            byte[] pngBytes = outputTexture.EncodeToPNG();
            File.WriteAllBytes(outputPath, pngBytes);

            Debug.Log("RenderTexture exported to: " + outputPath);
            AssetDatabase.Refresh();

            EditorUtility.RevealInFinder(outputPath);
        }
        catch (System.Exception exception)
        {
            Debug.LogException(exception);
        }
        finally
        {
            RenderTexture.active = previousActive;

            if (outputTexture != null)
                Object.DestroyImmediate(outputTexture);

            if (temporary != null)
                RenderTexture.ReleaseTemporary(temporary);
        }
    }
}

#endif