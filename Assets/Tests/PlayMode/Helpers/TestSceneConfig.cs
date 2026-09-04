namespace DuskBlade.Tests
{
    public static class TestSceneConfig
    {
        public static readonly string[] GameFlowScenePaths =
        {
            "Assets/_Data/Scenes/ReleaseGameScenes/LoginGame.unity",
            "Assets/_Data/Scenes/ReleaseGameScenes/UIGame.unity",
            "Assets/_Data/Scenes/ReleaseGameScenes/LobbyMainGame.unity",
            "Assets/_Data/Scenes/ReleaseGameScenes/RunGame.unity",
            "Assets/_Data/Scenes/Khoa/Run Scene Khoa.unity",
            "Assets/_Data/Scenes/ReleaseGameScenes/FinalBosArenaGame.unity"
        };

        public static string LoginScenePath { get { return GameFlowScenePaths[0]; } }
        public static string UiScenePath { get { return GameFlowScenePaths[1]; } }
        public static string LobbyScenePath { get { return GameFlowScenePaths[2]; } }
        public static string RunScenePath { get { return GameFlowScenePaths[3]; } }
        public static string RunScene2Path { get { return GameFlowScenePaths[4]; } }
        public static string FinalBossScenePath { get { return GameFlowScenePaths[5]; } }

        public static string GameplayScenePath
        {
            get { return RunScenePath; }
        }
    }
}
