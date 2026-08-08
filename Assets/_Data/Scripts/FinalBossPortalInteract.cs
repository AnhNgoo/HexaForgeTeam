using UnityEngine;

public class FinalBossPortalInteract : MonoBehaviour
{
    public void OnInteract()
    {
        if (RunManager.Instance == null)
        {
            Debug.LogError("[FinalBossPortal] Không tìm thấy RunManager.");
            return;
        }

        RunManager.Instance.EnterFinalBoss();
    }
}