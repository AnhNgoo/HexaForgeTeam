using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICharacterState
{
    void Enter();
    void Exit();
    void Update();
    void FixedUpdate();
}
