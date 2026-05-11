using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
public class JumpLandState : ICharacterState
{
    private CharacterBase character;
    public JumpLandState(CharacterBase character)
    {
        this.character = character;
    }
    public void Enter()
    {
        character.CharacterMovement.JumpLanding = true;
        JumpLand();
    }

    public void Exit()
    {
        character.CharacterMovement.JumpLanding = false;
    }

    public void FixedUpdate()
    {

    }

    public void Update()
    {

    }

    private async void JumpLand()
    {
        character.CharacterMovement.Stop();
        character.CharacterAnimation.CrossFade("Jump_Land", 0.1f);
        await UniTask.Delay(1000);
        character.StateController.ChangeState(new IdleState(character));
    }
}
