using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

internal static class EnemyTestUtils
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

    internal static T CreateScriptableObject<T>() where T : ScriptableObject
    {
        return ScriptableObject.CreateInstance<T>();
    }

    internal static Component AddComponent(GameObject go, string typeName)
    {
        var type = FindType(typeName);
        Assert.IsNotNull(type, $"Could not find type '{typeName}'. Ensure scripts compile in Unity.");
        return go.AddComponent(type);
    }

    internal static object GetProperty(object obj, string propertyName, BindingFlags flags)
    {
        var prop = obj.GetType().GetProperty(propertyName, flags);
        Assert.IsNotNull(prop, $"Property '{propertyName}' not found on {obj.GetType().Name}");
        return prop!.GetValue(obj);
    }

    internal static void AddEventHandler(object obj, string eventName, Delegate handler)
    {
        var ev = obj.GetType().GetEvent(eventName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        Assert.IsNotNull(ev, $"Event '{eventName}' not found on {obj.GetType().Name}");
        ev!.AddEventHandler(obj, handler);
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

    internal static void InvokeLoadComponent(Component component)
    {
        // LoadComponents.LoadComponentRuntime is called from Awake, but tests often need to force-cache after we set fields.
        Call(component, "LoadComponentRuntime", BindingFlags.Instance | BindingFlags.NonPublic);
    }

    internal static void EnsureMainCamera(Vector3 position, Vector3 lookAt)
    {
        var camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        var cam = camGo.AddComponent<Camera>();
        cam.transform.position = position;
        cam.transform.LookAt(lookAt);
    }

    internal static void EnsureLight(Vector3 eulerAngles)
    {
        var lightGo = new GameObject("Directional Light");
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Directional;
        lightGo.transform.eulerAngles = eulerAngles;
    }
}
