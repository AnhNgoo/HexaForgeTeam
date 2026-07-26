using UnityEngine;

public class GoldTest : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            GoldManager.Instance?.AddGold(100);
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            GoldManager.Instance?.RemoveGold(50);
        }
    }
}