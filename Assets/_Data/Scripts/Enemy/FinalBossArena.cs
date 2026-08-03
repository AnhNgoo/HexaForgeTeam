using UnityEngine;

public class FinalBossArena : MonoBehaviour
{
    [SerializeField] private Transform center;
    [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private Transform bossSpawnPoint;
    [SerializeField, Min(1f)] private float radius = 25f;
    [SerializeField] private GameObject[] barriers;

    public Transform PlayerSpawnPoint => playerSpawnPoint;
    public Transform BossSpawnPoint => bossSpawnPoint;

    public bool IsNearWall(Vector3 position, float distance)
    {
        if (center == null) return false;
        Vector3 delta = position - center.position;
        delta.y = 0f;
        return delta.magnitude >= Mathf.Max(0f, radius - distance);
    }

    public void SetLocked(bool locked)
    {
        foreach (GameObject barrier in barriers)
            if (barrier != null) barrier.SetActive(locked);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (center == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(center.position, radius);
    }
#endif
}
