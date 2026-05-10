using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : ICharacterState
{
    private CharacterBase character;

    public IdleState(CharacterBase character)
    {
        this.character = character;
    }

    public void Enter()
    {
        character.CharacterAnimation.CrossFade("Idle", 0.1f);
        character.CharacterMovement.Stop();
    }

    public void Exit()
    {

    }

    public void FixedUpdate()
    {

    }

    public void Update()
    {
        if (character.CharacterMovement.MoveDirection != Vector2.zero)
        {
            character.StateController.ChangeState(new MoveState(character));
            return;
        }

        if (!character.CharacterMovement.IsGrounded && character.CharacterMovement.CC.velocity.y < character.CharacterMovement.FallThreshold)
        {
            character.StateController.ChangeState(new FallState(character));
            return;
        }
    }
}


