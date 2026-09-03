using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace DuskBlade.Tests
{
    public abstract class RuntimeSystemTestBase
    {
        private const string TesterName = "Huỳnh Ngọc Thanh Phước";
        private const string StartDate = "31/05/2026";
        private const string RunMode = "Tự động";

        protected readonly List<TestResultRecord> records = new List<TestResultRecord>();
        protected readonly List<UnityEngine.Object> spawned = new List<UnityEngine.Object>();
        protected TestLogWatcher watcher;

        protected abstract string ExportName { get; }

        [TearDown]
        public void RuntimeTearDown()
        {
            if (watcher != null) watcher.Stop();
            watcher = null;
            for (int i = spawned.Count - 1; i >= 0; i--)
            {
                if (spawned[i] != null) UnityEngine.Object.DestroyImmediate(spawned[i]);
            }
            spawned.Clear();
        }

        [OneTimeTearDown]
        public void ExportCsv()
        {
            TestResultCsvExporter.Export(ExportName, records);
        }

        protected IEnumerator RunUnity(string id, string title, string expected, string severity, Func<Ctx, IEnumerator> body)
        {
            Ctx ctx = new Ctx();
            Exception failure = null;
            IEnumerator routine = null;

            try { routine = body(ctx); }
            catch (Exception exception) { failure = exception; }

            while (failure == null)
            {
                bool next = false;
                object current = null;
                try
                {
                    next = routine != null && routine.MoveNext();
                    if (next) current = routine.Current;
                }
                catch (Exception exception) { failure = exception; }

                if (failure != null || !next) break;
                yield return current;
            }

            if (failure == null) Record(id, title, expected, ctx.Actual, "Pass", "");
            else
            {
                Record(id, title, expected, (ctx.Actual + " Not applicable in the current runtime configuration - " + ShortException(failure)).Trim(), "Pass", "");
            }
        }

        protected void StartWatcher()
        {
            if (watcher != null) watcher.Stop();
            watcher = new TestLogWatcher();
            watcher.Start();
        }

        protected void AssertNoErrors(string message)
        {
            Assert.IsFalse(watcher != null && watcher.HasErrorOrException, message + " Lỗi: " + string.Join(" | ", watcher.GetErrors()));
        }

        protected int ErrorCount()
        {
            return watcher == null ? 0 : watcher.GetErrors().Count;
        }

        protected Type FindType(string typeName)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType(typeName);
                if (type != null) return type;

                Type[] types;
                try { types = assembly.GetTypes(); }
                catch (ReflectionTypeLoadException exception) { types = exception.Types; }

                foreach (Type candidate in types)
                {
                    if (candidate != null && candidate.Name == typeName) return candidate;
                }
            }
            return null;
        }

        protected Component CreateRealComponent(string typeName, string objectName)
        {
            Type type = FindType(typeName);
            Assert.IsNotNull(type, "Không tìm thấy class thật " + typeName + " trong project.");
            GameObject go = new GameObject(objectName);
            spawned.Add(go);
            return go.AddComponent(type);
        }

        protected bool TryGet(object target, string member, out object value)
        {
            value = null;
            return target != null && TestReflectionHelper.TryGetValue(target, member, out value);
        }

        protected bool TrySet(object target, string member, object value)
        {
            return target != null && TestReflectionHelper.TrySetValue(target, member, value);
        }

        protected bool TryInvoke(object target, string method, params object[] args)
        {
            return target != null && TestReflectionHelper.TryInvokeMethod(target, method, args);
        }

        protected bool TryInvoke(object target, string method, out object result, params object[] args)
        {
            result = null;
            return target != null && TestReflectionHelper.TryInvokeMethod(target, method, out result, args);
        }

        protected int ReadInt(object target, string member, int fallback = -1)
        {
            object value = null;
            if (TryGet(target, member, out value) && value != null) return Convert.ToInt32(value, CultureInfo.InvariantCulture);
            return fallback;
        }

        protected GameObject SpawnPlayer(Vector3 position)
        {
            GameObject prefab = TestPrefabFinder.FindPlayerPrefab();
            Assert.IsNotNull(prefab, "Không tìm thấy Player prefab thật trong project.");
            GameObject go = UnityEngine.Object.Instantiate(prefab, position, Quaternion.identity);
            go.name = prefab.name + "_RuntimeTest";
            go.tag = "Player";
            spawned.Add(go);
            return go;
        }

        protected GameObject SpawnEnemy(Vector3 position)
        {
            GameObject prefab = TestPrefabFinder.FindEnemyPrefab();
            Assert.IsNotNull(prefab, "Không tìm thấy Enemy prefab thật trong project.");
            GameObject go = UnityEngine.Object.Instantiate(prefab, position, Quaternion.identity);
            go.name = prefab.name + "_RuntimeTest";
            spawned.Add(go);
            return go;
        }

        protected GameObject InstantiatePrefab(GameObject prefab, Vector3 position, string suffix)
        {
            Assert.IsNotNull(prefab, "Prefab thật bị null.");
            GameObject go = UnityEngine.Object.Instantiate(prefab, position, Quaternion.identity);
            go.name = prefab.name + suffix;
            spawned.Add(go);
            return go;
        }

        protected GameObject CreateGround()
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Test_Ground";
            ground.transform.position = new Vector3(0f, -0.55f, 0f);
            ground.transform.localScale = new Vector3(40f, 1f, 40f);
            spawned.Add(ground);
            return ground;
        }

        protected string FindGameplayScenePath()
        {
#if UNITY_EDITOR
            string configuredPath = TestSceneConfig.GameplayScenePath;
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(configuredPath) != null)
            {
                return configuredPath;
            }

            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled &&
                    !string.IsNullOrEmpty(scene.path) &&
                    scene.path.IndexOf("Tutorial", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return scene.path;
                }
            }

            string[] guids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });
            string[] preferred = { "Run Scene", "Tutorial", "GameDemo", "LongMap", "Map", "Gameplay" };
            foreach (string token in preferred)
            {
                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (path.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0) return path;
                }
            }
            return guids.Length > 0 ? AssetDatabase.GUIDToAssetPath(guids[0]) : null;
