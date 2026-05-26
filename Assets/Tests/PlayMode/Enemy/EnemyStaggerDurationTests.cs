using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Reflection;

[TestReport]
[Category("Integration")]
public class EnemyStaggerDurationTests
{
    private EnemyTestWorld _world;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        _world = new EnemyTestWorld("EnemyStaggerDuration");
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
        id: "EN-TEC-003",
        title: "Time-based: Stagger kết thúc sau staggerDuration",
        expected: "State Stagger rồi quay về Idle sau thời gian.",
        steps: "1) Set maxPoise thấp + duration ngắn. 2) TakeHit poise lớn. 3) Wait. 4) Verify state.")]
    public IEnumerator Stagger_Ends_AfterDuration()
    {
        _world.SetEnemyDataField("maxPoise", 10f);
        _world.SetEnemyDataField("staggerDuration", 0.05f);

        var damageReceiver = EnemyTestUtils.GetProperty(_world.Enemy, "DamageReceiver", BindingFlags.Instance | BindingFlags.Public);
        EnemyTestUtils.Call(damageReceiver, "TakeHit", BindingFlags.Instance | BindingFlags.Public, 0f, 999f);
        yield return null;

        var stateMachine = EnemyTestUtils.GetProperty(_world.Enemy, "StateMachine", BindingFlags.Instance | BindingFlags.Public);
        var currentState = EnemyTestUtils.GetProperty(stateMachine, "CurrentState", BindingFlags.Instance | BindingFlags.Public);
        Assert.AreEqual("EnemyState_Stagger", currentState.GetType().Name);

        yield return new WaitForSeconds(0.1f);
        currentState = EnemyTestUtils.GetProperty(stateMachine, "CurrentState", BindingFlags.Instance | BindingFlags.Public);
        Assert.AreEqual("EnemyState_Idle", currentState.GetType().Name);
    }
}
