using UnityEngine;

public class EnemyState
{
    protected EnemyBase _enemyBase;

    public EnemyState(EnemyBase enemyBase)
    {
        _enemyBase = enemyBase;
    }

    public virtual void Enter() { }

    public virtual void UpdateLogic() { }

    public virtual void Exit() { }
}
