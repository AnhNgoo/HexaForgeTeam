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
    public float timeDelay;
    public float shrinkDuration;
    public float radius;
}

