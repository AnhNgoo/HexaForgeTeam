using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDetection : MonoBehaviour
{
    private EnemyBase _enemyBase;
    public void Initialize(EnemyBase enemyBase)
    {
        _enemyBase = enemyBase;
        Debug.Log($"{gameObject.name} - EnemyDetection đã được khởi tạo!");
    }
}
