using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Data", menuName = "Enemy/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Stats")]
    public float maxHealth = 100f; //Máu tối đa
    public float damage = 10f; //Sát thương gây ra
    public float maxDefense = 5f; //Giá trị phòng thủ tối đa, có thể giảm sát thương nhận vào
    public float maxPoise = 50f; //Giá trị poise tối đa, khi bị tấn công sẽ giảm, nếu giảm xuống 0 sẽ bị stagger
    public float moveSpeed = 3f; //Tốc độ di chuyển
    public float patrolSpeed = 2f; //Tốc độ di chuyển khi đi tuần tra
    [Header("State Parameters")]
    public float staggerDuration = 2f; //Thời gian bị stagger khi poise giảm xuống 0
    public float detectRange = 10f; //Khoảng cách phát hiện mục tiêu
    public float loseTargetRange = 15f; //Khoảng cách mất mục tiêu
    public float povAngle = 90f; //Góc nhìn
    public float angularSpeed = 720f; //Tốc độ xoay mặt khi theo dõi mục tiêu
    [Header("Combat Rotation")]
    public float chaseAngularSpeed = 720f; //Tốc độ xoay khi đang Chase. Cao hơn để enemy bám player nhanh.
    public float attackTurnSpeed = 540f; //Tốc độ xoay thủ công trong Attack state. Không nên quá cao với enemy nặng.
    public float recoveryTurnSpeed = 360f; //Tốc độ xoay sau khi đánh xong / trong khoảng chờ cooldown.
    public float attackFacingAngle = 25f; //Enemy được phép đánh khi lệch góc bao nhiêu độ so với player.
    public float combatAwarenessDuration = 1f; //Thời gian mà Enemy vẫn giữ mục tiêu trong trạng thái nghi ngờ sau khi mất tầm nhìn, giúp Enemy không bị mất mục tiêu ngay lập tức khi player chạy ra khỏi tầm nhìn nhưng vẫn còn trong khoảng cách phát hiện
    [Header("Award Settings")]
    public int minGoldReward = 10; //Số lượng vàng thưởng khi tiêu diệt Enemy
    public int maxGoldReward = 20; //Số lượng vàng thưởng tối đa khi tiêu diệt Enemy
    public bool isBoss; //Công tắc để xác định nếu Enemy là boss, có thể dùng để điều chỉnh phần thưởng và hành vi của Enemy khi bị tiêu diệt
    [Header("Territory Settings")]
    public float maxLeashDistance = 25f; //Khoảng cách tối đa mà Enemy có thể rời khỏi vị trí spawn của nó, nếu vượt quá khoảng cách này sẽ tự động quay về vị trí spawn để tránh việc Enemy bị lạc quá xa và không thể tương tác với player
    public float roamRadius = 10f; //Bán kính mà Enemy có thể di chuyển xung quanh vị trí spawn khi không phát hiện mục tiêu, có thể dùng để tạo ra hành vi đi lang thang tự nhiên cho Enemy khi không có mục tiêu nào trong tầm nhìn
}
