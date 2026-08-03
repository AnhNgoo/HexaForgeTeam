using System;
using System.Collections.Generic;
using UnityEngine;

public class RuntimeMapStructure : MonoBehaviour
{
    public static event Action<RuntimeMapStructure> Registered;
    public static event Action<RuntimeMapStructure> Unregistered;

    private static readonly List<RuntimeMapStructure> activeStructures =
        new List<RuntimeMapStructure>();

    public static IReadOnlyList<RuntimeMapStructure> ActiveStructures =>
        activeStructures;

    [Header("Map Information")]
    [SerializeField] private string locationName;
    [SerializeField] private MapLocationType locationType;
    [SerializeField, TextArea] private string description;
    [SerializeField] private Sprite icon;
    [SerializeField] private bool discovered = true;
    [SerializeField] private float identifyRadius = 40f;

    private MapLocationData runtimeData;

    public MapLocationData LocationData
    {
        get
        {
            if (runtimeData == null)
                CreateRuntimeData();

            runtimeData.worldPosition = transform.position;
            return runtimeData;
        }
    }

    private void OnEnable()
    {
        if (!activeStructures.Contains(this))
            activeStructures.Add(this);

        CreateRuntimeData();
        Registered?.Invoke(this);
    }

    private void OnDisable()
    {
        activeStructures.Remove(this);
        Unregistered?.Invoke(this);
    }

    private void CreateRuntimeData()
    {
        runtimeData = new MapLocationData
        {
            locationName = string.IsNullOrWhiteSpace(locationName)
                ? gameObject.name.Replace("(Clone)", "")
                : locationName,

            locationType = locationType,
            description = description,
            worldPosition = transform.position,
            icon = icon,
            discovered = discovered,
            identifyRadius = identifyRadius
        };
    }

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        activeStructures.Clear();
        Registered = null;
        Unregistered = null;
    }
}