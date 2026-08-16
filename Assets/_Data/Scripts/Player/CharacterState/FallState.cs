using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallState : ICharacterState
{
    private CharacterBase character;
    public FallState(CharacterBase character)
    {
        this.character = character;
    }
    public void Enter()
    {
        character.CharacterAnimation.CrossFade("Fall", 0.1f);
    }

    public void Exit()
    {

    }

    public void FixedUpdate()
    {

    }

    public void Update()
    {
        if (character.CharacterMovement.IsGrounded)
        {
            character.StateController.ChangeState(new JumpLandState(character));
            return;
        }

        float speed = character.CharacterStat.finalStats.speed;

        Vector3 rotationDirection = new Vector3(character.CharacterMovement.MoveDirection.x,
                                                0f,
                                                character.CharacterMovement.MoveDirection.y);

        if (character.CharacterMovement.MoveDirection != Vector2.zero)
        {
            character.CharacterMovement.MoveAir(character.CharacterMovement.MoveDirection, speed);
            character.CharacterRotate.Rotate(rotationDirection);
            return;
        }
        if (character.CharacterMovement.MoveDirection == Vector2.zero)
        {
            character.CharacterMovement.Stop();
            return;
        }
    }
}
