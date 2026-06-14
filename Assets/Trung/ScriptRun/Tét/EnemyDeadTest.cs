using UnityEngine;

public class EnemyDeadTest : MonoBehaviour
{
    private EnemyBase enemyBase;

    private void Awake()
    {
        enemyBase = GetComponent<EnemyBase>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            enemyBase.EventManager.CallDead();
        }
    }
}