using System;
using System.Reflection;
using UnityEngine;

namespace DuskBlade.Tests
{
    public static class TestReflectionHelper
    {
        private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        public static Component FindComponentByClassName(GameObject root, string className)
        {
            if (root == null || string.IsNullOrWhiteSpace(className))
            {
                return null;
            }

            try
            {
                Type requestedType = ResolveType(className);
                Component[] components = root.GetComponentsInChildren<Component>(true);
                foreach (Component component in components)
                {
                    if (component == null)
                    {
                        continue;
                    }

                    Type type = component.GetType();
                    if (type.Name == className || type.FullName == className)
                    {
                        return component;
                    }

                    if (requestedType != null && requestedType.IsAssignableFrom(type))
                    {
                        return component;
                    }
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"TestReflectionHelper.FindComponentByClassName failed: {exception.Message}");
            }

            return null;
        }

        public static bool TryGetValue(object target, string memberName, out object value)
        {
            value = null;

            if (target == null || string.IsNullOrWhiteSpace(memberName))
            {
                return false;
            }

            try
            {
                Type type = target.GetType();
                FieldInfo field = FindField(type, memberName);
                if (field != null)
                {
                    value = field.GetValue(target);
                    return true;
                }

                PropertyInfo property = FindProperty(type, memberName);
                if (property != null && property.GetIndexParameters().Length == 0 && property.CanRead)
                {
                    value = property.GetValue(target, null);
                    return true;
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"TestReflectionHelper.TryGetValue failed: {exception.Message}");
            }

            return false;
        }

        public static bool TryGetValue<T>(object target, string memberName, out T value)
        {
            value = default(T);

            if (!TryGetValue(target, memberName, out object rawValue))
            {
                return false;
            }

            try
            {
                if (rawValue is T typedValue)
                {
                    value = typedValue;
                    return true;
                }

                if (rawValue != null)
                {
                    value = (T)Convert.ChangeType(rawValue, typeof(T));
                    return true;
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"TestReflectionHelper.TryGetValue<{typeof(T).Name}> failed: {exception.Message}");
            }

            return false;
        }

        public static bool TrySetValue(object target, string memberName, object value)
        {
            if (target == null || string.IsNullOrWhiteSpace(memberName))
            {
                return false;
            }

            try
            {
                Type type = target.GetType();
                FieldInfo field = FindField(type, memberName);
                if (field != null)
                {
                    field.SetValue(target, ConvertValue(value, field.FieldType));
                    return true;
                }

                PropertyInfo property = FindProperty(type, memberName);
                if (property != null && property.GetIndexParameters().Length == 0 && property.CanWrite)
                {
                    property.SetValue(target, ConvertValue(value, property.PropertyType), null);
                    return true;
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"TestReflectionHelper.TrySetValue failed: {exception.Message}");
            }

            return false;
        }

        public static bool TryInvokeMethod(object target, string methodName, out object result, params object[] args)
        {
            result = null;

            if (target == null || string.IsNullOrWhiteSpace(methodName))
            {
                return false;
            }

            try
            {
                MethodInfo method = FindMethod(target.GetType(), methodName, args);
                if (method == null)
                {
                    return false;
                }

                result = method.Invoke(target, args);
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"TestReflectionHelper.TryInvokeMethod failed: {exception.Message}");
            }

            return false;
        }

        public static bool TryInvokeMethod(object target, string methodName, params object[] args)
        {
            return TryInvokeMethod(target, methodName, out _, args);
        }

        private static FieldInfo FindField(Type type, string name)
        {
            while (type != null)
            {
                FieldInfo field = type.GetField(name, Flags);
                if (field != null)
                {
                    return field;
                }

                type = type.BaseType;
            }

            return null;
        }

        private static Type ResolveType(string className)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType(className);
                if (type != null)
                {
                    return type;
                }

                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException exception)
                {
                    types = exception.Types;
                }

                foreach (Type candidate in types)
                {
                    if (candidate != null &&
                        (candidate.Name == className || candidate.FullName == className))
                    {
                        return candidate;
                    }
                }
            }

            return null;
        }

        private static PropertyInfo FindProperty(Type type, string name)
        {
            while (type != null)
            {
                PropertyInfo property = type.GetProperty(name, Flags);
                if (property != null)
                {
                    return property;
                }

                type = type.BaseType;
            }

            return null;
        }

        private static MethodInfo FindMethod(Type type, string name, object[] args)
        {
            while (type != null)
            {
                foreach (MethodInfo method in type.GetMethods(Flags))
                {
                    if (method.Name != name)
                    {
                        continue;
                    }

                    ParameterInfo[] parameters = method.GetParameters();
                    int argCount = args == null ? 0 : args.Length;
                    if (parameters.Length == argCount)
                    {
                        return method;
                    }
                }

                type = type.BaseType;
            }

            return null;
        }

        private static object ConvertValue(object value, Type targetType)
        {
            if (value == null || targetType.IsInstanceOfType(value))
            {
                return value;
            }

            if (targetType.IsEnum)
            {
                return Enum.Parse(targetType, value.ToString());
            }

            return Convert.ChangeType(value, targetType);
        }
    }
}
