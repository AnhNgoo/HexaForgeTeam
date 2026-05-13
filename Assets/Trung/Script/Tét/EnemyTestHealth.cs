using UnityEngine;

public class EnemyTestHealth : MonoBehaviour
{
    [Header("Máu")]
    [SerializeField] private float maxHP = 100f;

    [Header("Debug")]
    [SerializeField] private KeyCode testDamageKey = KeyCode.K;
    [SerializeField] private float testDamageAmount = 25f;

    private float currentHP;

    private EnemyBase enemyBase;

    private void Awake()
    {
        enemyBase = GetComponent<EnemyBase>();

        currentHP = maxHP;
    }

    private void Update()
    {
        if (Input.GetKeyDown(testDamageKey))
        {
            TakeDamage(testDamageAmount);
        }
    }

    public void TakeDamage(float damageAmount)
    {
        currentHP -= damageAmount;

        Debug.Log($"{gameObject.name} nhận {damageAmount} damage | HP: {currentHP}");

        if (currentHP <= 0)
        {
            Dead();
        }
    }

    private void Dead()
    {
        Debug.Log($"{gameObject.name} đã chết");

        enemyBase.EventManager.CallDead();

        gameObject.SetActive(false);
    }
}