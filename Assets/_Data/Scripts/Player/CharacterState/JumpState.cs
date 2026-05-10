using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class JumpState : ICharacterState
{
    private bool hasJumped = false;
    private CharacterBase character;
    public JumpState(CharacterBase character)
    {
        this.character = character;
    }
    public void Enter()
    {
        character.CharacterAnimation.CrossFade("Jump", 0.1f);
        character.CharacterMovement.Jump();
        CheckJumped();
    }

    public void Exit()
    {
        character.CharacterAnimation.ResetState();
    }

    public void FixedUpdate()
    {

    }

    public void Update()
    {
        if (character.CharacterMovement.IsGrounded && hasJumped) //Chuyển về IdleState khi đã chạm đất
        {
            character.StateController.ChangeState(new IdleState(character));
            return;
        }

        if (!character.CharacterMovement.IsGrounded && character.CharacterMovement.Rb.velocity.y < -0.1f)
        {
            character.StateController.ChangeState(new FallState(character));
            return;
        }

        float speed = character.CharacterData.stats.speed;

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

    private async void CheckJumped()
    {
        await UniTask.Delay(500);
        hasJumped = true;
    }
}