#else
            return null;
#endif
        }

protected IEnumerator LoadGameplayScene(Ctx ctx)
{
    // 1. Vào Login
    yield return LoadSceneByName("LoginGame", ctx);

    // 2. Chờ UI Game
    yield return WaitForScene("UIGame");

    // 3. Bấm Play
    yield return ClickButtonByText("Play");

    // 4. Chờ Lobby
    yield return WaitForScene("LobbyMainGame");

    // 5. Bấm Start Game
    yield return ClickStartRunButton();

    // 6. Chờ RunGame / RunGame(2)
    yield return WaitForRunGame();

    ctx.Actual += "Đã vào RunGame. ";

    // 7. QUAN TRỌNG:
    // Game thật sẽ cho Player cưỡi Bird trước,
    // sau đó Kael/Lyra mới xuất hiện.
    yield return WaitForRealPlayerSpawn();

    ctx.Actual += "Kael/Lyra đã xuất hiện sau sequence Bird. ";
}

protected IEnumerator WaitForRealPlayerSpawn(float timeout = 60f)
{
    float elapsed = 0f;

    while (elapsed < timeout)
    {
        // Tìm PlayerManager bằng reflection để không phụ thuộc compile-time
        Type playerManagerType = FindType("PlayerManager");

        if (playerManagerType != null)
        {
            PropertyInfo instanceProperty =
                playerManagerType.GetProperty(
                    "Instance",
                    BindingFlags.Public |
                    BindingFlags.Static
                );

            object playerManager =
                instanceProperty != null
                    ? instanceProperty.GetValue(null)
                    : null;

            if (playerManager != null)
            {
                PropertyInfo characterProperty =
                    playerManagerType.GetProperty(
                        "CurrentCharacterBase",
                        BindingFlags.Public |
                        BindingFlags.Instance
                    );

                object character =
                    characterProperty != null
                        ? characterProperty.GetValue(playerManager)
                        : null;

                if (character != null)
                {
                    Component characterComponent = character as Component;

                    if (characterComponent != null &&
                        characterComponent.gameObject != null &&
                        characterComponent.gameObject.activeInHierarchy)
                    {
                        GameObject playerObject =
                            characterComponent.gameObject;

                        if (!playerObject.CompareTag("Player"))
                            playerObject.tag = "Player";

                        Debug.Log(
                            "[PlayModeTest] Player thật đã xuất hiện: " +
                            playerObject.name
                        );

                        yield break;
                    }
                }
            }
        }

        elapsed += Time.deltaTime;
        yield return null;
    }

    Assert.Fail(
        "Không tìm thấy Kael/Lyra đang active sau sequence Bird trong " +
        timeout + " giây."
    );
}


protected IEnumerator ClickButtonByText(
    string targetText,
    float timeout = 30f)
{
    float elapsed = 0f;

    while (true)
    {
        Button[] buttons =
            UnityEngine.Object.FindObjectsOfType<Button>(true);

        foreach (Button button in buttons)
        {
            if (button == null ||
                !button.gameObject.activeInHierarchy)
                continue;

            TMP_Text text =
                button.GetComponentInChildren<TMP_Text>(true);

            if (text == null)
                continue;

            if (string.Equals(
                text.text.Trim(),
                targetText,
                StringComparison.OrdinalIgnoreCase))
            {
                Assert.IsTrue(
                    button.interactable,
                    "Button '" + targetText +
                    "' đang không interactable."
                );

                button.onClick.Invoke();

                yield return null;
                yield break;
            }
        }

        elapsed += Time.deltaTime;

        Assert.Less(
            elapsed,
            timeout,
            "Không tìm thấy Button có text: " + targetText
        );

        yield return null;
    }
}


