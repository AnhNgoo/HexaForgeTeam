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
            EnsureMainCamera();
            yield return null;
        }

        /// <summary>
        /// A number of production scenes spawn their gameplay camera only after
        /// the login/player sequence.  PlayMode tests load those scenes directly,
        /// therefore they need a deterministic camera fixture for systems such as
        /// water, input and camera validation.
        /// </summary>
        public static Camera EnsureMainCamera()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                return mainCamera;
            }

            GameObject cameraObject = new GameObject("TestRunner_MainCamera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetPositionAndRotation(
                new Vector3(0f, 6f, -8f),
                Quaternion.Euler(20f, 0f, 0f));

            Camera camera = cameraObject.AddComponent<Camera>();
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 1000f;
            cameraObject.AddComponent<AudioListener>();
            return camera;
        }

        /// <summary>
        /// Prepares the lightweight environment used by prefab/system tests.
        /// These tests exercise a component in isolation and do not need to
        /// reload a large gameplay scene for every testcase.
        /// </summary>
        public static void PrepareRuntimeFixture()
        {
            LogAssert.ignoreFailingMessages = true;
            EnsureMainCamera();
        }
    }
}
