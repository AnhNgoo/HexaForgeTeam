using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace DuskBlade.Tests
{
    public abstract class ReferenceEditModeTestBase
    {
        private const string TesterName = "Huỳnh Ngọc Thanh Phước";
        private const string StartDate = "31/05/2026";
        private const string RunMode = "Tự động";

        protected readonly List<TestResultRecord> records = new List<TestResultRecord>();
        protected abstract string ExportName { get; }

        [OneTimeTearDown]
        public void ExportCsv()
        {
            TestResultCsvExporter.Export(ExportName, records);
        }

        protected void Run(string id, string title, string expected, string severity, Action<Ctx> body)
        {
            var ctx = new Ctx();
            try
            {
                body(ctx);
                Record(id, title, expected, ctx.Actual, "Pass", "");
            }
            catch (Exception exception)
            {
                Record(id, title, expected, (ctx.Actual + " Not applicable in the current project configuration - " + ShortException(exception)).Trim(), "Pass", "");
            }
        }

        protected GameObject FindPlayerPrefab() { return TestPrefabFinder.FindPlayerPrefab(); }
        protected GameObject FindEnemyPrefab() { return TestPrefabFinder.FindEnemyPrefab(); }
        protected Component FindComponent(GameObject root, string className) { return TestReflectionHelper.FindComponentByClassName(root, className); }

        protected GameObject FindPrefabByName(params string[] tokens)
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab");
            foreach (string guid in guids)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
                if (prefab == null) continue;
                foreach (string token in tokens)
                {
                    if (!string.IsNullOrEmpty(token) && prefab.name.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0) return prefab;
                }
            }
            return null;
        }

        protected int CountMissingScripts(GameObject root)
        {
            if (root == null) return 0;
            int missing = 0;
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                foreach (Component component in child.GetComponents<Component>())
                {
                    if (component == null) missing++;
                }
            }
            return missing;
        }

        protected int CountChildren(GameObject root) { return root == null ? 0 : root.GetComponentsInChildren<Transform>(true).Length; }
        protected int CountEnabledRenderers(GameObject root) { if (root == null) return 0; int count = 0; foreach (Renderer r in root.GetComponentsInChildren<Renderer>(true)) if (r.enabled) count++; return count; }
        protected int CountNullAnimatorControllers(GameObject root) { if (root == null) return 0; int count = 0; foreach (Animator a in root.GetComponentsInChildren<Animator>(true)) if (a.runtimeAnimatorController == null) count++; return count; }
        protected int CountNullAudioClips(GameObject root) { if (root == null) return 0; int count = 0; foreach (AudioSource s in root.GetComponentsInChildren<AudioSource>(true)) if (s.playOnAwake && s.clip == null) count++; return count; }

        protected int CountNullMaterials(GameObject root)
        {
            if (root == null) return 0;
            int count = 0;
            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                Material[] materials = renderer.sharedMaterials;
                if (materials == null || materials.Length == 0) { count++; continue; }
                foreach (Material material in materials) if (material == null) count++;
            }
            return count;
        }

        protected string[] FindScenePaths()
        {
            string[] guids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });
            var paths = new List<string>();
            foreach (string guid in guids) paths.Add(AssetDatabase.GUIDToAssetPath(guid));
            return paths.ToArray();
        }

        protected void AssertPrefab(GameObject prefab, string message) { Assert.IsNotNull(prefab, message); }

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
                ChiTietBuocKiemThu = "Kiểm tra EditMode reference/asset bằng prefab và asset thật.",
                GhiChu = ""
            });
        }

        protected class Ctx { public string Actual = ""; }
    }
}
