using UnityEngine;

public class BruteBossBehaviour : EnemyBossBehaviour
{
    [SerializeField, Range(0f, 1f)] private float titanSkinDamageReduction = 0.3f;

    public override float ModifyIncomingDamage(float damage, Transform attacker)
    {
        return damage * (1f - titanSkinDamageReduction);
    }
}