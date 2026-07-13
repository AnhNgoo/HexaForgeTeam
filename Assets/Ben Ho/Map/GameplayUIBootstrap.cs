using System.Collections;
using UnityEngine;

public class GameplayUIBootstrap : MonoBehaviour
{
    [SerializeField] private bool openGameplayMenuOnStart = true;

    private IEnumerator Start()
    {
        yield return null;

        if (UIManager.Instance == null)
        {
            Debug.LogError("Missing UIManager in gameplay scene.");
            yield break;
        }

        UIManager.Instance.InitUI();

        if (openGameplayMenuOnStart)
            UIManager.Instance.ChangeMenu(MenuType.GameplayMenu);

        Debug.Log("Gameplay UI started. Current menu: " + UIManager.Instance.CurrentMenuType);
    }
}