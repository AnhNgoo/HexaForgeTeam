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

        if (character.CharacterMovement.MoveDirection == Vector2.zero)
        {
            character.StateController.ChangeState(new IdleState(character));
            character.CharacterMovement.Stop();
            return;
        }

        //Chuyển về FallState nếu đang ở trên không và bắt đầu rơi
        if (!character.CharacterMovement.IsGrounded && character.CharacterMovement.CC.velocity.y < character.CharacterMovement.FallThreshold)
        {
            character.StateController.ChangeState(new FallState(character));
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
        float xAbs = Mathf.Abs(character.JoystickInput.x);
        float yAbs = Mathf.Abs(character.JoystickInput.y);
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
        float x = character.JoystickInput.x; // Hướng đi ngang
        float y = character.JoystickInput.y; // Hướng đi dọc
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
