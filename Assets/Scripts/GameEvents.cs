using System;
using System.Diagnostics.CodeAnalysis;

public static class GameEvents
{
    public static event Action PrevLevelRequested;
    public static event Action NextLevelRequested;

    public static event Action LevelCheckRequested;
    public static event Action LevelCompleted;

    public static event Action PlayerDataUpdateRequested;
    public static event Action PlayerDataDeleteRequested;

    public static event Action<bool> InputStateChangeRequested;

    [SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
    public static class Invoke
    {
        public static void PrevLevelRequested() => GameEvents.PrevLevelRequested?.Invoke();
        public static void NextLevelRequested() => GameEvents.NextLevelRequested?.Invoke();

        public static void LevelCheckRequested() => GameEvents.LevelCheckRequested?.Invoke();
        public static void PlayerDataUpdateRequested() => GameEvents.PlayerDataUpdateRequested?.Invoke();
        public static void PlayerDataDeleteRequested() => GameEvents.PlayerDataDeleteRequested?.Invoke();

        public static void InputStateChangeRequested(bool isEnabled) =>
            GameEvents.InputStateChangeRequested?.Invoke(isEnabled);

    }
}
