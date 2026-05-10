using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class KaelAttackStep_3 : IAttackStep
{
    public string AttackStateName => "Attack_3";

    public void Attack(CharacterBase character)
    {
        character.CharacterAnimation.CrossFade(AttackStateName, 0.1f);
        Vector2 lungeDirection = new Vector2(character.transform.forward.x, character.transform.forward.z).normalized;

        character.CharacterMovement.Lunge(lungeDirection);

        PlaySlashEffect();
    }

    private async void PlaySlashEffect()
    {
        await UniTask.Delay(300);
        EventManager.Instance.Notify(GameEvent.OnSlashEffect);
    }
}
