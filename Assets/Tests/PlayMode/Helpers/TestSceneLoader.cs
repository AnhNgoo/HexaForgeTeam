using System.Collections;
using NUnit.Framework;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace DuskBlade.Tests
{
    public static class TestSceneLoader
    {
        public static IEnumerator Load(string path)
        {
            Assert.IsFalse(string.IsNullOrEmpty(path), "Scene test không được để trống.");
            LogAssert.ignoreFailingMessages = true;
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
            Assert.AreEqual(SceneManager.GetActiveScene().path, path, "Scene active không đúng scene test: " + path);
            yield return null;
        }
    }
}
