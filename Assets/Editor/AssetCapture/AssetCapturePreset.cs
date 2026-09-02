#if UNITY_EDITOR
using System;
using UnityEngine;

// ============================================================
//  AssetCapturePreset.cs  –  HexaForge 3D to 2D Asset Capture
//  ScriptableObject: holds ALL tool settings.
// ============================================================

namespace HexaForge.AssetCapture
{
    public enum ACProjection   { Orthographic, Perspective }
    public enum ACExportFormat { PNG, JPG }
    public enum ACSpriteMode   { FullRect, Tight }
    public enum ACCompression  { None, LowQuality, NormalQuality, HighQuality }

    [Serializable]
    public class ACLightSettings
    {
        public bool    enabled   = true;
        public Vector3 rotation  = new Vector3(50f, -30f, 0f);
        public float   intensity = 1f;
        public Color   color     = Color.white;

        public ACLightSettings Clone() => new ACLightSettings
            { enabled = enabled, rotation = rotation, intensity = intensity, color = color };
    }

    [Serializable]
    public class ACCameraSettings
    {
        public ACProjection projection       = ACProjection.Orthographic;
        public Vector3      position         = new Vector3(0f, 0f, -5f);
        public Vector3      eulerRotation    = new Vector3(30f, -30f, 0f);
        public float        fieldOfView      = 60f;
        public float        orthographicSize = 2f;
        public float        distance         = 5f;

        public ACCameraSettings Clone() => new ACCameraSettings
        {
            projection = projection, position = position, eulerRotation = eulerRotation,
            fieldOfView = fieldOfView, orthographicSize = orthographicSize, distance = distance
        };
    }

    [Serializable]
    public class ACObjectSettings
    {
        public Vector3 position      = Vector3.zero;
        public Vector3 eulerRotation = Vector3.zero;
        public Vector3 scale         = Vector3.one;

        public ACObjectSettings Clone() => new ACObjectSettings
            { position = position, eulerRotation = eulerRotation, scale = scale };
    }

    [Serializable]
    public class ACLightingSettings
    {
        public ACLightSettings mainLight = new ACLightSettings
            { enabled = true, rotation = new Vector3(50f,-30f,0f), intensity = 1f, color = Color.white };
        public ACLightSettings fillLight = new ACLightSettings
            { enabled = true, rotation = new Vector3(0f,120f,0f), intensity = 0.5f, color = new Color(0.8f,0.85f,1f) };
        public ACLightSettings rimLight  = new ACLightSettings
            { enabled = false, rotation = new Vector3(-50f,180f,0f), intensity = 0.3f, color = Color.white };
        public float ambientIntensity = 0.5f;
        public Color ambientColor     = new Color(0.2f, 0.2f, 0.25f, 1f);

        public ACLightingSettings Clone() => new ACLightingSettings
        {
            mainLight = mainLight.Clone(), fillLight = fillLight.Clone(), rimLight = rimLight.Clone(),
            ambientIntensity = ambientIntensity, ambientColor = ambientColor
        };
    }

    [Serializable]
    public class ACImageAdjustSettings
    {
        [Range(-1f,  1f)] public float brightness = 0f;
        [Range( 0f,  3f)] public float contrast   = 1f;
        [Range( 0f,  3f)] public float saturation = 1f;
        [Range(-3f,  3f)] public float exposure   = 0f;
        [Range(0.1f, 3f)] public float gamma      = 1f;

        public bool IsIdentity =>
            Mathf.Approximately(brightness, 0f) && Mathf.Approximately(contrast, 1f) &&
            Mathf.Approximately(saturation, 1f) && Mathf.Approximately(exposure, 0f) &&
            Mathf.Approximately(gamma, 1f);

        public ACImageAdjustSettings Clone() => new ACImageAdjustSettings
            { brightness = brightness, contrast = contrast, saturation = saturation,
              exposure = exposure, gamma = gamma };
    }

    [Serializable]
    public class ACExportSettings
    {
        public int            resolutionWidth       = 512;
        public int            resolutionHeight      = 512;
        public string         exportPath            = "Assets/2DAssetItem";
        public string         fileName              = "";
        public ACExportFormat format                = ACExportFormat.PNG;
        public bool           overwriteExistingFile = true;
        public ACSpriteMode   spriteMode            = ACSpriteMode.FullRect;
        public ACCompression  compression           = ACCompression.None;
        public bool           generateMipMaps       = false;

        public ACExportSettings Clone() => new ACExportSettings
        {
            resolutionWidth = resolutionWidth, resolutionHeight = resolutionHeight,
            exportPath = exportPath, fileName = fileName, format = format,
            overwriteExistingFile = overwriteExistingFile, spriteMode = spriteMode,
            compression = compression, generateMipMaps = generateMipMaps
        };
    }

    [Serializable]
    public class ACBackgroundSettings
    {
        public bool  transparentBackground = true;
        public Color backgroundColor       = new Color(0.2f, 0.2f, 0.2f, 1f);

        public ACBackgroundSettings Clone() => new ACBackgroundSettings
            { transparentBackground = transparentBackground, backgroundColor = backgroundColor };
    }

    [Serializable]
    public class ACAutoFramingSettings
    {
        public bool  autoCenter = true;
        public bool  autoFit    = true;
        [Range(0f, 0.5f)]
        public float padding    = 0.1f;

        public ACAutoFramingSettings Clone() => new ACAutoFramingSettings
            { autoCenter = autoCenter, autoFit = autoFit, padding = padding };
    }

    // ── Main Preset ScriptableObject ─────────────────────────────

    /// <summary>
    /// Persists all Asset Capture settings as a Unity asset file.
    /// Create via: Assets > Create > HexaForge > Asset Capture > Preset
    /// </summary>
    [CreateAssetMenu(fileName = "NewCapturePreset",
                     menuName  = "HexaForge/Asset Capture/Preset",
                     order     = 200)]
    public class AssetCapturePreset : ScriptableObject
    {
        [Header("Preset Info")]
        public string presetName = "New Preset";

        [Header("Camera")]
        public ACCameraSettings camera = new ACCameraSettings();

        [Header("Object Transform")]
        public ACObjectSettings objectTransform = new ACObjectSettings();

        [Header("Background")]
        public ACBackgroundSettings background = new ACBackgroundSettings();

        [Header("Lighting")]
        public ACLightingSettings lighting = new ACLightingSettings();

        [Header("Image Adjustment")]
        public ACImageAdjustSettings imageAdjust = new ACImageAdjustSettings();

        [Header("Export")]
        public ACExportSettings export = new ACExportSettings();

        [Header("Auto Framing")]
        public ACAutoFramingSettings autoFraming = new ACAutoFramingSettings();

        /// <summary>Overwrites this preset data with data from another preset instance.</summary>
        public void CopyFrom(AssetCapturePreset other)
        {
            if (other == null) return;
            presetName      = other.presetName;
            camera          = other.camera.Clone();
            objectTransform = other.objectTransform.Clone();
            background      = other.background.Clone();
            lighting        = other.lighting.Clone();
            imageAdjust     = other.imageAdjust.Clone();
            export          = other.export.Clone();
            autoFraming     = other.autoFraming.Clone();
        }
    }
}
#endif
