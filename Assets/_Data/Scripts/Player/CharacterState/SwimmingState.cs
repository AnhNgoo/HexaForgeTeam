using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwimmingState : ICharacterState
{
    private readonly CharacterBase character;

    public SwimmingState(CharacterBase character)
    {
        this.character = character;
    }

    public void Enter()
    {
        character.CharacterMovement.SetSwimmingState(true);
        character.CharacterAnimation.CrossFadeOneshot("TreadingWater", 0.1f);
        character.CharacterMovement.Stop();
    }

    public void Exit()
    {
        character.CharacterMovement.SetSwimmingState(false);
        character.CharacterAnimation.ResetState();
    }

    public void FixedUpdate()
    {

    }

    public void Update()
    {
        bool isInWater = character.IsInWaterVolume;
        bool isUnderWater = character.IsSwimmingCandidate();
        bool isOnGround = character.CharacterMovement.IsGrounded;

        if (!isInWater || (isOnGround && !isUnderWater))
        {
            if (character.CharacterInput.MoveInput != Vector2.zero)
            {
                if (isOnGround)
                {
                    character.StateController.ChangeState(new MoveState(character));
                }
                else
                {
                    character.StateController.ChangeState(new FallState(character));
                }
            }
            else if (isOnGround)
            {
                character.StateController.ChangeState(new IdleState(character));
            }
            else
            {
                character.StateController.ChangeState(new FallState(character));
            }
            return;
        }

        float waterLevel = character.WaterLevel;
        if (float.IsNaN(waterLevel) || float.IsInfinity(waterLevel))
        {
            character.StateController.ChangeState(new FallState(character));
            return;
        }

        float targetY = waterLevel - 0.65f;
        Vector3 pos = character.transform.position;
        pos.y = Mathf.Lerp(pos.y, targetY, Time.deltaTime * 8f);
        character.transform.position = pos;

        if (character.CharacterInput.MoveInput == Vector2.zero)
        {
            character.CharacterMovement.Stop();
            character.CharacterAnimation.CrossFadeOneshot("TreadingWater", 0.1f);
            return;
        }

        Move();
    }

    private void Move()
    {
        float speed = character.CharacterStat.finalStats.speed;

        Vector3 rotationDirection = new Vector3(character.CharacterMovement.MoveDirection.x,
                                                0f,
                                                character.CharacterMovement.MoveDirection.y);

        character.CharacterMovement.Swim(character.CharacterMovement.MoveDirection, speed);
        character.CharacterRotate.Rotate(rotationDirection);
        character.CharacterAnimation.CrossFadeOneshot("Swimming", 0.1f);
    }
}
