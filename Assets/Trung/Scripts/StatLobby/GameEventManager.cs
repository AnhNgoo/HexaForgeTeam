using System;

public static class GameEventManager
{
    public static event Action OnRuneDataChanged;
    public static void TriggerRuneDataChanged() => OnRuneDataChanged?.Invoke();

    public static event Action OnCharacterStatsRecalculated;
    public static void TriggerCharacterStatsRecalculated() => OnCharacterStatsRecalculated?.Invoke();

    public static event Action<CharacterType> OnSelectedCharacterChanged;
    public static void TriggerSelectedCharacterChanged(CharacterType type) => OnSelectedCharacterChanged?.Invoke(type);

    public static event Action<int> OnGachaRolled;
    public static void TriggerGachaRolled(int rollCount) => OnGachaRolled?.Invoke(rollCount);

    public static event Action<int, bool> OnEnemyKilled;
    public static void TriggerEnemyKilled(int killCount, bool isBoss) => OnEnemyKilled?.Invoke(killCount, isBoss);

    public static event Action OnTutorialCompleted;
    public static void TriggerTutorialCompleted() => OnTutorialCompleted?.Invoke();
    public static event System.Action<bool> OnRunCompleted;

    public static void TriggerRunCompleted(bool isVictory)
    {
        OnRunCompleted?.Invoke(isVictory);
    }
}