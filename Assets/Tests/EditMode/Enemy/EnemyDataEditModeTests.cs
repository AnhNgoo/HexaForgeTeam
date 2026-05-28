using NUnit.Framework;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

[TestReport]
[Category("EditMode")]
[Category("Enemy")]
public class EnemyDataEditModeTests
{
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

    private static float GetFloatField(object instance, string fieldName)
    {
        var field = instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        Assert.IsNotNull(field, $"Missing field '{fieldName}' on type '{instance.GetType().FullName}'.");
        return Convert.ToSingle(field!.GetValue(instance));
    }

    [Test]
    [Category("P1")]
    [Description("TC EN-DATA-001: Kiểm tra giá trị mặc định của EnemyData không âm (các thông số cơ bản phải >= 0).")]
    [TestCaseMeta(
        id: "EN-DATA-001",
        title: "EnemyData giá trị mặc định không âm",
        expected: "Các stats/parameter >= 0.",
        steps: "1) CreateInstance EnemyData. 2) Assert các field >= 0.")]
    public void EnemyData_Defaults_AreNonNegative()
    {
        var enemyDataType = FindType("EnemyData");
        Assert.IsNotNull(enemyDataType, "Could not find type 'EnemyData' via reflection. Ensure scripts compile.");

        var data = ScriptableObject.CreateInstance(enemyDataType!);
        Assert.IsNotNull(data);

        Assert.GreaterOrEqual(GetFloatField(data, "maxHealth"), 0f);
        Assert.GreaterOrEqual(GetFloatField(data, "damage"), 0f);
        Assert.GreaterOrEqual(GetFloatField(data, "maxDefense"), 0f);
        Assert.GreaterOrEqual(GetFloatField(data, "maxPoise"), 0f);
        Assert.GreaterOrEqual(GetFloatField(data, "moveSpeed"), 0f);
        Assert.GreaterOrEqual(GetFloatField(data, "patrolSpeed"), 0f);
        Assert.GreaterOrEqual(GetFloatField(data, "detectRange"), 0f);
        Assert.GreaterOrEqual(GetFloatField(data, "loseTargetRange"), 0f);
        Assert.GreaterOrEqual(GetFloatField(data, "povAngle"), 0f);
        Assert.GreaterOrEqual(GetFloatField(data, "attackCooldown"), 0f);
        Assert.GreaterOrEqual(GetFloatField(data, "staggerDuration"), 0f);
    }

    [Test]
    [Category("P1")]
    [Description("TC EN-DATA-002: Kiểm tra AttackDataSO có các thông số mặc định không âm để tránh lỗi cân bằng/thiết kế.")]
    [TestCaseMeta(
        id: "EN-DATA-002",
        title: "AttackDataSO giá trị mặc định không âm",
        expected: "Các modifier >= 0.",
        steps: "1) CreateInstance AttackDataSO. 2) Assert các field >= 0.")]
    public void AttackData_Defaults_AreNonNegative_AndHaveStateName()
    {
        var attackDataType = FindType("AttackDataSO");
        Assert.IsNotNull(attackDataType, "Could not find type 'AttackDataSO' via reflection. Ensure scripts compile.");

        var attack = ScriptableObject.CreateInstance(attackDataType!);
        Assert.IsNotNull(attack);

        Assert.GreaterOrEqual(GetFloatField(attack, "transitionDuration"), 0f);
        Assert.GreaterOrEqual(GetFloatField(attack, "attackDuration"), 0f);
        Assert.GreaterOrEqual(GetFloatField(attack, "damageMultiplier"), 0f);
        Assert.GreaterOrEqual(GetFloatField(attack, "poiseDamage"), 0f);
        Assert.GreaterOrEqual(GetFloatField(attack, "attackRange"), 0f);
    }
}
