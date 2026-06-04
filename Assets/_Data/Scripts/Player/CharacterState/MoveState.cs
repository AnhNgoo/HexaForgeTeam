using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveState : ICharacterState
{
    private float strafeThreshold = 0.8f; // Ngưỡng để xác định di chuyển chéo hay thẳng
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
        character.CharacterAnimation.ResetState();
    }

    public void FixedUpdate()
    {

    }

    public void Update()
    {
        character.LookAtTarget();

        if (character.CharacterInput.moveInput == Vector2.zero)
        {
            character.StateController.ChangeState(new IdleState(character));
            character.CharacterMovement.Stop();
            return;
        }

        if (character.CharacterInput.Skill_1)
        {
            character.Skill_1();
            return;
        }

        if (character.CharacterInput.Skill_2)
        {
            character.Skill_2();
            return;
        }

        if (character.CharacterInput.Attack)
        {
            character.Attack();
            return;
        }

        if (character.CharacterInput.HealthRecovery)
        {
            character.StateController.ChangeState(new HealthRecoveryState(character));
            return;
        }

        if (character.CharacterInput.Dodge)
        {
            character.Dodge();
            return;
        }

        if (character.CharacterInput.Jump && character.CharacterMovement.IsGrounded)
        {
            character.StateController.ChangeState(new JumpState(character));
            return;
        }

        if (!character.CharacterLockTarget.IsLockingTarget)
        {
            MoveNormal();
            return;
        }

        if (character.CharacterLockTarget.IsLockingTarget)
        {
            MoveLockTarget();
            return;
        }
    }

    private void MoveNormal()
    {
        float xAbs = Mathf.Abs(character.CharacterInput.moveInput.x);
        float yAbs = Mathf.Abs(character.CharacterInput.moveInput.y);
        float inputSpeed = Mathf.Max(xAbs, yAbs);// Giá trị lớn nhất giữa x và y của joystick để xác định tốc độ di chuyển
        float speed = character.CharacterData.stats.speed;

        Vector3 rotationDirection = new Vector3(character.CharacterMovement.MoveDirection.x,
                                                0f,
                                                character.CharacterMovement.MoveDirection.y);

        if (inputSpeed > 0 && inputSpeed <= character.CharacterMovement.WalkThreshold)
        {
            character.CharacterMovement.Walk(character.CharacterMovement.MoveDirection, speed);
            character.CharacterAnimation.CrossFadeOneshot("Walk", 0.1f);
            character.CharacterRotate.Rotate(rotationDirection);
            return;
        }

        if (inputSpeed > character.CharacterMovement.WalkThreshold && inputSpeed <= character.CharacterMovement.RunThreshold)
        {
            character.CharacterMovement.Run(character.CharacterMovement.MoveDirection, speed);
            character.CharacterAnimation.CrossFadeOneshot("Run", 0.1f);
            character.CharacterRotate.Rotate(rotationDirection);
            return;
        }
        if (inputSpeed > character.CharacterMovement.RunThreshold && inputSpeed <= character.CharacterMovement.SprintThreshold)
        {
            character.CharacterMovement.Sprint(character.CharacterMovement.MoveDirection, speed);
            character.CharacterAnimation.CrossFadeOneshot("Sprint", 0.1f);
            character.CharacterRotate.Rotate(rotationDirection);
            return;
        }
    }

    private void MoveLockTarget()
    {
        float x = character.CharacterInput.moveInput.x; // Hướng đi ngang
        float y = character.CharacterInput.moveInput.y; // Hướng đi dọc
        float yAbs = Mathf.Abs(y); // Ngưỡng y để xác định di chuyển chéo hay thẳng
        float speed = character.CharacterData.stats.speed;

        if (x < 0 && yAbs < strafeThreshold)
        {
            character.CharacterMovement.Run(character.CharacterMovement.MoveDirection, speed);
            character.CharacterAnimation.CrossFadeOneshot("Run_Strafe_Left", 0.1f);
            return;
        }

        if (x > 0 && yAbs < strafeThreshold)
        {
            character.CharacterMovement.Run(character.CharacterMovement.MoveDirection, speed);
            character.CharacterAnimation.CrossFadeOneshot("Run_Strafe_Right", 0.1f);
            return;
        }
        if (y < 0)
        {
            character.CharacterMovement.Run(character.CharacterMovement.MoveDirection, speed);
            character.CharacterAnimation.CrossFadeOneshot("Run_Backward", 0.1f);
            return;
        }

        if (y > 0)
        {
            character.CharacterMovement.Run(character.CharacterMovement.MoveDirection, speed);
            character.CharacterAnimation.CrossFadeOneshot("Run", 0.1f);
            return;
        }
    }
}
