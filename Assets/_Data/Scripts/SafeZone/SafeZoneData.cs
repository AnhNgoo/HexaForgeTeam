using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SafeZoneData", menuName = "ScriptableObjects/SafeZoneData", order = 1)]
public class SafeZoneData : ScriptableObject
{
    public float startRadius = 120f;
    public List<SafeZoneStat> safeZoneStats = new();
}

[Serializable]
public class SafeZoneStat
{
    [Tooltip("Thời gian delay trước khi vòng bo bắt đầu thu nhỏ")]
    public float timeDelay;
    [Tooltip("Thời gian thu nhỏ vòng bo")]
    public float shrinkDuration;
    [Tooltip("Bán kính của vòng bo")]
    public float radius;
}

