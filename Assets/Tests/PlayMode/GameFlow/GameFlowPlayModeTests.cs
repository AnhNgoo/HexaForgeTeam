using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace DuskBlade.Tests
{
    public class GameFlowPlayModeTests : RuntimeSystemTestBase
    {
        protected override string ExportName => "GameFlow";

        [UnityTest, Category("GameFlow"), Category("Integration"), Category("Tự động")]
        [Description("Kiểm tra flow Login -> Lobby -> Run Game -> Run Scene Khoa -> Final Boss theo đúng thứ tự, không lỗi Console.")]
        public IEnumerator FLOW_001_LoadToanBoGameFlowKhongLoi()
        {
            return RunUnity("FLOW-001", "Load toàn bộ game flow không lỗi", "Tất cả scene trong GameFlowScenePaths load tuần tự, active đúng scene và không có Error/Exception.", "High", RunConfiguredFlow);
        }

        private IEnumerator RunConfiguredFlow(Ctx context)
        {
            Assert.Greater(TestSceneConfig.GameFlowScenePaths.Length, 0, "Chưa cấu hình scene cho game flow.");
            StartWatcher();

            for (int i = 0; i < TestSceneConfig.GameFlowScenePaths.Length; i++)
            {
                string path = TestSceneConfig.GameFlowScenePaths[i];
                yield return LoadSceneByPath(path, context);
                yield return null;

                string expectedName = System.IO.Path.GetFileNameWithoutExtension(path);
                string actualName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                Assert.AreEqual(expectedName, actualName, "Scene active không đúng thứ tự flow tại bước " + (i + 1) + ": " + path);
                AssertNoErrors("Load scene flow phát sinh Error/Exception tại bước " + (i + 1) + ": " + path);
            }

            context.Actual += "Tổng scene flow=" + TestSceneConfig.GameFlowScenePaths.Length + ", Error/Exception=" + ErrorCount() + ".";
        }
    }
}