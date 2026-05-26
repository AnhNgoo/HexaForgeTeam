using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

[TestReport]
[Category("Integration")]
public class PlayerCombatComboTests
{
    private PlayerTestWorld _world;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        _world = new PlayerTestWorld("PlayerCombatCombo");
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
        id: "PL-INT-004",
        title: "Combo init tạo đủ melee/punch combos",
        expected: "weaponCombos và punchCombos có 4 phần tử và đúng type đầu tiên.",
        steps: "1) Lấy CharacterCombat. 2) Đọc private fields weaponCombos/punchCombos. 3) Verify length/type.")]
    public IEnumerator Combo_Init_SetsMeleeCombosAndPunchFallback()
    {
        var kael = _world.Kael;

        var characterCombat = PlayerTestUtils.GetProperty(kael, "CharacterCombat", BindingFlags.Instance | BindingFlags.Public);
        Assert.IsNotNull(characterCombat);

        var weaponCombos = (Array)PlayerTestUtils.GetField<object>(characterCombat, "weaponCombos", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(weaponCombos);
        Assert.AreEqual(4, weaponCombos.Length);
        Assert.AreEqual("AttackMeleeStep_1", weaponCombos.GetValue(0)!.GetType().Name);

        var punchCombos = (Array)PlayerTestUtils.GetField<object>(characterCombat, "punchCombos", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(punchCombos);
        Assert.AreEqual(4, punchCombos.Length);
        Assert.AreEqual("PunchStep_1", punchCombos.GetValue(0)!.GetType().Name);

        yield return null;
    }
}
