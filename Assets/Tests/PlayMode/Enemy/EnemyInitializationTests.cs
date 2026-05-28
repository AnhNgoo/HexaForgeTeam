using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using System.Reflection;

[TestReport]
[Category("Integration")]
[Category("Enemy")]
public class EnemyInitializationTests
{
    private EnemyTestWorld _world;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        _world = new EnemyTestWorld("EnemyInit");
        yield return _world.BuildDefaultWorld();
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        if (_world != null)
            yield return _world.DisposeWorld();
    }

    [UnityTest]
    [Category("P0")]
    [Description("TC EN-INT-001: Kiểm tra Enemy khởi tạo vào state mặc định (thường là Idle).")]
    [TestCaseMeta(
        id: "EN-INT-001",
        title: "Enemy khởi tạo vào state mặc định",
        expected: "EnemyStateMachine ở Idle (hoặc state mặc định theo thiết kế).",
        steps: "1) Tạo scene test. 2) Spawn Enemy. 3) Chờ 1 frame.")]
    public IEnumerator DefaultState_IsIdle_AfterInitialize()
    {
        Assert.IsNotNull(_world.Enemy);

        var stateMachine = EnemyTestUtils.GetProperty(_world.Enemy, "StateMachine", BindingFlags.Instance | BindingFlags.Public);
        var currentState = EnemyTestUtils.GetProperty(stateMachine, "CurrentState", BindingFlags.Instance | BindingFlags.Public);
        Assert.AreEqual("EnemyState_Idle", currentState.GetType().Name);
        yield return null;
    }
}
