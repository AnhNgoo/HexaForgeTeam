using UnityEngine;

public class EnemyLootDropper : MonoBehaviour
{
    private EnemyBase _enemyBase;
    [SerializeField] private GameObject _lootPrefab; //Prefab của item loot để thả ra khi Enemy chết, có thể điều chỉnh để thả các loại item khác nhau tùy thuộc vào loại Enemy hoặc ngẫu nhiên
    private bool _isSubscribed;
    public void Initialize(EnemyBase enemyBase)
    {
        _enemyBase = enemyBase;
    }

    private void OnEnable()
    {
        if (_isSubscribed || _enemyBase == null || _enemyBase.EventManager == null) return; // Kiểm tra nếu đã đăng ký hoặc _enemyBase hoặc EventManager chưa được gán để tránh lỗi
        _enemyBase.EventManager.OnDead += AwardEnemy; //Đăng ký hàm AwardEnemy vào sự kiện OnDead để xử lý việc thưởng khi Enemy chết, cần đảm bảo rằng lambda được đăng ký đúng cách nếu dùng lambda để đăng ký
        _isSubscribed = true; // Đánh dấu đã đăng ký để tránh đăng ký lại nhiều lần
    }

    private void OnDisable()
    {
        if (!_isSubscribed || _enemyBase == null || _enemyBase.EventManager == null) return; // Kiểm tra nếu chưa đăng ký hoặc _enemyBase hoặc EventManager chưa được gán để tránh lỗi
        _enemyBase.EventManager.OnDead -= AwardEnemy; //Hủy đăng ký hàm AwardEnemy khỏi sự kiện OnDead khi Enemy bị vô hiệu hóa để tránh lỗi khi Enemy bị hủy hoặc vô hiệu hóa
        _isSubscribed = false; // Đánh dấu đã hủy đăng ký
    }

    private void GoldReceived()
    {
        int minGold = _enemyBase.Data.minGoldReward; //Lấy giá trị vàng tối thiểu từ dữ liệu của Enemy để tính toán số vàng thưởng, có thể điều chỉnh để lấy giá trị từ các nguồn khác nếu muốn
        int maxGold = _enemyBase.Data.maxGoldReward; //Lấy giá trị vàng tối đa từ dữ liệu của Enemy để tính toán số vàng thưởng, có thể điều chỉnh để lấy giá trị từ các nguồn khác nếu muốn

        if (maxGold < minGold)
        {
            maxGold = minGold; //Nếu giá trị vàng tối đa nhỏ hơn giá trị vàng tối thiểu, đặt giá trị vàng tối đa bằng giá trị vàng tối thiểu để tránh lỗi khi tính toán số vàng thưởng
        }

        int goldAmount = Random.Range(minGold, maxGold + 1); //Tạo số vàng thưởng ngẫu nhiên trong khoảng từ minGold đến maxGold, có thể điều chỉnh để tạo ra các loại phần thưởng khác nhau tùy thuộc vào loại Enemy hoặc ngẫu nhiên

        GoldManager.Instance?.AddGold(goldAmount); //Gọi hàm AddGold của GoldManager để cộng số vàng thưởng vào tổng số vàng của player, có thể mở rộng sau này để thêm các loại phần thưởng khác như item hoặc điểm kinh nghiệm

        DebugNote.Green($"Player nhận được {goldAmount} vàng từ Enemy {gameObject.name}!");
    }

    private void AwardEnemy()
    {
        GoldReceived(); //Gọi hàm GoldReceived để xử lý việc thưởng vàng cho player khi Enemy chết, có thể mở rộng sau này để thêm các loại phần thưởng khác như item hoặc điểm kinh nghiệm
        if (_enemyBase.Data.isBoss)
        {
            DropItem(); //Gọi hàm DropItem để xử lý việc thả item khi Enemy chết, có thể mở rộng sau này để thêm logic thả item ngẫu nhiên hoặc theo tỷ lệ nhất định
        }
    }

    private void DropItem()
    {
        if (_lootPrefab == null)
        {
            DebugNote.Red($"Loot prefab chưa được gán cho {gameObject.name} - EnemyLootDropper, không thể thả item!");
            return; // Kiểm tra nếu loot prefab chưa được gán để tránh lỗi khi cố gắng tạo instance của item loot
        }
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
