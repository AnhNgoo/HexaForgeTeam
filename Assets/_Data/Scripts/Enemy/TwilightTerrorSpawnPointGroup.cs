using System.Collections.Generic;
using UnityEngine;

public class TwilightTerrorSpawnPointGroup : MonoBehaviour
{
    [SerializeField] private List<Transform> minionSpawnPoints = new();

    public Transform BossSpawnPoint => transform;
    public IReadOnlyList<Transform> MinionSpawnPoints => minionSpawnPoints;
}