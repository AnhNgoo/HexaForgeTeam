using UnityEngine;

public sealed class EnemyTargetDummy : MonoBehaviour, ITakeDamage
{
    [SerializeField, Min(1f)]
    private float maxHealth = 10000f;

    [SerializeField]
    private bool logDamage = true;

    private float _currentHealth;

    public bool CanBeEngaged =>
        isActiveAndEnabled &&
        gameObject.activeInHierarchy &&
        _currentHealth > 0f;

    public float CurrentHealth => _currentHealth;

    private void OnEnable()
    {
        ResetHealth();
    }

    [ContextMenu("Reset Health")]
    public void ResetHealth()
    {
        _currentHealth = maxHealth;
    }

    public void TakeDamage(DamageInfo damageInfo)
    {
        if (!CanBeEngaged || damageInfo == null)
            return;

        float receivedDamage =
            Mathf.Max(0f, damageInfo.damageAmount);

        _currentHealth = Mathf.Max(
            0f,
            _currentHealth - receivedDamage
        );

        if (logDamage)
        {
            Debug.Log(
                $"[EnemyTargetDummy] {name} nhận " +
                $"{receivedDamage:F1} damage. " +
                $"HP: {_currentHealth:F1}/{maxHealth:F1}",
                this
            );
        }
    }
}