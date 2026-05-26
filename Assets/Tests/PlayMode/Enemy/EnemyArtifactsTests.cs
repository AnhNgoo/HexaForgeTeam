using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

[TestReport]
[Category("Artifact")]
public class EnemyArtifactsTests
{
    private EnemyTestWorld _world;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        _world = new EnemyTestWorld("EnemyArtifacts");
        yield return _world.BuildDefaultWorld();
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        if (_world != null)
            yield return _world.DisposeWorld();
    }

    [UnityTest]
    [Category("P1")]
    [TestCaseMeta(
        id: "EN-TEC-001",
        title: "Virtual scene + chụp screenshot (artifact)",
        expected: "Scene test được dựng; screenshot được lưu dưới TestScreenshots.",
        steps: "1) Build world. 2) Chờ vài frame. 3) CaptureScreenshot.",
        notes: "Mở file ảnh sau khi chạy test để xem trực quan.")]
    public IEnumerator CaptureScreenshot_AfterBootstrap()
    {
        yield return _world.StepFrames(10);
        yield return _world.CaptureScreenshot("EnemyTechniquesWorld.png");
        Assert.Pass("Screenshot captured.");
    }
}
