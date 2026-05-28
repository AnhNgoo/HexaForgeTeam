using NUnit.Framework;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

[TestReport]
[Category("EditMode")]
[Category("Gold")]
public class GoldManagerTest
{
    private GameObject goldManagerObject;
    private Component goldManager;
    private Type goldManagerType;

    private static Type FindType(string typeName)
    {
        return AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(a =>
            {
                try { return a.GetTypes(); }
                catch (ReflectionTypeLoadException e) { return e.Types.Where(t => t != null)!; }
            })
            .FirstOrDefault(t => t != null && t.Name == typeName);
    }

    private int GetCurrentGold()
    {
        var prop = goldManagerType.GetProperty("CurrentGold", BindingFlags.Instance | BindingFlags.Public);
        return (int)prop.GetValue(goldManager);
    }

    private void CallVoid(string methodName, params object[] args)
    {
        var method = goldManagerType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public);
        method.Invoke(goldManager, args);
    }

    private bool CallBool(string methodName, params object[] args)
    {
        var method = goldManagerType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public);
        return (bool)method.Invoke(goldManager, args);
    }

    [SetUp]
    public void SetUp()
    {
        goldManagerObject = new GameObject("GoldManager");

        goldManagerType = FindType("GoldManager");
        Assert.IsNotNull(goldManagerType, "Could not find type 'GoldManager' via reflection. Ensure scripts compile in Assembly-CSharp.");

        goldManager = goldManagerObject.AddComponent(goldManagerType);

        CallVoid("ResetGold");
    }

    [TearDown]
    public void TearDown()
    {
        UnityEngine.Object.DestroyImmediate(goldManagerObject);
    }

    [Test]
    [Category("P1")]
    [Description("TC GL-MGR-001: AddGold với số dương sẽ tăng CurrentGold đúng theo amount.")]
    [TestCaseMeta(
        id: "GL-MGR-001",
        title: "AddGold: cộng vàng số dương",
        expected: "CurrentGold tăng đúng theo amount.",
        steps: "1) ResetGold. 2) AddGold(100). 3) Kiểm tra CurrentGold == 100.")]
    public void AddGold_WhenAmountPositive_IncreasesCurrentGold()
    {
        CallVoid("AddGold", 100);

        Assert.AreEqual(100, GetCurrentGold());
    }

    [Test]
    [Category("P1")]
    [Description("TC GL-MGR-002: AddGold với 0 không làm thay đổi CurrentGold.")]
    [TestCaseMeta(
        id: "GL-MGR-002",
        title: "AddGold: cộng vàng bằng 0",
        expected: "CurrentGold không đổi.",
        steps: "1) ResetGold. 2) AddGold(0). 3) Kiểm tra CurrentGold == 0.")]
    public void AddGold_WhenAmountIsZero_DoesNotChangeCurrentGold()
    {
        CallVoid("AddGold", 0);

        Assert.AreEqual(0, GetCurrentGold());
    }

    [Test]
    [Category("P1")]
    [Description("TC GL-MGR-003: AddGold với số âm không làm thay đổi CurrentGold.")]
    [TestCaseMeta(
        id: "GL-MGR-003",
        title: "AddGold: cộng vàng số âm",
        expected: "CurrentGold không đổi (không cho phép cộng âm).",
        steps: "1) ResetGold. 2) AddGold(-50). 3) Kiểm tra CurrentGold == 0.")]
    public void AddGold_WhenAmountIsNegative_DoesNotChangeCurrentGold()
    {
        CallVoid("AddGold", -50);

        Assert.AreEqual(0, GetCurrentGold());
    }

    [Test]
    [Category("P1")]
    [Description("TC GL-MGR-004: RemoveGold với số dương sẽ giảm CurrentGold đúng theo amount.")]
    [TestCaseMeta(
        id: "GL-MGR-004",
        title: "RemoveGold: trừ vàng số dương",
        expected: "CurrentGold giảm đúng theo amount.",
        steps: "1) AddGold(100). 2) RemoveGold(40). 3) Kiểm tra CurrentGold == 60.")]
    public void RemoveGold_WhenAmountPositive_DecreasesCurrentGold()
    {
        CallVoid("AddGold", 100);

        CallVoid("RemoveGold", 40);

        Assert.AreEqual(60, GetCurrentGold());
    }

    [Test]
    [Category("P1")]
    [Description("TC GL-MGR-005: RemoveGold vượt quá CurrentGold thì CurrentGold về 0 (không âm).")]
    [TestCaseMeta(
        id: "GL-MGR-005",
        title: "RemoveGold: trừ vượt số vàng hiện có",
        expected: "CurrentGold không âm, về 0.",
        steps: "1) AddGold(50). 2) RemoveGold(100). 3) Kiểm tra CurrentGold == 0.")]
    public void RemoveGold_WhenAmountIsGreaterThanCurrentGold_CurrentGoldBecomesZero()
    {
        CallVoid("AddGold", 50);

        CallVoid("RemoveGold", 100);

        Assert.AreEqual(0, GetCurrentGold());
    }

    [Test]
    [Category("P1")]
    [Description("TC GL-MGR-006: RemoveGold với 0 không làm thay đổi CurrentGold.")]
    [TestCaseMeta(
        id: "GL-MGR-006",
        title: "RemoveGold: trừ vàng bằng 0",
        expected: "CurrentGold không đổi.",
        steps: "1) AddGold(100). 2) RemoveGold(0). 3) Kiểm tra CurrentGold == 100.")]
    public void RemoveGold_WhenAmountIsZero_DoesNotChangeCurrentGold()
    {
        CallVoid("AddGold", 100);

        CallVoid("RemoveGold", 0);

        Assert.AreEqual(100, GetCurrentGold());
    }

    [Test]
    [Category("P1")]
    [Description("TC GL-MGR-007: RemoveGold với số âm không làm thay đổi CurrentGold.")]
    [TestCaseMeta(
        id: "GL-MGR-007",
        title: "RemoveGold: trừ vàng số âm",
        expected: "CurrentGold không đổi (không cho phép trừ âm).",
        steps: "1) AddGold(100). 2) RemoveGold(-50). 3) Kiểm tra CurrentGold == 100.")]
    public void RemoveGold_WhenAmountIsNegative_DoesNotChangeCurrentGold()
    {
        CallVoid("AddGold", 100);

        CallVoid("RemoveGold", -50);

        Assert.AreEqual(100, GetCurrentGold());
    }

    [Test]
    [Category("P0")]
    [Description("TC GL-MGR-008: HasEnoughGold trả về true khi CurrentGold >= required.")]
    [TestCaseMeta(
        id: "GL-MGR-008",
        title: "HasEnoughGold: đủ vàng",
        expected: "Trả về true.",
        steps: "1) AddGold(100). 2) HasEnoughGold(80). 3) Assert true.")]
    public void HasEnoughGold_WhenCurrentGoldIsEnough_ReturnsTrue()
    {
        CallVoid("AddGold", 100);

        bool result = CallBool("HasEnoughGold", 80);

        Assert.IsTrue(result);
    }

    [Test]
    [Category("P0")]
    [Description("TC GL-MGR-009: HasEnoughGold trả về false khi CurrentGold < required.")]
    [TestCaseMeta(
        id: "GL-MGR-009",
        title: "HasEnoughGold: không đủ vàng",
        expected: "Trả về false.",
        steps: "1) AddGold(50). 2) HasEnoughGold(80). 3) Assert false.")]
    public void HasEnoughGold_WhenCurrentGoldIsNotEnough_ReturnsFalse()
    {
        CallVoid("AddGold", 50);

        bool result = CallBool("HasEnoughGold", 80);

        Assert.IsFalse(result);
    }

    [Test]
    [Category("P0")]
    [Description("TC GL-MGR-010: ResetGold đưa CurrentGold về 0.")]
    [TestCaseMeta(
        id: "GL-MGR-010",
        title: "ResetGold: reset vàng về 0",
        expected: "CurrentGold == 0.",
        steps: "1) AddGold(100). 2) ResetGold(). 3) Kiểm tra CurrentGold == 0.")]
    public void ResetGold_WhenCalled_SetsCurrentGoldToZero()
    {
        CallVoid("AddGold", 100);

        CallVoid("ResetGold");

        Assert.AreEqual(0, GetCurrentGold());
    }
}