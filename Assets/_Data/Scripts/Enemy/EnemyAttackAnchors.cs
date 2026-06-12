using UnityEngine;

public class EnemyAttackAnchors : MonoBehaviour
{
    private EnemyBase _enemyBase;
    [Header("Attack Anchor Transforms")]
    [SerializeField] private Transform _rootAnchor; // Điểm neo gốc của Enemy, có thể được thiết lập trong Inspector để xác định vị trí gắn hiệu ứng khi tấn công
    [SerializeField] private Transform _mouthAnchor; // Điểm neo miệng của Enemy, có thể được thiết lập trong Inspector để xác định vị trí gắn hiệu ứng khi tấn công
    [SerializeField] private Transform _chestAnchor; // Điểm neo ngực của Enemy, có thể được thiết lập trong Inspector để xác định vị trí gắn hiệu ứng khi tấn công
    [SerializeField] private Transform _headAnchor; // Điểm neo đầu của Enemy, có thể được thiết lập trong Inspector để xác định vị trí gắn hiệu ứng khi tấn công
    [SerializeField] private Transform _legAnchor; // Điểm neo chân của Enemy, có thể được thiết lập trong Inspector để xác định vị trí gắn hiệu ứng khi tấn công
    [SerializeField] private Transform _handAnchor; // Điểm neo tay của Enemy, có thể được thiết lập trong Inspector để xác định vị trí gắn hiệu ứng khi tấn công
    [SerializeField] private Transform _weaponAnchor; // Điểm neo vũ khí của Enemy, có thể được thiết lập trong Inspector để xác định vị trí gắn hiệu ứng khi tấn công
    [SerializeField] private Transform _projectileSpawnAnchor; // Điểm xuất hiện của projectile khi Enemy sử dụng kỹ năng tấn công tầm xa, có thể được thiết lập trong Inspector để xác định vị trí xuất hiện của projectile khi tấn công

    public void Initialize(EnemyBase enemyBase)
    {
        _enemyBase = enemyBase;
    }

    public Transform GetAnchor(EnemyAttackAnchorType anchorType)
    {
        return anchorType switch
        {
            EnemyAttackAnchorType.Root => _rootAnchor != null ? _rootAnchor : transform,
            EnemyAttackAnchorType.Mouth => _mouthAnchor != null ? _mouthAnchor : transform,
            EnemyAttackAnchorType.Chest => _chestAnchor != null ? _chestAnchor : transform,
            EnemyAttackAnchorType.Head => _headAnchor != null ? _headAnchor : transform,
            EnemyAttackAnchorType.Leg => _legAnchor != null ? _legAnchor : transform,
            EnemyAttackAnchorType.Hand => _handAnchor != null ? _handAnchor : transform,
            EnemyAttackAnchorType.Weapon => _weaponAnchor != null ? _weaponAnchor : transform,
            EnemyAttackAnchorType.ProjectileSpawn => _projectileSpawnAnchor != null ? _projectileSpawnAnchor : transform,
            _ => transform
        };
    }

}
