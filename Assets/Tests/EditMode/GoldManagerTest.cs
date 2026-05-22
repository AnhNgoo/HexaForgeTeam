using NUnit.Framework;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

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
    public void AddGold_WhenAmountPositive_IncreasesCurrentGold()
    {
        CallVoid("AddGold", 100);

        Assert.AreEqual(100, GetCurrentGold());
    }

    [Test]
    public void AddGold_WhenAmountIsZero_DoesNotChangeCurrentGold()
    {
        CallVoid("AddGold", 0);

        Assert.AreEqual(0, GetCurrentGold());
    }

    [Test]
    public void AddGold_WhenAmountIsNegative_DoesNotChangeCurrentGold()
    {
        CallVoid("AddGold", -50);

        Assert.AreEqual(0, GetCurrentGold());
    }

    [Test]
    public void RemoveGold_WhenAmountPositive_DecreasesCurrentGold()
    {
        CallVoid("AddGold", 100);

        CallVoid("RemoveGold", 40);

        Assert.AreEqual(60, GetCurrentGold());
    }

    [Test]
    public void RemoveGold_WhenAmountIsGreaterThanCurrentGold_CurrentGoldBecomesZero()
    {
        CallVoid("AddGold", 50);

        CallVoid("RemoveGold", 100);

        Assert.AreEqual(0, GetCurrentGold());
    }

    [Test]
    public void RemoveGold_WhenAmountIsZero_DoesNotChangeCurrentGold()
    {
        CallVoid("AddGold", 100);

        CallVoid("RemoveGold", 0);

        Assert.AreEqual(100, GetCurrentGold());
    }

    [Test]
    public void RemoveGold_WhenAmountIsNegative_DoesNotChangeCurrentGold()
    {
        CallVoid("AddGold", 100);

        CallVoid("RemoveGold", -50);

        Assert.AreEqual(100, GetCurrentGold());
    }

    [Test]
    public void HasEnoughGold_WhenCurrentGoldIsEnough_ReturnsTrue()
    {
        CallVoid("AddGold", 100);

        bool result = CallBool("HasEnoughGold", 80);

        Assert.IsTrue(result);
    }

    [Test]
    public void HasEnoughGold_WhenCurrentGoldIsNotEnough_ReturnsFalse()
    {
        CallVoid("AddGold", 50);

        bool result = CallBool("HasEnoughGold", 80);

        Assert.IsFalse(result);
    }

    [Test]
    public void ResetGold_WhenCalled_SetsCurrentGoldToZero()
    {
        CallVoid("AddGold", 100);

        CallVoid("ResetGold");

        Assert.AreEqual(0, GetCurrentGold());
    }
}