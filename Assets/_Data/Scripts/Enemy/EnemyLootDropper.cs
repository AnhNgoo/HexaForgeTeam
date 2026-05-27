using UnityEngine;

public class EnemyLootDropper : MonoBehaviour
{
    private EnemyBase _enemyBase;

    public void Initialize(EnemyBase enemyBase)
    {
        _enemyBase = enemyBase;
        if (_enemyBase.EventManager != null)
        {
            _enemyBase.EventManager.OnDead += AwardEnemy; //Đăng ký hàm AwardEnemy vào sự kiện OnDead của EnemyEventManager để đảm bảo rằng loot sẽ được thả ra khi Enemy chết
        }
        Debug.Log($"{gameObject.name} - EnemyLootDropper đã được khởi tạo!");
    }

    private void OnDisable()
    {
        if (_enemyBase != null && _enemyBase.EventManager != null)
        {
            _enemyBase.EventManager.OnDead -= AwardEnemy; //Hủy đăng ký hàm AwardEnemy khỏi sự kiện OnDead khi Enemy bị vô hiệu hóa để tránh lỗi khi Enemy bị hủy hoặc vô hiệu hóa
        }
    }

    private void GoldReceived()
    {
        float goldAmount = Random.Range(_enemyBase.Data.minGoldReward, _enemyBase.Data.maxGoldReward + 1); //Tính toán số lượng vàng thưởng ngẫu nhiên dựa trên min và max gold reward trong EnemyData, cộng thêm 1 vào max để đảm bảo rằng giá trị max cũng có thể được chọn
        DebugNote.Yellow($"Player nhận được {goldAmount} vàng từ việc tiêu diệt {gameObject.name}!");
    }

    private void AwardEnemy()
    {
        GoldReceived(); //Gọi hàm GoldReceived để xử lý việc thưởng vàng cho player khi Enemy chết, có thể mở rộng sau này để thêm các loại phần thưởng khác như item hoặc điểm kinh nghiệm
        DropItem(); //Gọi hàm DropItem để xử lý việc thả item khi Enemy chết, có thể mở rộng sau này để thêm logic thả item ngẫu nhiên hoặc theo tỷ lệ nhất định
    }

    private void DropItem()
    {
        Vector3 dropPosition = _enemyBase.MyTransform.position + Vector3.up * 0.5f; //Vị trí thả item, có thể điều chỉnh để thả item ở vị trí khác so với vị trí của Enemy nếu cần thiết
        DebugNote.Green("Item đã được thả ra tại vị trí: " + dropPosition);
    }
}
