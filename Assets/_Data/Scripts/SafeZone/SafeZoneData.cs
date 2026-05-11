using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SafeZoneData", menuName = "ScriptableObjects/SafeZoneData", order = 1)]
public class SafeZoneData : ScriptableObject
{
    public float startRadius;
    public SafeZoneStat safeZoneStat;
}

[Serializable]
public class SafeZoneStat
{
    public float timeDelay;
    public float shrinkDuration;
    public float radius;
}

