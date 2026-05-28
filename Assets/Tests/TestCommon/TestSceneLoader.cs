using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

public static class TestSceneLoader
{
    public const string DefaultScenePath = "Assets/_Data/Scenes/GameDemo.unity";

    public static IEnumerator LoadScene(string scenePath = DefaultScenePath)
    {
        Assert.IsFalse(string.IsNullOrWhiteSpace(scenePath), "Scene path must be provided.");

#if UNITY_EDITOR
        var op = EditorSceneManager.LoadSceneAsyncInPlayMode(
            scenePath,
            new LoadSceneParameters(LoadSceneMode.Single));
        Assert.IsNotNull(op, $"Failed to start loading scene at path '{scenePath}'.");
        while (!op.isDone)
            yield return null;
#else
        // In player, you can only load scenes included in Build Settings.
        var sceneName = Path.GetFileNameWithoutExtension(scenePath);
        var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        Assert.IsNotNull(op, $"Failed to start loading scene '{sceneName}'. Ensure it's in Build Settings.");
        while (!op.isDone)
            yield return null;
#endif

        // Let Awake/OnEnable/Start run.
        yield return null;
    }
}
