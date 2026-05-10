using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class Cooldown
{
    private float timer = 0f;
    public bool IsOnCooldown => timer > 0f;

    public Cooldown()
    {
        timer = 0f;
    }
    public void StartCooldown(float cooldown)
    {
        timer = cooldown;
        CooldownTimer();
    }

    private async void CooldownTimer()
    {
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            await UniTask.Yield();
        }
        timer = 0f;
    }
}
