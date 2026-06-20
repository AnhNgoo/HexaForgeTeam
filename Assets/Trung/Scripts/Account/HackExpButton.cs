using UnityEngine;

public class HackExpButton :
    MonoBehaviour
{
    public void Add100Exp()
    {
        if (AccountLevelManager.Instance == null)
        {
            return;
        }

        AccountLevelManager.Instance
            .AddExp(100);
    }
}