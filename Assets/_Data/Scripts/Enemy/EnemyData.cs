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
    public float attackCooldown = 1.5f; //Thời gian giữa các đòn tấn công
}
