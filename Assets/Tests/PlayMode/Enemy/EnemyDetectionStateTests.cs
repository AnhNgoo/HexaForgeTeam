using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Reflection;

[TestReport]
[Category("Integration")]
public class EnemyDetectionStateTests
{
    private EnemyTestWorld _world;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        _world = new EnemyTestWorld("EnemyDetectState");
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
    [TestCaseMeta(
        id: "EN-INT-002",
        title: "Detection dẫn state Idle → Attack khi Player ở gần",
        expected: "Enemy phát hiện Player và chuyển sang Attack sau vài frame.",
        steps: "1) Đặt Player trước mặt Enemy trong phạm vi. 2) Chờ vài frame. 3) Verify CurrentTarget + state Attack.")]
    public IEnumerator Idle_To_Attack_WhenPlayerDetected_CloseRange()
    {
        _world.EnemyGo.transform.position = new Vector3(0f, 1f, 0f);
        _world.PlayerGo.transform.position = new Vector3(0f, 1f, 1.5f);
        _world.EnemyGo.transform.LookAt(_world.PlayerGo.transform.position);

        yield return _world.StepFrames(5);

        var detection = EnemyTestUtils.GetProperty(_world.Enemy, "Detection", BindingFlags.Instance | BindingFlags.Public);
        var currentTarget = (Transform)EnemyTestUtils.GetProperty(detection, "CurrentTarget", BindingFlags.Instance | BindingFlags.Public);
        Assert.IsNotNull(currentTarget, "Enemy should detect player.");

        var stateMachine = EnemyTestUtils.GetProperty(_world.Enemy, "StateMachine", BindingFlags.Instance | BindingFlags.Public);
        var currentState = EnemyTestUtils.GetProperty(stateMachine, "CurrentState", BindingFlags.Instance | BindingFlags.Public);
        Assert.AreEqual("EnemyState_Attack", currentState.GetType().Name);
    }
}
