using UnityEngine;

public class LevelTest : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            LevelManager.Instance?.LevelUp();
        }
    }
}