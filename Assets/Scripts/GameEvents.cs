using System;
using System.Diagnostics.CodeAnalysis;

public static class GameEvents
{
    public static event Action PrevLevelRequested;
    public static event Action NextLevelRequested;

    public static event Action LevelCheckRequested;
    public static event Action LevelCompleted;

    [SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
    public static class Invoke
    {
        public static void PrevLevelRequested() => GameEvents.PrevLevelRequested?.Invoke();
        public static void NextLevelRequested() => GameEvents.NextLevelRequested?.Invoke();

        public static void LevelCheckRequested() => GameEvents.LevelCheckRequested?.Invoke();
    }
}
