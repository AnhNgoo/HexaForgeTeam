using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

[TestReport]
[Category("Artifact")]
public class PlayerArtifactsTests
{
    private PlayerTestWorld _world;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        _world = new PlayerTestWorld("PlayerArtifacts");
        yield return _world.BuildDefaultWorld();
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        if (_world != null)
            yield return _world.DisposeWorld();
    }

    [UnityTest]
    [Category("P2")]
    [TestCaseMeta(
        id: "PL-ART-001",
        title: "Artifact: chụp screenshot Player test world",
        expected: "Screenshot được lưu dưới TestScreenshots.",
        steps: "1) Build world. 2) CaptureScreenshot.")]
    public IEnumerator CaptureScreenshot_AfterBootstrap()
    {
        yield return _world.CaptureScreenshot("PlayerTestWorld.png");
        Assert.Pass("Screenshot captured (see console log for path)." );
    }
}
