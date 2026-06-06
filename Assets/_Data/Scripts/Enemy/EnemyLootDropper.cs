using UnityEngine;

public class EnemyLootDropper : MonoBehaviour
{
    private EnemyBase _enemyBase;
    [SerializeField] private GameObject _lootPrefab; //Prefab của item loot để thả ra khi Enemy chết, có thể điều chỉnh để thả các loại item khác nhau tùy thuộc vào loại Enemy hoặc ngẫu nhiên

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
        Vector3 dropPosition = _enemyBase.MyTransform.position + Vector3.up * 0.5f; //Lấy vị trí của Enemy để làm vị trí thả item, có thể điều chỉnh để thả item ở vị trí khác nếu muốn (ví dụ: thả item ở vị trí gần player hơn hoặc thả item ở vị trí ngẫu nhiên xung quanh Enemy)
        GameObject lootObj = Instantiate(_lootPrefab, dropPosition, Quaternion.identity); //Tạo instance của item loot tại vị trí thả, có thể điều chỉnh để tạo ra các loại item khác nhau tùy thuộc vào loại Enemy hoặc ngẫu nhiên

        Rigidbody lootRb = lootObj.GetComponent<Rigidbody>();
        if (lootRb != null)
        {
            float randomX = Random.Range(-1f, 1f); //Tạo lực đẩy ngẫu nhiên trên trục X để làm cho item loot bay ra một cách tự nhiên thay vì chỉ rơi thẳng xuống đất
            float randomZ = Random.Range(-1f, 1f); //Tạo lực đẩy ngẫu nhiên trên trục Z để làm cho item loot bay ra một cách tự nhiên thay vì chỉ rơi thẳng xuống đất
            Vector3 dropDirection = new Vector3(randomX, 1.5f, randomZ).normalized; //Tạo vector lực đẩy dựa trên các giá trị ngẫu nhiên và chuẩn hóa để đảm bảo rằng lực đẩy có cùng cường độ bất kể hướng nào

            lootRb.AddForce(dropDirection * 5f, ForceMode.Impulse); //Áp dụng lực đẩy lên item loot để làm cho nó bay ra một cách tự nhiên, có thể điều chỉnh cường độ lực đẩy để tạo ra hiệu ứng thả item phù hợp với loại Enemy hoặc loại item
            lootRb.AddTorque(Random.insideUnitSphere * 5f, ForceMode.Impulse); //Áp dụng lực xoay ngẫu nhiên lên item loot để làm cho nó quay khi bay ra, tạo thêm hiệu ứng động cho item loot khi thả ra
        }

        DebugNote.Green($"Item loot đã được thả ra tại vị trí {dropPosition}!");
    }
}
