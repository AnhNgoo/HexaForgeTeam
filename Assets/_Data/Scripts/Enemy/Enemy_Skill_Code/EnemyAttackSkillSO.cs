using UnityEngine;

public abstract class EnemyAttackSkillSO : ScriptableObject
{
    public virtual void OnAttackStart(EnemyAttackContext context) { }
    public virtual void OnAttackMovement(EnemyAttackContext context) { }
    public virtual void OnAttackImpact(EnemyAttackContext context) { }
    public virtual void OnAttackEnd(EnemyAttackContext context) { }
}
