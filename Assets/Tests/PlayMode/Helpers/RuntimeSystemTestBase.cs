using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;

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
                Record(id, title, expected, (ctx.Actual + " Fail - " + ShortException(failure)).Trim(), "Fail", severity);
                throw failure;
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
            return TestEnemySpawnHelper.SpawnEnemyWithCampLifecycle(prefab, position, "_RuntimeTest", spawned);
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
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled && !string.IsNullOrEmpty(scene.path)) return scene.path;
            }

            string[] guids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });
            string[] preferred = { "GameDemo", "LongMap", "Map", "Gameplay" };
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
            string path = FindGameplayScenePath();
            Assert.IsFalse(string.IsNullOrEmpty(path), "Không tìm thấy scene gameplay thật trong Assets.");
#if UNITY_EDITOR
            AsyncOperation op = EditorSceneManager.LoadSceneAsyncInPlayMode(path, new LoadSceneParameters(LoadSceneMode.Single));
            while (op != null && !op.isDone) yield return null;
#else
            yield return SceneManager.LoadSceneAsync(path, LoadSceneMode.Single);
#endif
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
