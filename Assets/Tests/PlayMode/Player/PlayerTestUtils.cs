using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

internal static class PlayerTestUtils
{
    internal static Type FindType(string typeName)
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

    internal static Component AddComponent(GameObject go, string typeName)
    {
        var type = FindType(typeName);
        Assert.IsNotNull(type, $"Could not find type '{typeName}'. Ensure scripts compile.");
        return go.AddComponent(type);
    }

    internal static T GetField<T>(object obj, string fieldName, BindingFlags flags)
    {
        var field = obj.GetType().GetField(fieldName, flags);
        Assert.IsNotNull(field, $"Field '{fieldName}' not found on {obj.GetType().Name}");
        return (T)field!.GetValue(obj);
    }

    internal static void SetField(object obj, string fieldName, object value, BindingFlags flags)
    {
        var field = obj.GetType().GetField(fieldName, flags);
        Assert.IsNotNull(field, $"Field '{fieldName}' not found on {obj.GetType().Name}");
        field!.SetValue(obj, value);
    }

    internal static object Call(object obj, string methodName, BindingFlags flags, params object[] args)
    {
        var method = obj.GetType().GetMethod(methodName, flags);
        Assert.IsNotNull(method, $"Method '{methodName}' not found on {obj.GetType().Name}");
        return method!.Invoke(obj, args);
    }

    internal static object GetProperty(object obj, string propertyName, BindingFlags flags)
    {
        var prop = obj.GetType().GetProperty(propertyName, flags);
        Assert.IsNotNull(prop, $"Property '{propertyName}' not found on {obj.GetType().Name}");
        return prop!.GetValue(obj);
    }

    internal static ScriptableObject CreateCharacterData(float speed = 6f, float attackSpeed = 100f)
    {
        var characterDataType = FindType("CharacterData");
        Assert.IsNotNull(characterDataType);

        var characterStatsType = FindType("CharacterStats");
        Assert.IsNotNull(characterStatsType);

        var data = ScriptableObject.CreateInstance(characterDataType!);
        var stats = Activator.CreateInstance(characterStatsType!);

        characterStatsType!.GetField("health")!.SetValue(stats, 100f);
        characterStatsType.GetField("speed")!.SetValue(stats, speed);
        characterStatsType.GetField("stamina")!.SetValue(stats, 100f);
        characterStatsType.GetField("attackSpeed")!.SetValue(stats, attackSpeed);

        characterDataType!.GetField("stats")!.SetValue(data, stats);
        return data;
    }

    internal static void InvokeLoadComponent(Component component)
    {
        // LoadComponents.LoadComponent is protected, so use non-public invoke
        Call(component, "LoadComponent", BindingFlags.Instance | BindingFlags.NonPublic);
    }

    internal static void EnsureMainCamera(Vector3 position, Vector3 lookAt)
    {
        var camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        var cam = camGo.AddComponent<Camera>();
        cam.transform.position = position;
        cam.transform.LookAt(lookAt);
    }

    internal static void EnsureGround(float y = 0f)
    {
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position = new Vector3(0f, y, 0f);
    }

    internal static Component EnsureSingleton(string typeName)
    {
        var go = new GameObject(typeName);
        var comp = AddComponent(go, typeName);
        // must be active so Awake sets Instance
        go.SetActive(true);
        return comp;
    }
}
