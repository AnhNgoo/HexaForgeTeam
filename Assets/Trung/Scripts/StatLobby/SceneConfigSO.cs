using UnityEngine;

[CreateAssetMenu(fileName = "SceneConfig_DevName", menuName = "Config/Personal Scene Config")]
public class SceneConfigSO : ScriptableObject
{
    [Header("Developer Info")]
    public string devName = "Dev Name";
    
    [Tooltip("BẬT cờ này nếu bạn muốn đè Scene cá nhân lên Scene chung của dự án khi test trên Editor local!")]
    public bool isOverrideMyLocalScene = false;

    [Header("Personal Test Scenes (Nếu bỏ trống sẽ tự quay về Scene Main chung)")]
    public string customLoginScene;
    public string customUiScene;
    public string customLoadingScene;
    public string customLobbyScene;
    public string customRunGameplayScene;
    public string customTutorialScene;
    public string customFinalBossScene;
}