using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

[TestReport]
[Category("Techniques")]
public class PlayerTechniqueVirtualSceneTests
{
    private PlayerTestWorld _world;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        _world = new PlayerTestWorld("Tech_VirtualScene");
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
        id: "PL-TEC-003",
        title: "Kỹ thuật: scene cô lập + Physics.Simulate + artifact",
        expected: "World bootstrap thành công và chụp screenshot.",
        steps: "1) Build world. 2) SimulateFixedSteps(5). 3) CaptureScreenshot.")]
    public IEnumerator VirtualScene_IsolatedBootstrap_And_PhysicsSimulate()
    {
        Assert.IsNotNull(_world.PlayerGo);
        Assert.IsNotNull(_world.Kael);

        _world.SimulateFixedSteps(steps: 5, fixedDeltaTime: 0.02f);
        yield return null;

        yield return _world.CaptureScreenshot("PlayerTechniquesWorld.png");
    }
}
