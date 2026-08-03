using Sirenix.OdinInspector;
using UnityEngine;

public sealed class FinalBossEncounterTestSpawner : MonoBehaviour
{
    [SerializeField] private FinalBossArena arena;
    [SerializeField] private FinalBossEncounterDirector director;
    [SerializeField] private CharacterData playerData;

    private CharacterBase _spawnedPlayer;

    [Button("TEST: Spawn Player + Final Boss")]
    private void SpawnTestEncounter()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[FinalBossTest] Chỉ dùng nút khi đang Play.");
            return;
        }

        if (ObjectPooling.Instance == null ||
            arena == null ||
            arena.PlayerSpawnPoint == null ||
            director == null ||
            playerData == null)
        {
            Debug.LogError(
                "[FinalBossTest] Thiếu ObjectPooling, Arena, " +
                "PlayerSpawnPoint, Director hoặc PlayerData.");
            return;
        }

        if (_spawnedPlayer == null)
        {
            GameObject playerObject =
                ObjectPooling.Instance.SpawnFromPool(
                    playerData.characterPoolType,
                    arena.PlayerSpawnPoint.position,
                    arena.PlayerSpawnPoint.rotation
                );

            _spawnedPlayer = playerObject != null
                ? playerObject.GetComponent<CharacterBase>()
                : null;

            if (_spawnedPlayer == null)
            {
                Debug.LogError(
                    "[FinalBossTest] Không spawn được CharacterBase.");
                return;
            }

            _spawnedPlayer.gameObject.tag = "Player";
            _spawnedPlayer.Init(Instantiate(playerData));
        }

        director.StartEncounter();
    }
}