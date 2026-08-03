using Cysharp.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyCataclysmPillarsSkill",
    menuName = "Enemy/Skills/Cataclysm Pillars")]
public class EnemyCataclysmPillarsSkillSO : EnemyAttackSkillSO
{
    [SerializeField] private EnemyEarthPillarsSkillSO pillarsSkill;

    public override void OnAttackImpact(EnemyAttackContext context)
    {
        if (context != null && pillarsSkill != null)
            pillarsSkill.CastCataclysmAsync(context).Forget();
    }
}
