using System;

public static class GameEventManager
{
    // Sự kiện khi kho ngọc hoặc trang bị ngọc thay đổi
    public static event Action OnRuneDataChanged;
    public static void TriggerRuneDataChanged() => OnRuneDataChanged?.Invoke();

    // Sự kiện khi chỉ số nhân vật thay đổi
    public static event Action OnCharacterStatsRecalculated;
    public static void TriggerCharacterStatsRecalculated() => OnCharacterStatsRecalculated?.Invoke();

    // Sự kiện khi đổi nhân vật xem thử
    public static event Action<CharacterType> OnSelectedCharacterChanged;
    public static void TriggerSelectedCharacterChanged(CharacterType type) => OnSelectedCharacterChanged?.Invoke(type);
}