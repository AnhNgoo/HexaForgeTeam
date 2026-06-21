using UnityEngine;

public class ResetDataManager : MonoBehaviour
{
    public void ResetAllData()
{
    if (CharacterManager.Instance != null)
    {
        CharacterManager.Instance
            .ResetCharacterData();
    }

    if (SaveLoadManager.Instance != null)
    {
        SaveLoadManager.Instance
            .DeleteSave();
    }

    Debug.Log(
        "All Data Reset");

#if UNITY_EDITOR
    UnityEditor.EditorApplication.isPlaying =
        false;
#endif
}
}