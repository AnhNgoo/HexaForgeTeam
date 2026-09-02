using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace DuskBlade.Editor.CI
{
    public static class CIBuilder
    {
        private const string ProductName = "DuskBlade";

        private static readonly string[] RequiredScenes =
        {
            "Assets/_Data/Scenes/ReleaseGameScenes/LoginGame.unity",
            "Assets/_Data/Scenes/ReleaseGameScenes/UIGame.unity",
            "Assets/Trung/Scene/Loading Scene.unity",
            "Assets/_Data/Scenes/ReleaseGameScenes/LobbyMainGame.unity",
            "Assets/_Data/Scenes/ReleaseGameScenes/TutorialGame.unity",
            "Assets/_Data/Scenes/ReleaseGameScenes/RunGame.unity",
            "Assets/_Data/Scenes/RunGame(2).unity",
            "Assets/_Data/Scenes/ReleaseGameScenes/FinalBosArenaGame.unity"
        };

        public static void BuildWindows()
        {
            string originalVersion = PlayerSettings.bundleVersion;

            try
            {
                ApplyOptionalReleaseVersion();
                SwitchToWindows64();

                string[] scenes = GetAndValidateScenes();

                BuildAddressables();

                string outputPath = ResolveOutputPath();
                PrepareOutputDirectory(outputPath);

                Debug.Log($"[CI] Building {ProductName} to: {outputPath}");
                Debug.Log(
                    $"[CI] Backend: " +
                    $"{PlayerSettings.GetScriptingBackend(BuildTargetGroup.Standalone)}");
                Debug.Log(
                    $"[CI] API compatibility: " +
                    $"{PlayerSettings.GetApiCompatibilityLevel(BuildTargetGroup.Standalone)}");
                Debug.Log($"[CI] Bundle version: {PlayerSettings.bundleVersion}");

                BuildReport report = BuildPipeline.BuildPlayer(
                    new BuildPlayerOptions
                    {
                        scenes = scenes,
                        locationPathName = outputPath,
                        target = BuildTarget.StandaloneWindows64,
                        options = BuildOptions.None
                    });

                if (report == null ||
                    report.summary.result != BuildResult.Succeeded)
                {
                    string result = report == null
                        ? "No BuildReport"
                        : report.summary.result.ToString();

                    int errors = report == null
                        ? -1
                        : report.summary.totalErrors;

                    throw new BuildFailedException(
                        $"Windows build failed. Result={result}, Errors={errors}");
                }

                VerifyRuntimeOutput(outputPath);

                Debug.Log(
                    $"[CI] Build succeeded. " +
                    $"Size={report.summary.totalSize} bytes, " +
                    $"Time={report.summary.totalTime}.");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                throw;
            }
            finally
            {
                if (PlayerSettings.bundleVersion != originalVersion)
                    PlayerSettings.bundleVersion = originalVersion;
            }
        }

        private static void ApplyOptionalReleaseVersion()
        {
            string version = GetArgument("-releaseVersion");

            if (string.IsNullOrWhiteSpace(version))
                return;

            version = version.Trim().TrimStart('v');

            if (!Regex.IsMatch(version, @"^\d+\.\d+\.\d+$"))
            {
                throw new BuildFailedException(
                    $"Invalid -releaseVersion '{version}'. Expected X.Y.Z.");
            }

            PlayerSettings.bundleVersion = version;
        }

        private static void SwitchToWindows64()
        {
            if (EditorUserBuildSettings.activeBuildTarget ==
                BuildTarget.StandaloneWindows64)
            {
                return;
            }

            bool switched =
                EditorUserBuildSettings.SwitchActiveBuildTarget(
                    BuildTargetGroup.Standalone,
                    BuildTarget.StandaloneWindows64);

            if (!switched)
            {
                throw new BuildFailedException(
                    "Could not switch active build target to StandaloneWindows64.");
            }
        }

        private static string[] GetAndValidateScenes()
        {
            string[] enabledScenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            if (!enabledScenes.SequenceEqual(
                    RequiredScenes,
                    StringComparer.OrdinalIgnoreCase))
            {
                string actual =
                    string.Join(Environment.NewLine, enabledScenes);

                throw new BuildFailedException(
                    "Enabled Build Settings scenes do not match the " +
                    "production scene contract." +
                    Environment.NewLine +
                    actual);
            }

            string projectRoot = GetProjectRoot();

            foreach (string scene in enabledScenes)
            {
                string absolutePath = Path.Combine(
                    projectRoot,
                    scene.Replace(
                        '/',
                        Path.DirectorySeparatorChar));

                if (!File.Exists(absolutePath))
                {
                    throw new BuildFailedException(
                        $"Enabled scene does not exist: {scene}");
                }
            }

            return enabledScenes;
        }

        private static void BuildAddressables()
        {
            AddressableAssetSettings settings =
                AddressableAssetSettingsDefaultObject.Settings;

            if (settings == null)
            {
                throw new BuildFailedException(
                    "AddressableAssetSettingsDefaultObject.Settings is null.");
            }

            if (!(settings.ActivePlayerDataBuilder is BuildScriptPackedMode))
            {
                string builder =
                    settings.ActivePlayerDataBuilder == null
                        ? "null"
                        : settings.ActivePlayerDataBuilder
                            .GetType()
                            .FullName;

                throw new BuildFailedException(
                    "Addressables active builder must be Packed Mode. " +
                    $"Actual={builder}");
            }

            if (settings.BuildAddressablesWithPlayerBuild !=
                AddressableAssetSettings.PlayerBuildOption
                    .DoNotBuildWithPlayer)
            {
                throw new BuildFailedException(
                    "This CI script builds Addressables explicitly. " +
                    "Set Build Addressables With Player Build to " +
                    "Do Not Build With Player.");
            }

            if (settings.BuildRemoteCatalog)
            {
                throw new BuildFailedException(
                    "Remote catalog is enabled. " +
                    "This Windows release expects local Addressables.");
            }

            Debug.Log(
                "[CI] Building Addressables with Packed Mode.");

            AddressableAssetSettings.BuildPlayerContent(
                out AddressablesPlayerBuildResult result);

            if (result == null ||
                !string.IsNullOrEmpty(result.Error))
            {
                string error = result == null
                    ? "No result returned."
                    : result.Error;

                throw new BuildFailedException(
                    $"Addressables build failed: {error}");
            }

            Debug.Log(
                $"[CI] Addressables succeeded. " +
                $"Locations={result.LocationCount}, " +
                $"Output={result.OutputPath}");
        }

        private static string ResolveOutputPath()
        {
            string path = GetArgument("-customBuildPath");

            if (string.IsNullOrWhiteSpace(path))
            {
                path = Path.Combine(
                    GetProjectRoot(),
                    "build",
                    "StandaloneWindows64",
                    ProductName + ".exe");
            }

            path = Path.GetFullPath(path);

            if (!string.Equals(
                    Path.GetExtension(path),
                    ".exe",
                    StringComparison.OrdinalIgnoreCase))
            {
                path += ".exe";
            }

            return path;
        }

        private static void PrepareOutputDirectory(
            string outputPath)
        {
            string directory =
                Path.GetDirectoryName(outputPath);

            if (string.IsNullOrWhiteSpace(directory))
            {
                throw new BuildFailedException(
                    $"Invalid output path: {outputPath}");
            }

            if (Directory.Exists(directory))
                Directory.Delete(directory, true);

            Directory.CreateDirectory(directory);
        }

        private static void VerifyRuntimeOutput(
            string executablePath)
        {
            string directory =
                Path.GetDirectoryName(executablePath);

            var required = new List<string>
            {
                executablePath,
                Path.Combine(
                    directory,
                    ProductName + "_Data"),
                Path.Combine(
                    directory,
                    "UnityPlayer.dll")
            };

            if (PlayerSettings.GetScriptingBackend(
                    BuildTargetGroup.Standalone) ==
                ScriptingImplementation.Mono2x)
            {
                required.Add(
                    Path.Combine(
                        directory,
                        "MonoBleedingEdge"));
            }

            string[] missing = required
                .Where(path =>
                    !File.Exists(path) &&
                    !Directory.Exists(path))
                .ToArray();

            if (missing.Length > 0)
            {
                throw new BuildFailedException(
                    "Build reported success but runtime output " +
                    "is incomplete:" +
                    Environment.NewLine +
                    string.Join(Environment.NewLine, missing));
            }
        }

        private static string GetArgument(string name)
        {
            string[] args =
                Environment.GetCommandLineArgs();

            for (int i = 0; i < args.Length - 1; i++)
            {
                if (string.Equals(
                        args[i],
                        name,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return args[i + 1];
                }
            }

            return null;
        }

        private static string GetProjectRoot()
        {
            DirectoryInfo parent =
                Directory.GetParent(Application.dataPath);

            if (parent == null)
            {
                throw new BuildFailedException(
                    "Could not resolve Unity project root.");
            }

            return parent.FullName;
        }
    }
}