using System.Collections.Generic;
using UnityEngine;

public class TwilightTerrorSpawnPointGroup : MonoBehaviour
{
    [SerializeField] private List<Transform> minionSpawnPoints = new();
    [SerializeField] private GameObject finalBossPortal;

    public Transform BossSpawnPoint => transform;
    public IReadOnlyList<Transform> MinionSpawnPoints => minionSpawnPoints;

    public void ShowFinalBossPortal()
    {
        if (finalBossPortal != null)
            finalBossPortal.SetActive(true);
    }
}