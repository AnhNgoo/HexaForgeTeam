using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

[TestReport]
[Category("Techniques")]
public class PlayerTechniqueCooldownTests
{
    private PlayerTestWorld _world;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        _world = new PlayerTestWorld("Tech_Cooldown");
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
        id: "PL-TEC-001",
        title: "Kỹ thuật: test cooldown theo thời gian",
        expected: "Cooldown hết sau thời gian chờ.",
        steps: "1) StartCooldown(0.05). 2) Wait 0.1s. 3) IsOnCooldown = false.")]
    public IEnumerator TimeBased_Cooldown_Expires()
    {
        var cooldownType = PlayerTestUtils.FindType("Cooldown");
        Assert.IsNotNull(cooldownType);

        var cooldown = Activator.CreateInstance(cooldownType!);
        var startCooldown = cooldownType!.GetMethod("StartCooldown", BindingFlags.Instance | BindingFlags.Public);
        var isOnCooldownProp = cooldownType.GetProperty("IsOnCooldown", BindingFlags.Instance | BindingFlags.Public);
        Assert.IsNotNull(startCooldown);
        Assert.IsNotNull(isOnCooldownProp);

        startCooldown!.Invoke(cooldown, new object[] { 0.05f });
        Assert.IsTrue((bool)isOnCooldownProp!.GetValue(cooldown));

        yield return new WaitForSeconds(0.1f);
        Assert.IsFalse((bool)isOnCooldownProp.GetValue(cooldown));
    }
}
