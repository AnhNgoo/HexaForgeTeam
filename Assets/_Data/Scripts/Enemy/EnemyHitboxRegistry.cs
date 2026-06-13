using UnityEngine;
using System;

[Serializable]
public class EnemyHitboxEntry
{
    public EnemyHitboxType type;
    public EnemyHitbox hitbox;
}

public class EnemyHitboxRegistry : MonoBehaviour
{
    private EnemyBase _enemyBase;
    [SerializeField] private EnemyHitboxEntry[] hitboxEntries;

    public EnemyHitbox GetHitbox(EnemyHitboxType type)
    {
        foreach (var entry in hitboxEntries)
        {
            if (entry.type == type)
            {
                return entry.hitbox;
            }
        }
        return null;
    }

    public void DisableAllHitboxes()
    {
        foreach (var entry in hitboxEntries)
        {
            if (entry.hitbox != null)
            {
                entry.hitbox.DisableHitBox();
            }
        }
    }

    public void Initialize(EnemyBase enemyBase)
    {
        _enemyBase = enemyBase;

        foreach (var entry in hitboxEntries)
        {
            if (entry != null && entry.hitbox != null)
            {
                entry.hitbox.Initialize(enemyBase);
                entry.hitbox.DisableHitBox();
            }
        }
    }


}
