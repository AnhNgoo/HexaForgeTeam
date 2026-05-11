using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Data", menuName = "Enemy/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Stats")]
    public float heath = 100f;
    public float damage = 10f;
    public float defense = 5f;
    public float moveSpeed = 3f;
}
