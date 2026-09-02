using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class DeathState : ICharacterState
{
    private CharacterBase character;

    public DeathState(CharacterBase character)
    {
        this.character = character;
    }

    public void Enter()
    {
        character.CharacterAnimation.CrossFade("Death", 0.1f);
        character.CharacterMovement.Stop();
        character.CharacterSound.Play(character.CharacterSound.deathSoundEffect);
        HandleDeath();
    }

    public void Exit()
    {

    }

    public void FixedUpdate()
    {

    }

    public void Update()
    {

    }

    private async void HandleDeath()
    {
        await UniTask.WaitUntil(() => character.CharacterAnimation.GetAnimationTime("Death") >= 0.5f);
        character.DissolveEffect.PlayDissolveEffect(3.0f);

        EventManager.Notify(GameEvent.OnPlayerDeath);
    }
}
