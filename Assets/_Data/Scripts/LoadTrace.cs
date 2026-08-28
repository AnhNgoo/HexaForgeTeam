using UnityEngine;
using UnityEngine.SceneManagement;

public static class LoadTrace
{
    private static float _flowStart;
    private static float _lastCheckpoint;
    private static bool _active;

    public static bool IsActive => _active;

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Install()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    public static void Begin(string flowName)
    {
        _active = true;
        _flowStart = Time.realtimeSinceStartup;
        _lastCheckpoint = _flowStart;

        Debug.Log($"[LOAD-TRACE] BEGIN: {flowName}");
    }

    public static void Mark(string checkpoint)
    {
        if (!_active)
            return;

        float now = Time.realtimeSinceStartup;
        float delta = now - _lastCheckpoint;
        float total = now - _flowStart;

        Debug.Log(
            $"[LOAD-TRACE] {checkpoint} | " +
            $"+{delta:F2}s | Total {total:F2}s"
        );

        _lastCheckpoint = now;
    }

    public static void End(string checkpoint)
    {
        if (!_active)
            return;

        Mark(checkpoint);
        _active = false;
    }

    private static void HandleSceneLoaded(
        Scene scene,
        LoadSceneMode mode)
    {
        Mark($"Scene loaded: {scene.name} ({mode})");
    }
}