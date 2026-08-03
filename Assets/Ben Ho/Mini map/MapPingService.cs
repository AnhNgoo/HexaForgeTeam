using System;
using UnityEngine;

public static class MapPingService
{
    public static event Action<Vector3> OnPingChanged;
    public static event Action OnPingCleared;

    public static bool HasPing { get; private set; }
    public static Vector3 PingWorldPosition { get; private set; }

    public static void SetPing(Vector3 worldPosition)
    {
        HasPing = true;
        PingWorldPosition = worldPosition;
        OnPingChanged?.Invoke(worldPosition);
    }

    public static void ClearPing()
    {
        HasPing = false;
        OnPingCleared?.Invoke();
    }
}