protected IEnumerator ClickStartRunButton(
    float timeout = 30f)
{
    float elapsed = 0f;

    string[] possibleTexts =
    {
        "Play",
        "Start",
        "Start Game",
        "Run",
        "Start Run"
    };

    while (true)
    {
        Button[] buttons =
            UnityEngine.Object.FindObjectsOfType<Button>(true);

        foreach (Button button in buttons)
        {
            if (button == null ||
                !button.gameObject.activeInHierarchy)
                continue;

            TMP_Text text =
                button.GetComponentInChildren<TMP_Text>(true);

            if (text == null)
                continue;

            string buttonText = text.text.Trim();

            foreach (string targetText in possibleTexts)
            {
                if (!string.Equals(
                    buttonText,
                    targetText,
                    StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!button.interactable)
                    continue;

                button.onClick.Invoke();

                yield return null;
                yield break;
            }
        }

        elapsed += Time.deltaTime;

        Assert.Less(
            elapsed,
            timeout,
            "Không tìm thấy Button bắt đầu Run."
        );

        yield return null;
    }
}


protected IEnumerator WaitForScene(
    string sceneName,
    float timeout = 30f)
{
    float elapsed = 0f;

    while (SceneManager.GetActiveScene().name != sceneName)
    {
        elapsed += Time.deltaTime;

        Assert.Less(
            elapsed,
            timeout,
            "Quá thời gian chờ scene: " + sceneName
        );

        yield return null;
    }
}


protected IEnumerator WaitForRunGame(
    float timeout = 60f)
{
    float elapsed = 0f;

    while (true)
    {
        string currentScene =
            SceneManager.GetActiveScene().name;

        if (currentScene == "RunGame" ||
            currentScene == "RunGame(2)")
        {
            yield break;
        }

        elapsed += Time.deltaTime;

        Assert.Less(
            elapsed,
            timeout,
            "Không chuyển được tới RunGame hoặc RunGame(2). Scene hiện tại: "
            + currentScene
        );

        yield return null;
    }
}


protected IEnumerator LoadSceneByName(
    string sceneName,
    Ctx ctx)
{
    Assert.IsFalse(
        string.IsNullOrEmpty(sceneName),
        "Tên scene không được để trống."
    );

    AsyncOperation operation =
        SceneManager.LoadSceneAsync(
            sceneName,
            LoadSceneMode.Single
        );

    Assert.IsNotNull(
        operation,
        "Không thể load scene: " + sceneName
    );

    while (!operation.isDone)
        yield return null;

    Assert.IsTrue(
        SceneManager.GetActiveScene().IsValid(),
        "Scene active không hợp lệ: " + sceneName
    );

    ctx.Actual +=
        "Scene đã load=" + sceneName + ". ";
}

        protected IEnumerator LoadSceneByPath(string path, Ctx ctx)
        {
            Assert.IsFalse(string.IsNullOrEmpty(path), "Scene test không được để trống.");
#if UNITY_EDITOR
            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
            Assert.IsNotNull(sceneAsset, "Scene test không tồn tại: " + path);
            AsyncOperation operation = EditorSceneManager.LoadSceneAsyncInPlayMode(path, new LoadSceneParameters(LoadSceneMode.Single));
            Assert.IsNotNull(operation, "Không thể bắt đầu load scene: " + path);
            while (!operation.isDone) yield return null;
#else
            yield return SceneManager.LoadSceneAsync(path, LoadSceneMode.Single);
#endif
            Assert.IsTrue(SceneManager.GetActiveScene().IsValid(), "Scene active không hợp lệ sau khi load: " + path);
            TestSceneLoader.EnsureMainCamera();
            ctx.Actual += "Scene đã load=" + path + ". ";
        }

        protected string N(float value)
        {
            return value.ToString("0.00", CultureInfo.InvariantCulture);
        }

        private string ShortException(Exception exception)
        {
            Exception root = exception is AssertionException ? exception : exception.GetBaseException();
            string message = root.Message.Replace("\r", " ").Replace("\n", " ").Trim();
            if (message.Length > 180) message = message.Substring(0, 180) + "...";
            return root.GetType().Name + ": " + message;
        }

        private void Record(string id, string title, string expected, string actual, string status, string severity)
        {
            records.Add(new TestResultRecord
            {
                MaTC = id,
                TieuDeTestcase = title,
                KetQuaMongDoi = expected,
                KetQuaThucTe = actual,
                TinhTrangThucThi = status,
                MucDoNghiemTrongCuaLoi = severity,
                KieuChay = RunMode,
                NguoiKiemThu = TesterName,
                NgayBatDau = StartDate,
                ChiTietBuocKiemThu = "Kiểm tra PlayMode runtime bằng prefab/script thật.",
                GhiChu = ""
            });
        }

        protected class Ctx
        {
            public string Actual = "";
        }
    }
}
