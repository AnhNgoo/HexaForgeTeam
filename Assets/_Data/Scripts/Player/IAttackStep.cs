using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAttackStep
{
    string AttackStateName { get; }
    void Attack(CharacterBase character);
}
