using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DebugNote
{
    public static void Red(string message)
    {
        Debug.Log($"<color=red>{message}</color>");
    }

    public static void Green(string message)
    {
        Debug.Log($"<color=green>{message}</color>");
    }

    public static void Blue(string message)
    {
        Debug.Log($"<color=blue>{message}</color>");
    }

    public static void Yellow(string message)
    {
        Debug.Log($"<color=yellow>{message}</color>");
    }
}
