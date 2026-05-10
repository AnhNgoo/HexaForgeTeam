using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveState : ICharacterState
{
    private CharacterBase character;
    public MoveState(CharacterBase character)
    {
        this.character = character;
    }
    public void Enter()
    {

    }

    public void Exit()
    {
        character.CharacterMovement.Stop();
        character.CharacterAnimation.ResetState();
    }

    public void FixedUpdate()
    {

    }

    public void Update()
    {
        if (character.CharacterMovement.MoveDirection == Vector2.zero)
        {
            character.StateController.ChangeState(new IdleState(character));
            return;
        }

        if (!character.CharacterMovement.IsGrounded && character.CharacterMovement.CC.velocity.y < character.CharacterMovement.FallThreshold)
        {
            character.StateController.ChangeState(new FallState(character));
            return;
        }

        float xAbs = Mathf.Abs(character.JoystickInput.x);
        float yAbs = Mathf.Abs(character.JoystickInput.y);
        float inputSpeed = Mathf.Max(xAbs, yAbs);
        float speed = character.CharacterData.stats.speed;

        Vector3 rotationDirection = new Vector3(character.CharacterMovement.MoveDirection.x,
                                                0f,
                                                character.CharacterMovement.MoveDirection.y);

        if (inputSpeed > 0 && inputSpeed <= character.CharacterMovement.WalkThreshold)
        {
            character.CharacterMovement.Walk(character.CharacterMovement.MoveDirection, speed);
            character.CharacterAnimation.CrossFadeOnshot("Walk", 0.1f);
            character.CharacterRotate.Rotate(rotationDirection);
            return;
        }

        if (inputSpeed > character.CharacterMovement.WalkThreshold && inputSpeed <= character.CharacterMovement.RunThreshold)
        {
            character.CharacterMovement.Run(character.CharacterMovement.MoveDirection, speed);
            character.CharacterAnimation.CrossFadeOnshot("Run", 0.1f);
            character.CharacterRotate.Rotate(rotationDirection);
            return;
        }
        if (inputSpeed > character.CharacterMovement.RunThreshold && inputSpeed <= character.CharacterMovement.SprintThreshold)
        {
            character.CharacterMovement.Sprint(character.CharacterMovement.MoveDirection, speed);
            character.CharacterAnimation.CrossFadeOnshot("Sprint", 0.1f);
            character.CharacterRotate.Rotate(rotationDirection);
            return;
        }
    }
}
