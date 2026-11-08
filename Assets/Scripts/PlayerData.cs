using System;

[Serializable]
public sealed class PlayerData
{
    private const int LevelCount = 12;
    private const int LastLevel = LevelCount - 1;
    private const int AllLevelsCompletedMask = (1 << LevelCount) - 1;

    public int Progress; // 12 levels, 1 bit each, 20 bits unused

    public bool HasCompletedAllLevels => (Progress & AllLevelsCompletedMask) == AllLevelsCompletedMask;

    public int FirstIncompleteLevel
    {
        get
        {
            for (var i = 0; i <= LastLevel; i++)
                if (!HasCompletedLevel(i))
                    return i;

            return 0;
        }
    }

    public bool HasCompletedLevel(int level)
    {
        var mask = 1 << level;
        return (Progress & mask) == mask;
    }
}
