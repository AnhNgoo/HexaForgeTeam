using UnityEngine;

public class LevelStation : MonoBehaviour
{
    [SerializeField] private LevelUpMenu levelUpMenu;

    public void OnInteract()
    {
        if (levelUpMenu == null)
            return;

        levelUpMenu.Open();

        Debug.Log("Đã mở Level Station");
    }
}