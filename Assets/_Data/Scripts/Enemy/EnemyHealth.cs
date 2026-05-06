using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    private EnemyBase _enemyBase;
    public void Initialize(EnemyBase enemyBase)
    {
        _enemyBase = enemyBase;
        Debug.Log($"{gameObject.name} - EnemyHealth đã được khởi tạo!");
    }
}
