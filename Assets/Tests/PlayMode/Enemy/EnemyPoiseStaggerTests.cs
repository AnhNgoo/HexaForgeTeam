using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEngine;
using System;
using System.Reflection;

[TestReport]
[Category("Integration")]
public class EnemyPoiseStaggerTests
{
    private EnemyTestWorld _world;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        _world = new EnemyTestWorld("EnemyPoiseStagger");
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
        id: "EN-INT-004",
        title: "Vỡ poise → Stagger và tự hồi về state mặc định",
        expected: "OnStagger được gọi; state Stagger; sau staggerDuration quay về Idle.",
        steps: "1) Set maxPoise thấp + staggerDuration ngắn. 2) TakeHit poise lớn. 3) Wait. 4) Verify state.")]
    public IEnumerator PoiseBreak_TriggersStagger_ThenReturnsToIdle()
    {
        bool staggerCalled = false;
        var eventManager = EnemyTestUtils.GetProperty(_world.Enemy, "EventManager", BindingFlags.Instance | BindingFlags.Public);
        EnemyTestUtils.AddEventHandler(eventManager, "OnStagger", (Action)(() => staggerCalled = true));

        _world.SetEnemyDataField("maxPoise", 20f);
        _world.SetEnemyDataField("staggerDuration", 0.05f);

        var damageReceiver = EnemyTestUtils.GetProperty(_world.Enemy, "DamageReceiver", BindingFlags.Instance | BindingFlags.Public);
        EnemyTestUtils.Call(damageReceiver, "TakeHit", BindingFlags.Instance | BindingFlags.Public, 0f, 25f);
        yield return null;

        Assert.IsTrue(staggerCalled);
        var stateMachine = EnemyTestUtils.GetProperty(_world.Enemy, "StateMachine", BindingFlags.Instance | BindingFlags.Public);
        var currentState = EnemyTestUtils.GetProperty(stateMachine, "CurrentState", BindingFlags.Instance | BindingFlags.Public);
        Assert.AreEqual("EnemyState_Stagger", currentState.GetType().Name);

        yield return new WaitForSeconds(0.1f);
        currentState = EnemyTestUtils.GetProperty(stateMachine, "CurrentState", BindingFlags.Instance | BindingFlags.Public);
        Assert.AreEqual("EnemyState_Idle", currentState.GetType().Name);
    }
}
